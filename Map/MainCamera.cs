using Godot;
using System;

namespace GodotVTT
{
    public partial class MainCamera : Camera2D
    {
        private ControlState _controlState = ControlState.None;

        private float _currentZoom = 1f;

        [Export]
        public float MinZoom = 0.1f;

        [Export]
        public float MaxZoom = 10f;

        [Export]
        public Vector2 MapScale = new(1f, 1f);

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {

        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton)
            {
                if (eventMouseButton.IsActionPressed(Actions.Secondary))
                {
                    _controlState |= ControlState.Secondary;
                }
                else if (eventMouseButton.IsActionReleased(Actions.Secondary))
                {
                    _controlState &= (~ControlState.Secondary);
                }
                else if (eventMouseButton.IsAction(Actions.WheelUp))
                {
                    ScaleZoom(1.25f);
                }
                else if (eventMouseButton.IsAction(Actions.WheelDown))
                {
                    ScaleZoom(0.8f);
                }
            }
            if (@event is InputEventMouseMotion eventMouseMotion)
            {
                if (_controlState == ControlState.Secondary)
                {
                    Position -= eventMouseMotion.Relative / _currentZoom;
                }
            }
        }

        public void ScaleZoom(float factor)
        {
            SetZoom(_currentZoom * factor);
        }

        public void SetZoom(float zoom)
        {
            _currentZoom = Mathf.Clamp(zoom, MinZoom, MaxZoom);
            Zoom = MapScale * _currentZoom;
        }
    }
}
