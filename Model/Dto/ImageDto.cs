namespace GodotVTT.Model.Dto
{
    public class ImageDto : ResourceDto
    {
        public Godot.Image.Format Format { get; set; }

        public byte[] Data { get; set; }
    }
}
