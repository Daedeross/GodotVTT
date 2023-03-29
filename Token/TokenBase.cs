using Godot;
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

        private NavigationAgent2D _navigationAgent;
        private MapBase _currentMap;


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();

            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;
            _currentMap = GetParent<MapBase>();
            _navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
            _navigationAgent.Radius = _currentMap.MapScale / 2f;

            var mid = _navigationAgent.GetNavigationMap();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            if (_controlState == ControlState.Primary)
            {
                var pos = _currentMap.GetGlobalMousePosition();
                Rpc(MethodName.SetPosition, pos);


                if (Input.IsActionJustReleased(Actions.Primary))
                {
                    _controlState &= (~ControlState.Primary);
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
                    }
                    else if (eventMouseButton.IsActionReleased(Actions.Primary))
                    {
                        _controlState &= (~ControlState.Primary);
                        if (_path.Any())
                        {
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

            if (_path.IsEmpty())
                return;

            DrawSetTransformMatrix(GlobalTransform.Inverse());
            var lastPoint = _path[0];
            for (int i = 1; i < _path.Length; i++)
            {
                DrawLine(lastPoint, _path[i], new Color("White"), 2f, true);
                lastPoint = _path[i];
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

        [Rpc(CallLocal = true)]
        public void SetPosition(Vector2 pos)
        {
            //GD.Print($"RPC {Multiplayer.GetRemoteSenderId()} : {pos}");


            _pathQueryParams.Map = GetWorld2D().NavigationMap;
            _pathQueryParams.StartPosition = GlobalPosition;
            _pathQueryParams.TargetPosition = pos;

            NavigationServer2D.QueryPath(_pathQueryParams, _pathQueryResult);

            _path = _pathQueryResult.Path;
            QueueRedraw();

            //_pathQueryResult.Path.

            //_navigationAgent.TargetPosition = pos;
            //if(_navigationAgent.IsTargetReachable())
            //{
            //    GlobalPosition = pos;
            //}
        }
    }
}
