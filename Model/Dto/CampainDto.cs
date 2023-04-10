using GodotVTT.Interfaces;
using System;
using System.Collections.Generic;

namespace GodotVTT.Model.Dto
{
    public class CampainDto : IHaveId
    {
        public Guid Id { get; set; }

        public List<MapDto> Maps { get; set; }

        public List<ResourceDto> Resources { get; set; }
    }
}
