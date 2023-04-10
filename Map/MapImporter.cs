using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace GodotVTT
{
    public partial class MapImporter : Node
    {
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private const int _occluderMask = 1 | 2 | 4 | (1 << 19);

        private PackedScene _mapPrototype;

        [Export]
        public Texture2D DefaultLightTexture = null;

        public override void _Ready()
        {
            _jsonOptions.Converters.Add(new HtmlColorJsonConverter());
            _jsonOptions.Converters.Add(new Vector2JsonConverter());

            _mapPrototype = ResourceLoader.Load<PackedScene>("res://Map/map_base.tscn");
        }

        public MapBase LoadMapFromFile(string path, float wallWidth = 10f)
        {
            var map = _mapPrototype.Instantiate<MapBase>();

            using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
            var data = JsonSerializer.Deserialize<Dd2Vtt>(file.GetAsText(), _jsonOptions);

            map.Ready += () => LoadMap(map, data, wallWidth);
            return map;
        }

        private void LoadMap(MapBase map, Dd2Vtt data, float wallWidth)
        {
            var image = new Image();
            image.LoadPngFromBuffer(data.image);
            map.SetMapTexture(ImageTexture.CreateFromImage(image));

            map.MapResolution = data.resolution;
            map.MapScale = data.resolution.pixels_per_grid;

            //var camera = map.GetNode<MainCamera>("MainCamera");
            //camera.MapScale = new Vector2(data.resolution.pixels_per_grid, data.resolution.pixels_per_grid);
            //camera.SetZoom(1.0f);

            SetOccluders(map, data.resolution, data.line_of_sight, wallWidth);
            SetLights(map, data.lights);
        }

        private void SetOccluders(MapBase map, Resolution resolution, List<List<Vector2>> points, float wallWidth)
        {
            var navRegion = new NavigationRegion2D
            {
                Name = $"{map.Name}Nav",
            };
            navRegion.AddToGroup("nav_regions");

            var helper = new CompositePolygon();

            foreach (var set in points)
            {
                var occluder = CreateOccluder(set, map.MapScale);
                map.MainGroup.AddChild(occluder);
                occluder.Owner = map;

                helper = AddWall(helper, set, wallWidth, map.MapScale);
            }

            map.SetNavigation(helper);
        }

        private LightOccluder2D CreateOccluder(List<Vector2> points, float scale = 1f)
        {
            return new LightOccluder2D
            {
                Occluder = new OccluderPolygon2D
                {
                    Polygon = points.Select(p => p * scale).ToArray(),
                },
                OccluderLightMask = _occluderMask
            };
        }

        private void SetLights(MapBase map, List<Light> lights)
        {
            foreach (var light in lights)
            {
                var node = CreateLight(light, map.MapScale);
                map.MainGroup.AddChild(node);
                node.Owner = map;
            }
        }

        private Light2D CreateLight(Light light, float scale = 1f)
        {
            var node = new PointLight2D();
            node.Position = light.position * scale;
            node.Color = light.color;
            node.ShadowEnabled = light.shadows;
            node.Energy = light.intensity;

            node.Texture = DefaultLightTexture;
            node.TextureScale = light.range / 512f * scale;    // TODO: Remove Magic Number

            return node;
        }

        private CompositePolygon AddWall(CompositePolygon helper, List<Vector2> points, float wallWidth, float scale = 1f)
        {
            if (points.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(points));
            }

            var strip = Geometry2D.OffsetPolyline(points.Select(p => p * scale).ToArray(), wallWidth, Geometry2D.PolyJoinType.Miter, Geometry2D.PolyEndType.Square);

            helper.UnionWith(strip);

            return helper;
        }
    }
}