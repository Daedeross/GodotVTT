using Godot;
using System;

namespace GodotVTT
{
    public partial class MainCamera : Camera2D
    {
        private ControlState _controlState = ControlState.None;

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
                    _controlState = _controlState | ControlState.Secondary;
                }
                else if (eventMouseButton.IsActionReleased(Actions.Secondary))
                {
                    _controlState = _controlState & (~ControlState.Secondary);
                }
            }
            if (@event is InputEventMouseMotion eventMouseMotion)
            {
                if (_controlState == ControlState.Secondary)
                {
                    Position -= eventMouseMotion.Relative;
                }
            }
        }
    }
}