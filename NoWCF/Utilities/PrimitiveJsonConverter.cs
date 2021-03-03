using Newtonsoft.Json;
using System;

namespace NoWCF.Utilities
{
    public sealed class CustomJsonDeserializer : JsonConverter
    {
        readonly JsonSerializer defaultSerializer = new JsonSerializer();

        public override bool CanConvert(Type objectType)
        {
            objectType = Nullable.GetUnderlyingType(objectType) ?? objectType;
            return
                objectType == typeof(long)
                || objectType == typeof(ulong)
                || objectType == typeof(int)
                || objectType == typeof(uint)
                || objectType == typeof(short)
                || objectType == typeof(ushort)
                || objectType == typeof(byte)
                || objectType == typeof(sbyte)
                || objectType == typeof(System.Numerics.BigInteger)
                || objectType == typeof(object);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    if (Convert.ToInt64(reader.Value) < int.MaxValue)
                        return Convert.ToInt32(reader.Value);
                    return reader.Value;
                default:
                    return defaultSerializer.Deserialize(reader, objectType);
            }
        }

        public override bool CanWrite { get { return false; } }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
