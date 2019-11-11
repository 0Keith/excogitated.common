using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public class JsonObject
    {
        public override string ToString() => Jsonizer.Serialize(this, true);
    }

    public class JsonLambdaConverter : JsonConverter
    {
        private readonly Func<Type, bool> _canConvert;
        private readonly Func<object, string> _serializer;
        private readonly Func<string, Type, object> _deserializer;

        public override bool CanConvert(Type objectType) => _canConvert(objectType);

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer) =>
            writer.WriteValue(_serializer(value));

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer) =>
            _deserializer(reader.Value?.ToString(), objectType);

        public JsonLambdaConverter(Func<Type, bool> canConvert, Func<object, string> serializer, Func<string, Type, object> deserializer)
        {
            _canConvert = canConvert.NotNull(nameof(canConvert));
            _serializer = serializer.NotNull(nameof(serializer));
            _deserializer = deserializer.NotNull(nameof(deserializer));
        }
    }

    public class JsonLambdaConverter<T> : JsonConverter<T>
    {
        private readonly Func<T, string> _serializer;
        private readonly Func<string, Type, T> _deserializer;

        public override void WriteJson(JsonWriter writer, T value, Newtonsoft.Json.JsonSerializer serializer) =>
            writer.WriteValue(_serializer(value));

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer) =>
            _deserializer(reader.Value?.ToString(), objectType);

        public JsonLambdaConverter(Func<T, string> serializer, Func<string, Type, T> deserializer)
        {
            _serializer = serializer.NotNull(nameof(serializer));
            _deserializer = deserializer.NotNull(nameof(deserializer));
        }
    }

    public static class Jsonizer
    {
        public static JsonSerializerSettings DefaultSettings { get; } = BuildDefaultSettings();
        public static JsonSerializerSettings FormattedSettings { get; } = BuildDefaultSettings(true);
        public static JsonSerializer DefaultSerializer { get; } = JsonSerializer.Create(DefaultSettings);
        public static JsonSerializer FormattedSerializer { get; } = JsonSerializer.Create(FormattedSettings);

        public static JsonSerializerSettings BuildDefaultSettings(bool formatted = false)
        {
            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.DateFormatString = "o";
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            settings.Formatting = formatted ? Formatting.Indented : Formatting.None;
            settings.AddStructConverter<Date>(d => d, (s, t) => s);
            settings.AddClassConverter(e => e.ToString(), (s, t) =>
            {
                if (s.IsNullOrWhiteSpace())
                    return (Enum)Activator.CreateInstance(t);
                if (Enum.TryParse(t, s, true, out var result))
                    return (Enum)result;
                var clean = new string(s.SkipWhile(c => !c.IsLetter()).Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
                if (Enum.TryParse(t, clean, true, out result))
                    return (Enum)result;
                throw new Exception($"Invalid enum - Value: {s}, Type: {t.FullName}");
            });
            settings.AddStructConverter(d => d.ToString(), (s, t) => s.ToDecimal());
            return settings;
        }

        public static void AddClassConverter<T>(this JsonSerializerSettings settings, Func<T, string> serializer, Func<string, Type, T> deserializer)
            where T : class
        {
            var converters = settings?.Converters.NotNull(nameof(settings));
            for (int i = 0; i < converters.Count; i++)
                if (converters[i].CanConvert(typeof(T)))
                    converters.RemoveAt(i--);
            converters.Add(new JsonLambdaConverter<T>(serializer, deserializer));
        }

        public static void AddStructConverter<T>(this JsonSerializerSettings settings, Func<T, string> serializer, Func<string, Type, T> deserializer)
            where T : struct
        {
            var converters = settings?.Converters.NotNull(nameof(settings));
            for (int i = 0; i < converters.Count; i++)
                if (converters[i].CanConvert(typeof(T)))
                    converters.RemoveAt(i--);
            converters.Add(new JsonLambdaConverter<T>(serializer, deserializer));
            converters.Add(new JsonLambdaConverter<T?>(v =>
            {
                if (v.HasValue)
                    return serializer(v.Value);
                return null;
            }, (s, t) =>
            {
                if (s.IsNullOrWhiteSpace())
                    return null;
                return deserializer(s, t);
            }));
        }

        public static string Format(string json) => Serialize(Deserialize<object>(json), true);
        public static T CopyTo<T>(object source) => Deserialize<T>(Serialize(source));
        public static T DeepCopy<T>(T item) => CopyTo<T>(item);

        public static string Serialize<T>(T value, bool formatted = false) => JsonConvert.SerializeObject(value, formatted ? FormattedSettings : DefaultSettings);

        public static async Task SerializeAsync<T>(T item, Stream stream, bool formatted = true)
        {
            stream.NotNull(nameof(stream));
            using var mem = new MemoryStream();
            using var writer = new StreamWriter(mem);
            (formatted ? FormattedSerializer : DefaultSerializer).Serialize(writer, item);
            writer.Flush();
            mem.Position = 0;
            await mem.CopyToAsync(stream);
            await stream.FlushAsync();
        }

        public static T Deserialize<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, DefaultSettings);
            }
            catch (Exception e)
            {
                throw new Exception($"Could not Deserialize. Type: {typeof(T)}, Json: {json}", e);
            }
        }

        public static async Task<T> DeserializeAsync<T>(Stream stream)
        {
            stream.NotNull(nameof(stream));
            using var mem = new MemoryStream();
            await stream.CopyToAsync(mem);
            mem.Position = 0;
            using var reader = new StreamReader(mem);
            using var json = new JsonTextReader(reader);
            return DefaultSerializer.Deserialize<T>(json);
        }
    }
}