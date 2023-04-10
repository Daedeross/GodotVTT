using System;
using System.Collections.Generic;

namespace GodotVTT.Model.Dto
{
    public class MapDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public float MapScale { get; set; }

        public Guid MapImageId { get; set; }

        public List<TokenDto> Tokens { get; set; }
    }
}
