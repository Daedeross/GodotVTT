using Godot;
using System.Collections.Generic;
using System.Linq;

namespace GodotVTT
{
    public static class NodeExtensions
    {
        public static IEnumerable<T> GetChildren<T>(this Node node)
            where T : Node
        {
            return node.GetChildren().Where(child => child is T).Cast<T>();
        }
    }
}
