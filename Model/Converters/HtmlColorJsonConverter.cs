using Godot;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GodotVTT
{
    public class HtmlColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new Color(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToHtml());
        }
    }
}
