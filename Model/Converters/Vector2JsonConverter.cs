using Godot;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GodotVTT
{
    public class Vector2JsonConverter : JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Invalid json token.");
            }

            float x = 0;
            float y = 0;

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                var name = reader.GetString();
                reader.Read();
                if (string.Equals(name, nameof(Vector2.X), StringComparison.InvariantCultureIgnoreCase))
                {
                    x = reader.GetSingle();
                }
                else if (string.Equals(name, nameof(Vector2.Y), StringComparison.InvariantCultureIgnoreCase))
                {
                    y = reader.GetSingle();
                }
                else
                {
                    reader.Skip();
                }
            }

            return new Vector2(x, y);
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
