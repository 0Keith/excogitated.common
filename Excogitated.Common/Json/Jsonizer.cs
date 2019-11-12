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


    public delegate T JsonConverterRead<T>(ref Utf8JsonReader reader);
    public delegate void JsonConverterWrite<T>(Utf8JsonWriter writer, T value);
    public class StructConverter<T> : JsonConverter<T>
    {
        private readonly JsonConverterWrite<T> _serializer;
        private readonly JsonConverterRead<T> _deserializer;

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => _deserializer(ref reader);
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => _serializer(writer, value);

        public StructConverter(JsonConverterWrite<T> serializer, JsonConverterRead<T> deserializer)
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
            options.AddStructConverter<Date>((w, v) => w.WriteStringValue(v.ToCharSpan()), (ref Utf8JsonReader r) => r.GetString());
            options.AddStructConverter<MonthDayYear>((w, v) => w.WriteStringValue(v.ToCharSpan()), (ref Utf8JsonReader r) => r.GetString());
            options.AddStructConverter((w, v) => w.WriteNumberValue(v), (ref Utf8JsonReader r) => r.TryGetDecimal(out var d) ? d : r.GetString().ToDecimal());
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

        public static void AddStructConverter<T>(this JsonSerializerOptions settings, JsonConverterWrite<T> serializer, JsonConverterRead<T> deserializer)
            where T : struct
        {
            var converters = settings?.Converters.NotNull(nameof(settings));
            converters.Add(new StructConverter<T>(serializer, deserializer));
            converters.Add(new StructConverter<T?>((writer, value) =>
            {
                if (value.HasValue)
                    serializer(writer, value.Value);
                else
                    writer.WriteNullValue();
            }, (ref Utf8JsonReader reader) =>
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return null;
                return deserializer(ref reader);
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