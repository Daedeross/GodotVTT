using Godot;
using System;
using System.Collections;
using System.Linq;

namespace GodotVTT
{
    public partial class TokenBase : Area2D
    {
        private bool _isMouseOver;
        private ControlState _controlState = ControlState.None;

        NavigationPathQueryParameters2D _pathQueryParams = new ();
        NavigationPathQueryResult2D _pathQueryResult = new();

        private Vector2[] _path;
        private bool _pathValid;

        private NavigationAgent2D _navigationAgent;
        private MapBase _currentMap;

        #region Child References

        private Sprite2D _mainSprite;
        private Sprite2D _ghostSprite;

        #endregion

        private float _size = 1f;
        public float Size
        {
            get => _size;
            set => SetSize(value);
        }

        public void SetSize(float value, bool force = false)
        {
            if (force || value != _size)
            {
                //var size = _currentMap.MapScale * value;
                var size = 100f * value;
                var textureSize = _mainSprite.Texture.GetSize();
                var scale = new Vector2(size / textureSize.X, size / textureSize.Y);
                _mainSprite.Scale = scale;
                _ghostSprite.Scale = scale;
            }
        }


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();

            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;
            _currentMap = GetParent<MapBase>();
            _navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
            _navigationAgent.Radius = _currentMap.MapScale / 2f;
            _mainSprite = GetNode<Sprite2D>("MainSprite");
            _ghostSprite = GetNode<Sprite2D>("GhostSprite");
            _ghostSprite.Visible = false;

            SetSize(Size, true);

            var mid = _navigationAgent.GetNavigationMap();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            if (_controlState == ControlState.Primary)
            {
                var pos = _currentMap.GetGlobalMousePosition();
                _pathValid = CheckPosition(pos);


                if (Input.IsActionJustReleased(Actions.Primary))
                {
                    _controlState &= (~ControlState.Primary);
                    //if (valid)
                    //{
                    //    Rpc(MethodName.SetPosition, pos);
                    //}
                }
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (IsMultiplayerAuthority())
            {
                if (@event is InputEventMouseButton eventMouseButton)
                {
                    if (_isMouseOver && eventMouseButton.IsActionPressed(Actions.Primary))
                    {
                        _controlState |= ControlState.Primary;
                        _ghostSprite.Visible = true;
                    }
                    else if (eventMouseButton.IsActionReleased(Actions.Primary))
                    {
                        _controlState &= (~ControlState.Primary);
                        _ghostSprite.Visible = false;

                        if (_path != null && _path.Any())
                        {
                            if (_pathValid)
                            {
                                Rpc(MethodName.SetPosition, _currentMap.ToLocal(_path.Last()));
                            }
                            _path = null;
                            QueueRedraw();
                        }
                    }
                }
            }
        }

        public override void _Draw()
        {
            base._Draw();

            if (!_path.IsEmpty())
            {
                DrawSetTransformMatrix(GlobalTransform.Inverse());
                DrawPolyline(_path, new Color("White"), 2f, true);
            }
        }

        public void RevealFog()
        {
        }

        private void OnMouseEntered()
        {
            _isMouseOver = true;
        }

        private void OnMouseExited()
        {
            _isMouseOver = false;
        }

        public bool CheckPosition(Vector2 pos)
        {
            _pathQueryParams.Map = GetWorld2D().NavigationMap;
            _pathQueryParams.StartPosition = GlobalPosition;
            _pathQueryParams.TargetPosition = pos;

            NavigationServer2D.QueryPath(_pathQueryParams, _pathQueryResult);

            _path = _pathQueryResult.Path;
            var last = _path.Last();
            QueueRedraw();

            _ghostSprite.GlobalPosition = last;

            return last.IsEqualApprox(pos);
        }

        [Rpc(CallLocal = true)]
        public void SetPosition(Vector2 pos)
        {
            GD.Print($"RPC {Multiplayer.GetRemoteSenderId()} : {pos}");

            _path = null;

            Position = pos;
            _ghostSprite.Position = Vector2.Zero;

            //_pathQueryResult.Path.

            //_navigationAgent.TargetPosition = pos;
            //if(_navigationAgent.IsTargetReachable())
            //{
            //    GlobalPosition = pos;
            //}
        }

        public void SetTexture(Texture2D texture)
        {
            _mainSprite.Texture = texture;
            _ghostSprite.Texture = texture;
        }
    }
}
