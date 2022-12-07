using System;
using System.Linq;
using Newtonsoft.Json;

#if SERVER_APP
namespace MyLab.FileStorage.Models
#else
namespace MyLab.FileStorage.Client.Models
#endif
{
    /// <summary>
    /// Converts byte array into hex string
    /// </summary>
    public class HexJsonConverter : JsonConverter<byte[]>
    {
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, byte[]? value, JsonSerializer serializer)
        {
            if(value == null)
                writer.WriteNull();
            else
            {
                var strVal = BitConverter.ToString(value).Replace("-", "").ToLower();
                writer.WriteValue(strVal);
            }
        }

        /// <inheritdoc />
        public override byte[]? ReadJson(JsonReader reader, Type objectType, byte[]? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var strValue = (string?)reader.Value;

            if (strValue == null) return null;

            return Enumerable.Range(0, strValue.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(strValue.Substring(x, 2), 16))
                .ToArray();
        }
    }

    /// <summary>
    /// Converts guid into string
    /// </summary>
    public class GuidJsonConverter : JsonConverter<Guid>
    {
        public override void WriteJson(JsonWriter writer, Guid value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString("N"));
        }

        public override Guid ReadJson(JsonReader reader, Type objectType, Guid existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var strValue = (string?)reader.Value;

            if (strValue == null) return Guid.Empty;

            return Guid.Parse(strValue);
        }
    }
}
