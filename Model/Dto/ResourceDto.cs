namespace GodotVTT.Model.Dto
{
    public abstract class ResourceDto : DtoBase
    {
        public ResourceType ResourceType { get; set; }

        public string Name { get; set; }
    }
}
