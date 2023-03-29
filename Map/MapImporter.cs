using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GodotVTT
{
    public partial class MapImporter : Node
    {
        internal class NavHelper
        {
            public List<Vector2[]> Solids = new();
            public List<Vector2[]> Voids = new();
        }

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

            //var map = LoadMap("res://");
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

            var helper = new NavHelper();

            var o = resolution.map_origin * map.MapScale;
            var p = resolution.map_size * map.MapScale;

            var outline = new Vector2[]
            {
                new Vector2(Mathf.Min(o.X, p.X), Mathf.Min(o.Y, p.Y)),
                new Vector2(Mathf.Max(o.X, p.X), Mathf.Min(o.Y, p.Y)),
                new Vector2(Mathf.Max(o.X, p.X), Mathf.Max(o.Y, p.Y)),
                new Vector2(Mathf.Min(o.X, p.X), Mathf.Max(o.Y, p.Y))
            };

            foreach (var set in points)
            {
                var occluder = CreateOccluder(set, map.MapScale);
                map.MainGroup.AddChild(occluder);
                occluder.Owner = map;

                helper = AddWall(helper, set, wallWidth, map.MapScale);
            }

            var navPoly = new NavigationPolygon();
            navPoly.AddOutline(outline);
            foreach (var poly in helper.Voids.Union(helper.Solids))
            {
                navPoly.AddOutline(poly);
            }

            navPoly.MakePolygonsFromOutlines();
            navRegion.NavigationPolygon = navPoly;

            map.AddChild(navRegion);

            // TODO: TESTING
            map.TestPolys = helper;
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

        private NavHelper AddWall(NavHelper helper, List<Vector2> points, float wallWidth, float scale = 1f)
        {
            if (points.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(points));
            }

            var strip = Geometry2D.OffsetPolyline(points.Select(p => p * scale).ToArray(), 10f, Geometry2D.PolyJoinType.Miter, Geometry2D.PolyEndType.Square);

            //var p1 = Geometry2D.OffsetPolyline(new Vector2[] { new Vector2(0f, 1f), new Vector2(0f, 3f), new Vector2(3f, 3f), new Vector2(3f, 1f) }, 0.1f, Geometry2D.PolyJoinType.Miter, Geometry2D.PolyEndType.Square );
            //var p2 = Geometry2D.OffsetPolyline(new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, -3f), new Vector2(3f, -3f), new Vector2(3f, 0f) }, 0.1f, Geometry2D.PolyJoinType.Miter, Geometry2D.PolyEndType.Square );

            //var ps = Geometry2D.MergePolygons(p1[0], p2[0]);

            //var line = p1.Append(new Vector2(0f, 0f)).ToArray();
            //var tps = Geometry2D.OffsetPolyline(line, 1.0f, Geometry2D.PolyJoinType.Miter);

            foreach (var poly in strip)
            {
                // FYI clockwise = hole
                helper = UnionWithSet(helper, poly);
            }

            return helper;
        }

        private static NavHelper UnionWithSet(NavHelper old, Vector2[] newPoly)
        {
            var result = new NavHelper();
            var solids = new Queue<Vector2[]>(old.Solids);
            var voids = new Queue<Vector2[]>(old.Voids);

            var incomming = new Queue<Vector2[]>();
            incomming.Enqueue(newPoly);

            while (incomming.TryDequeue(out var currentPoly))
            {
                // clockwise = hole
                bool isHole = Geometry2D.IsPolygonClockwise(currentPoly);
                var currentSolids = solids;
                var currentVoids = voids;
                solids = new Queue<Vector2[]>();
                voids = new Queue<Vector2[]>();

                while (currentPoly != null && currentSolids.TryDequeue(out var testSolid))
                {
                    var merged = isHole
                        ? Geometry2D.ClipPolygons(testSolid, currentPoly)
                        : Geometry2D.MergePolygons(testSolid, currentPoly);

                    if (merged.Count == 1)
                    {
                        incomming.Enqueue(merged[0]);
                        currentPoly = null;
                        continue;
                    }

                    bool changed = true;
                    foreach (var poly in merged)
                    {
                        if (!poly.Except(currentPoly).Any())
                        {
                            changed = false;
                            continue;
                        }

                        if (poly.Except(testSolid).Any() && incomming.Any())
                        {
                            if (Geometry2D.IsPolygonClockwise(poly))
                            {
                                voids.Enqueue(poly);
                            }
                            else
                            {
                                solids.Enqueue(poly);
                            }
                        }
                        else
                        {
                            if (Geometry2D.IsPolygonClockwise(poly))
                            {
                                result.Voids.Add(poly);
                            }
                            else
                            {
                                result.Solids.Add(poly);
                            }
                        }
                    }
                    if (changed)
                    {
                        currentPoly = null;
                    }
                }
                while (currentPoly != null && currentVoids.TryDequeue(out var testVoid))
                {
                    var merged = isHole
                        ? Geometry2D.ClipPolygons(testVoid, currentPoly)
                        : Geometry2D.MergePolygons(testVoid, currentPoly);

                    if (merged.Count == 1)
                    {
                        incomming.Enqueue(merged[0]);
                        currentPoly = null;
                        continue;
                    }

                    bool changed = true;
                    foreach (var poly in merged)
                    {
                        if (!poly.Except(currentPoly).Any())
                        {
                            changed = true;
                            continue;
                        }

                        if (poly.Except(testVoid).Any() && incomming.Any())
                        {
                            if (Geometry2D.IsPolygonClockwise(poly))
                            {
                                voids.Enqueue(poly);
                            }
                            else
                            {
                                solids.Enqueue(poly);
                            }
                        }
                        else
                        {
                            result.Voids.Add(poly);
                        }
                    }

                    if (changed)
                    {
                        currentPoly = null;
                    }
                }

                if (currentPoly != null)
                {
                    if (Geometry2D.IsPolygonClockwise(currentPoly))
                    {
                        result.Voids.Add(currentPoly);
                    }
                    else
                    {
                        result.Solids.Add(currentPoly);
                    }
                }
            }

            return result;
        }
    }
}