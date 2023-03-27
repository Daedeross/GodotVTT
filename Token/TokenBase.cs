using Godot;
using System;

namespace GodotVTT
{
    public partial class TokenBase : Area2D
    {
        private bool _isMouseOver;
        private ControlState _controlState = ControlState.None;

        private MapBase _currentMap;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;
            _currentMap = GetParent<MapBase>();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            if (_controlState == ControlState.Primary)
            {
                var pos = _currentMap.GetLocalMousePosition();
                Rpc(MethodName.SetPosition, pos);
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (_isMouseOver && @event is InputEventMouseButton eventMouseButton)
            {
                if (eventMouseButton.IsActionPressed(Actions.Primary))
                {
                    _controlState = _controlState | ControlState.Primary;
                }
                else if (eventMouseButton.IsActionReleased(Actions.Primary))
                {
                    _controlState = _controlState & (~ControlState.Primary);
                }
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
            GD.Print($"RPC {Multiplayer.GetRemoteSenderId()} : {pos}");
            Position = pos;
        }
    }
}
