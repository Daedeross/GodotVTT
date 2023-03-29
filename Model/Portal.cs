using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotVTT
{
    public class Portal
    {
        public Vector2 position { get; set; }
        public List<Vector2> bounds { get; set; }
        public float rotation { get; set; }
        public bool closed { get; set; }
        public bool freestanding { get; set; }
    }
}
