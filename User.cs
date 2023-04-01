using Godot;
using System;
using System.Collections.Generic;

namespace GodotVTT
{
    /// <summary>
    /// Class that represents a single player or GM in a session.
    /// </summary>
    public partial class User : Node
    {
        public string UserName { get; set; }

        public UserRole Role { get; set; }

        //public ISet<>

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
