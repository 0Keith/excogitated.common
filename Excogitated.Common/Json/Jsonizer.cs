using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public class JsonObject
    {
        public override string ToString() => Jsonizer.Serialize(this, true);
    }

    public class ClassConverter<T> : JsonConverter<T>
    {
        private readonly Func<T, string> _serializer;
        private readonly Func<string, Type, T> _deserializer;

        public override bool CanConvert(Type objectType) => typeof(T).IsAssignableFrom(objectType);

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            _deserializer(reader.GetString(), typeToConvert);
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteStringValue(_serializer(value));

        public ClassConverter(Func<T, string> serializer, Func<string, Type, T> deserializer)
        {
            _serializer = serializer.NotNull(nameof(serializer));
            _deserializer = deserializer.NotNull(nameof(deserializer));
        }
    }

    public class StructConverter<T> : JsonConverter<T>
    {
        private readonly Func<T, string> _serializer;
        private readonly Func<string, T> _deserializer;

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            _deserializer(reader.GetString());
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteStringValue(_serializer(value));

        public StructConverter(Func<T, string> serializer, Func<string, T> deserializer)
        {
            _serializer = serializer.NotNull(nameof(serializer));
            _deserializer = deserializer.NotNull(nameof(deserializer));
        }
    }

    public static class Jsonizer
    {
        public static JsonSerializerOptions DefaultSettings { get; } = BuildDefaultSettings();
        public static JsonSerializerOptions FormattedSettings { get; } = BuildDefaultSettings(true);

        public static JsonSerializerOptions BuildDefaultSettings(bool formatted = false)
        {
            var options = new JsonSerializerOptions();
            options.IgnoreNullValues = true;
            options.WriteIndented = formatted;
            options.PropertyNameCaseInsensitive = true;
            options.AddStructConverter<Date>(d => d, d => d);
            options.AddStructConverter<MonthDayYear>(d => d, d => d);
            options.AddStructConverter(d => d.ToString(), s => s.ToDecimal());
            //options.MissingMemberHandling = MissingMemberHandling.Error;
            options.AddClassConverter(e => e.ToString(), (s, t) =>
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
            return options;
        }

        public static void AddClassConverter<T>(this JsonSerializerOptions settings, Func<T, string> serializer, Func<string, Type, T> deserializer)
            where T : class
        {
            var converters = settings?.Converters.NotNull(nameof(settings));
            converters.Add(new ClassConverter<T>(serializer, deserializer));
        }

        public static void AddStructConverter<T>(this JsonSerializerOptions settings, Func<T, string> serializer, Func<string, T> deserializer)
            where T : struct
        {
            var converters = settings?.Converters.NotNull(nameof(settings));
            converters.Add(new StructConverter<T>(serializer, deserializer));
            converters.Add(new StructConverter<T?>(v =>
            {
                if (v.HasValue)
                    return serializer(v.Value);
                return null;
            }, s =>
            {
                if (s.IsNullOrWhiteSpace())
                    return null;
                return deserializer(s);
            }));
        }

        public static string Format(string json) => Serialize(Deserialize<object>(json), true);
        public static T CopyTo<T>(object source) => Deserialize<T>(Serialize(source));
        public static T DeepCopy<T>(T item) => CopyTo<T>(item);

        public static string Serialize<T>(T value, bool formatted = false) =>
            JsonSerializer.Serialize(value, formatted ? FormattedSettings : DefaultSettings);

        public static Task SerializeAsync<T>(T item, Stream stream, bool formatted = true) =>
            JsonSerializer.SerializeAsync(stream, item, formatted ? FormattedSettings : DefaultSettings);

        public static T Deserialize<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json, DefaultSettings);
            }
            catch (Exception e)
            {
                throw new Exception($"Could not Deserialize. Type: {typeof(T)}, Json: {json}", e);
            }
        }

        public static ValueTask<T> DeserializeAsync<T>(Stream stream) =>
            JsonSerializer.DeserializeAsync<T>(stream, DefaultSettings);
    }
}