using Godot;
using System.Collections.Generic;

namespace GodotVTT
{
    public class Dd2Vtt
    {
        public float format { get; set; }
        public Resolution resolution { get; set; }
        public List<List<Vector2>> line_of_sight { get; set; }
        public Environment environment { get; set; }
        public List<Light> lights { get; set; }
        public byte[] image { get; set; }

    }
}
