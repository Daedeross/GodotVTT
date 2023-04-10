using Godot;
using GodotVTT.Interfaces;
using GodotVTT.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotVTT
{
    /// <summary>
    /// Class to manage a campaign. A bucket of maps, characters, handouts, etc.
    /// </summary>
    public partial class Campaign : Node, IHaveId
    {
        private readonly Dictionary<ulong, int> _resourceHashes = new();

        private readonly Dictionary<string, MapBase> _maps = new();

        private readonly Dictionary<Guid, Resource> _resources = new();

        private MapBase _currentMap;

        public Guid Id { get; set; }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {

        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }

        public bool SwitchToMap(string mapName)
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

        public bool TryGetResource(Guid id, out Resource resource)
        {
            return _resources.TryGetValue(id, out resource);
        }

        public void AddResource(Guid id , Resource resource)
        {
            _resources.Add(id, resource);
        }

        public Guid AddResource(Resource resource)
        {
            var id = Guid.NewGuid();
            _resources.Add(id, resource);
            return id;
        }

        public CampainDto ToDto()
        {
            var resources = _resources.Select(TryConvertResource).Where(x => x is not null).ToList();
            return new CampainDto
            {
                Id = Id,
                Maps = _maps.Values.Select(m => m.ToDto()).ToList(),
                Resources = resources
            };
        }

        private ResourceDto TryConvertResource(KeyValuePair<Guid, Resource> resource) => 
            resource.Value switch
            {
                Image image => new ImageDto { Id = resource.Key, Format = image.GetFormat(), Name = image.ResourceName, ResourceType = Model.ResourceType.Image, Data = image.SavePngToBuffer() },
                _ => default
            };
    }
}