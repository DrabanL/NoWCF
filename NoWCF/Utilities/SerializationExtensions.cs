using Newtonsoft.Json;

namespace NoWCF.Utilities
{
    public static class SerializationExtensions
    {
        public static T Deserialize<T>(this string value)
        {
            return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings()
            {
                Converters = { new CustomJsonDeserializer() },
            });
        }

        public static string Serialize<T>(this T value)
        {
            if (value == null)
                return string.Empty;

            return JsonConvert.SerializeObject(value);
        }
    }
}
