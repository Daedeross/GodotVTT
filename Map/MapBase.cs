using Godot;
using GodotVTT.Interfaces;
using GodotVTT.Model.Dto;
using System;
using System.Linq;
using static GodotVTT.MapImporter;

namespace GodotVTT
{
    public partial class MapBase : Node2D, IHaveId
    {
        private readonly Color _debugVoidColor = new Color("Red");
        private readonly Color _debugSolidColor = new Color("Green");

        private ControlState _controlState = ControlState.None;

        private CanvasItem _instructions;

        private Sprite2D _fogOfWar;
        private Sprite2D _mapSprite;
        private SubViewport _vbl;

        private bool _plop = false;

        private CompositePolygon _navigationZones;
        private NavigationRegion2D _navigationRegion;

        public Resolution MapResolution { get; set; }

        [Export]
        public bool DrawNavigation { get; set; } = true;

        [Export]
        public PackedScene TokenPrototype;

        private float _mapScale;
        [Export]
        public float MapScale
        {
            get => _mapScale;
            set
            {
                if (_mapScale != value)
                {
                    _mapScale = value;

                    var navs = GetTree()?.GetNodesInGroup("nav_regions") ?? new Godot.Collections.Array<Node>();
                    foreach (var nav in navs)
                    {
                        if (nav is NavigationRegion2D region)
                        {
                            region.NavigationPolygon.GetNavigationMesh().AgentRadius = _mapScale * 0.45f;
                        }
                    }
                }
            }
        }

        private CanvasGroup _mainGroup;

        public CanvasGroup MainGroup
        {
            get
            {
                if (_mainGroup is null)
                {
                    _mainGroup = GetNode<CanvasGroup>("MainGroup");
                }

                return _mainGroup;
            }
        }

        public Guid Id { get; set; }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            //_instructions = GetNode<CanvasItem>("HUD/Instructions");
            _fogOfWar = GetNode<Sprite2D>("FogGroup/FogOfWar");
            _mapSprite = GetNode<Sprite2D>("MainGroup/MapSprite");
            _vbl = GetNode<SubViewport>("VBLRender");

            var navs = GetTree().GetNodesInGroup("nav_regions");
            foreach (var nav in navs)
            {
                if (nav is NavigationRegion2D region)
                {
                    var mesh = region.NavigationPolygon.GetNavigationMesh();
                    mesh.AgentRadius = _mapScale * 0.45f;
                }
            }
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }

        public override void _Draw()
        {
            base._Draw();

            if (DrawNavigation && _navigationZones != null)
            {
                foreach (var poly in _navigationZones.Solids)
                {
                    DrawPolyline(poly, _debugSolidColor, 2f, true);
                    DrawLine(poly.Last(), poly.First(), _debugSolidColor, 2f, true);
                }

                foreach (var poly in _navigationZones.Voids)
                {
                    DrawPolyline(poly, _debugVoidColor, 2f, true);
                    DrawLine(poly.Last(), poly.First(), _debugVoidColor, 2f, true);
                }
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventKey eventKey)
            {
                if(eventKey.Keycode == Key.Escape && eventKey.Pressed)
                {
                    StopPlop();
                }
            }
            else if (@event is InputEventMouseButton eventMouseButton)
            {
                if (_plop && eventMouseButton.IsActionPressed(Actions.Primary))
                {
                    Rpc(MethodName.SpawnToken, GetLocalMousePosition());
                    StopPlop();
                }
                else if (eventMouseButton.IsActionPressed(Actions.Secondary))
                {
                    _controlState |= ControlState.Secondary;
                }
                else if (eventMouseButton.IsActionReleased(Actions.Secondary))
                {
                    _controlState &= (~ControlState.Secondary);
                }
            }
            else if (@event is InputEventMouseMotion eventMouseMotion)
            {
                if (_controlState == ControlState.Secondary)
                {
                    Position += eventMouseMotion.Relative;
                }
            }
        }

        public void AddToken()
        {
            _plop= true;
            _instructions.Show();
        }

        public void StopPlop()
        {
            _plop = false;
            _instructions.Hide();
        }

        [Rpc(CallLocal = true)]
        public void SpawnToken(Vector2 pos)
        {
            var token = TokenPrototype.Instantiate<TokenBase>();

            token.Position = pos;

            AddChild(token);
            token.Owner = this;
        }

        public void SetMapTexture(Texture2D texture)
        {
            var size = texture.GetSize();
            _vbl.Size = new Vector2I(Mathf.FloorToInt( size.X), Mathf.FloorToInt(size.Y));
            _mapSprite.Texture = texture;
        }

        public MapDto ToDto()
        {
            return new MapDto
            {
                Id = Id,
                Name = Name,
            };
        }

        #region Navigation

        public void SetNavigation(CompositePolygon newZones)
        {
            EnsureNavigationRegion();

            if (_navigationZones is not null)
            {
                _navigationRegion.NavigationPolygon.ClearPolygons();
                _navigationRegion.NavigationPolygon.ClearOutlines();
            }
            else
            {
                _navigationRegion.NavigationPolygon?.Dispose();
                _navigationRegion.NavigationPolygon = new NavigationPolygon();
            }

            _navigationZones = newZones;

            CalculateNavPolygons();
        }

        private NavigationRegion2D EnsureNavigationRegion()
        {
            if (_navigationRegion is null)
            {
                var child = this.GetChildren<NavigationRegion2D>().SingleOrDefault();
                if (child is null)
                {
                    child = new NavigationRegion2D();
                    child.Name = $"{Name}Nav";
                    child.AddToGroup("nav_regions");
                    AddChild(child);
                    child.Owner = this;
                }

                _navigationRegion = child;
            }

            return _navigationRegion;
        }

        private void CalculateNavPolygons()
        {
            var o = MapResolution.map_origin * MapScale;
            var p = MapResolution.map_size * MapScale;

            var outline = new Vector2[]
            {
                new Vector2(Mathf.Min(o.X, p.X), Mathf.Min(o.Y, p.Y)),
                new Vector2(Mathf.Max(o.X, p.X), Mathf.Min(o.Y, p.Y)),
                new Vector2(Mathf.Max(o.X, p.X), Mathf.Max(o.Y, p.Y)),
                new Vector2(Mathf.Min(o.X, p.X), Mathf.Max(o.Y, p.Y))
            };

            var navPoly = _navigationRegion.NavigationPolygon;
            navPoly.AddOutline(outline);
            foreach (var poly in _navigationZones.Voids.Union(_navigationZones.Solids))
            {
                navPoly.AddOutline(poly);
            }

            navPoly.MakePolygonsFromOutlines();
        }

        public void AddPolygonToNavigation(Vector2[] polygon)
        {
            _navigationZones.Add(polygon);
            CalculateNavPolygons();
            if (DrawNavigation)
            {
                QueueRedraw();
            }
        }

        #endregion
    }
}
