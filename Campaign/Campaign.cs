using Godot;
using System;
using System.Collections.Generic;

namespace GodotVTT
{
    public partial class Campaign : Node
    {
        private readonly Dictionary<string, MapBase> _maps = new Dictionary<string, MapBase>();

        private MapBase _currentMap;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }

        public bool SwitchToMap(string  mapName)
        {
            if (_maps.TryGetValue(mapName, out var map))
            {
                if (_currentMap != null)
                {
                    this.RemoveChild(_currentMap);
                }
                AddChild(map);

                return true;
            }

            return false;
        }
    }
}