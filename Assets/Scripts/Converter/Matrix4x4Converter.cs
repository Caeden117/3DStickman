using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Stickman3D
{
    /// <summary>
    /// Custom Newtonsoft.Json converter for UnityEngine.Matrix4x4 since none come included.
    /// Other converters exist online but convert a Matrix into a full JSON object.
    /// So I wrote my own converter which converts a Matrix into a simple array of 16 floats.
    /// </summary>
    public sealed class Matrix4x4Converter : JsonConverter<Matrix4x4>
    {
        public override Matrix4x4 ReadJson(JsonReader reader, Type objectType, Matrix4x4 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            ref var matrix = ref existingValue;

            // Early return if we are at the start of an array
            if (reader.TokenType != JsonToken.StartArray) return matrix;

            for (var row = 0; row < 4; row++)
            {
                for (var col = 0; col < 4; col++)
                {
                    if (reader.TokenType == JsonToken.EndArray) break;

                    matrix[row, col] = (float)reader.ReadAsDouble();
                }
            }

            // Read the EndArray token
            reader.Read();

            return matrix;
        }

        // Simply iterates through the 16 float values and writes them as an array.
        public override void WriteJson(JsonWriter writer, Matrix4x4 value, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            for (var row = 0; row < 4; row++)
            {
                for (var col = 0; col < 4; col++)
                {
                    writer.WriteValue(value[row, col]);
                }
            }

            writer.WriteEndArray();
        }
    }
}

