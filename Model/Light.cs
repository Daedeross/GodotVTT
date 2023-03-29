using Godot;

namespace GodotVTT
{
    public class Light
    {
        public Vector2 position { get; set; }
        public float range { get; set; }
        public float intensity { get; set; }
        public Color color { get; set; }
        public bool shadows { get; set; }
    }
}
