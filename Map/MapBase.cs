using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GodotVTT.MapImporter;

namespace GodotVTT
{
    public partial class MapBase : Node2D
    {
        private readonly Color _debugColor = new Color("Red");

        private ControlState _controlState = ControlState.None;

        private CanvasItem _instructions;

        private Sprite2D _fogOfWar;
        private Sprite2D _mapSprite;
        private SubViewport _vbl;

        private bool _plop = false;

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

        internal NavHelper TestPolys;

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

            //var navs = GetTree().GetNodesInGroup("nav_regions");
            //foreach (var nav in navs)
            //{
            //    if (nav is NavigationRegion2D region)
            //    {
            //        foreach (var outline in region.NavigationPolygon.Outlines)
            //        {
            //            DrawPolyline(outline, _debugColor, 1f);
            //            DrawLine(outline.Last(), outline.First(), _debugColor, 1f);
            //        }
            //    }
            //}


            //if (TestPolys != null)
            //{
            //    var green = new Color("Green");
            //    foreach (var poly in TestPolys.Solids)
            //    {
            //        DrawPolyline(poly, green, 2f, true);
            //        DrawLine(poly.Last(), poly.First(), green, 2f, true);
            //    }
            //    var red = new Color("Red");
            //    foreach (var poly in TestPolys.Voids)
            //    {
            //        DrawPolyline(poly, red, 2f, true);
            //        DrawLine(poly.Last(), poly.First(), red, 2f, true);
            //    }
            //}
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventKey eventKey)
            {
                if(eventKey.Keycode == Key.Escape)
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
    }
}
