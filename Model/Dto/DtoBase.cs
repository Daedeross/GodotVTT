using GodotVTT.Interfaces;
using System;

namespace GodotVTT.Model.Dto
{
    public class DtoBase : IHaveId
    {
        public Guid Id { get; set; }
    }
}
