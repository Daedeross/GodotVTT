using Godot;
using System;

namespace GodotVTT
{
    public partial class TokenBase : Area2D
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
    }
}
