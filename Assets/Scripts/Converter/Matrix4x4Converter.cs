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
        private const int matrixSize = 16;

        public override Matrix4x4 ReadJson(JsonReader reader, Type objectType, Matrix4x4 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            ref var matrix = ref existingValue;

            // Early return if we are at the start of an array
            if (reader.TokenType != JsonToken.StartArray) return matrix;

            var floatsRead = 0;

            // Continue reading the array until we either hit the end of the array OR have read enough data to fill an entire matrix
            while (reader.Read() && reader.TokenType != JsonToken.EndArray && floatsRead < matrixSize)
            {
                // Unfortunately ReadAsSingle() doesnt exist so we have to read as a double, then cast to float
                var arrayValue = reader.ReadAsDouble();
                if (arrayValue != null)
                {
                    matrix[floatsRead] = (float)arrayValue.Value;
                }
                floatsRead++;
            }

            return matrix;
        }

        // Simply iterates through the 16 float values and writes them as an array.
        public override void WriteJson(JsonWriter writer, Matrix4x4 value, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            for (var i = 0; i < matrixSize; i++)
            {
                writer.WriteValue(value[i]);
            }

            writer.WriteEndArray();
        }
    }
}

