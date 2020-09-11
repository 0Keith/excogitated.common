using Excogitated.Common.Atomic.Collections;
using Excogitated.Common.Extensions;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Excogitated.Common.Json
{
    public class JsonObject
    {
        public override string ToString() => Jsonizer.Serialize(this, true);
    }

    public class CustomJsonStringEnumConverter : JsonConverterFactory
    {
        private static readonly CowDictionary<Type, JsonConverter> _converters = new CowDictionary<Type, JsonConverter>();
        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (!_converters.TryGetValue(typeToConvert, out var converter))
            {
                var type = typeof(CustomJsonStringEnumConverter<>).MakeGenericType(typeToConvert);
                _converters[typeToConvert] = converter = (JsonConverter)Activator.CreateInstance(type);
            }
            return converter;
        }
    }

    public class CustomJsonStringEnumConverter<T> : JsonConverter<T> where T : struct
    {
        private static readonly CowDictionary<string, T> _valuesByText = new CowDictionary<string, T>();
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var text = reader.GetString();
            if (_valuesByText.TryGetValue(text, out var value))
                return value;

            if (!Enum.TryParse(text, out value) && !Enum.TryParse(text, true, out value))
            {
                var cleaned = new string(text.Where(c => c.IsLetter() || c.IsDigit() || c == '_').ToArray());
                if (!Enum.TryParse(cleaned, out value) && !Enum.TryParse(cleaned, true, out value))
                    throw new Exception(new
                    {
                        Message = "Invalid enum value",
                        Type = typeof(T),
                        Text = text,
                    }.ToString());
            }
            _valuesByText[text] = value;
            return value;
        }

        private static readonly CowDictionary<T, string> _textByValue = new CowDictionary<T, string>();
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (!_textByValue.TryGetValue(value, out var text))
                _textByValue[value] = text = value.ToString();
            writer.WriteStringValue(text);
        }
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

        private const string ZERO = "0";
        private const JsonTokenType NUMBER = JsonTokenType.Number;

        public static JsonSerializerOptions BuildDefaultSettings(bool formatted = false)
        {
            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                WriteIndented = formatted,
                PropertyNameCaseInsensitive = true,
            };
            options.Converters.Add(new CustomJsonStringEnumConverter());
            options.AddStructConverter<Date>((w, v) => w.WriteStringValue(v.ToCharSpan()), (ref Utf8JsonReader r) => r.GetString());
            options.AddStructConverter<MonthDayYear>((w, v) => w.WriteStringValue(v.ToCharSpan()), (ref Utf8JsonReader r) => r.GetString());
            options.AddStructConverter<Currency>((w, v) => w.WriteStringValue(v.ToString()), (ref Utf8JsonReader r) => r.GetString() ?? ZERO);
            options.AddStructConverter((w, v) => w.WriteStringValue(v.ToString()), (ref Utf8JsonReader r) => TimeSpan.Parse(r.GetString()));

            //string to number handlers... because it doesn't handle numbers encased in quotes by default
            options.AddStructConverter((w, v) => w.WriteNumberValue(v), (ref Utf8JsonReader r) => r.TokenType == NUMBER && r.TryGetByte(out var d) || byte.TryParse(r.GetString() ?? ZERO, out d) ? d : default);
            options.AddStructConverter((w, v) => w.WriteNumberValue(v), (ref Utf8JsonReader r) => r.TokenType == NUMBER && r.TryGetSByte(out var d) || sbyte.TryParse(r.GetString() ?? ZERO, out d) ? d : default);
            options.AddStructConverter((w, v) => w.WriteNumberValue(v), (ref Utf8JsonReader r) => r.TokenType == NUMBER && r.TryGetInt16(out var d) || short.TryParse(r.GetString() ?? ZERO, out d) ? d : default);
            options.AddStructConverter((w, v) => w.WriteNumberValue(v), (ref Utf8JsonReader r) => r.TokenType == NUMBER && r.TryGetUInt16(out var d) || ushort.TryParse(r.GetString() ?? ZERO, out d) ? d : default);
            options.AddStructConverter((w, v) => w.WriteNumberValue(v), (ref Utf8JsonReader r) => r.TokenType == NUMBER && r.TryGetInt32(out var d) || int.TryParse(r.GetString() ?? ZERO, out d) ? d : default);
            options.AddStructConverter((w, v) => w.WriteNumberValue(v), (ref Utf8JsonReader r) => r.TokenType == NUMBER && r.TryGetUInt32(out var d) || uint.TryParse(r.GetString() ?? ZERO, out d) ? d : default);
            options.AddStructConverter((w, v) => w.WriteNumberValue(v), (ref Utf8JsonReader r) => r.TokenType == NUMBER && r.TryGetInt64(out var d) || long.TryParse(r.GetString() ?? ZERO, out d) ? d : default);
            options.AddStructConverter((w, v) => w.WriteNumberValue(v), (ref Utf8JsonReader r) => r.TokenType == NUMBER && r.TryGetUInt64(out var d) || ulong.TryParse(r.GetString() ?? ZERO, out d) ? d : default);
            options.AddStructConverter((w, v) => w.WriteNumberValue(v), (ref Utf8JsonReader r) => r.TokenType == NUMBER && r.TryGetSingle(out var d) || float.TryParse(r.GetString() ?? ZERO, out d) ? d : default);
            options.AddStructConverter((w, v) => w.WriteNumberValue(v), (ref Utf8JsonReader r) => r.TokenType == NUMBER && r.TryGetDouble(out var d) || double.TryParse(r.GetString() ?? ZERO, out d) ? d : default);
            options.AddStructConverter((w, v) => w.WriteNumberValue(v), (ref Utf8JsonReader r) => r.TokenType == NUMBER && r.TryGetDecimal(out var d) || decimal.TryParse(r.GetString() ?? ZERO, out d) ? d : default);
            return options;
        }

        public static async Task<TValue> DeserializeFromZipAsync<TValue>(string fileName, string entryName = "data.json")
        {
            var file = new FileInfo(fileName);
            using var zip = ZipFile.OpenRead(file.FullName);
            using var stream = zip.GetEntry(entryName).Open();
            return await DeserializeAsync<TValue>(stream);
        }

        public static async Task SerializeToZipAsync<TValue>(TValue data, string fileName, bool formatted = true, string entryName = "data.json")
        {
            var file = new FileInfo(fileName);
            await file.Directory.CreateStrongAsync();
            using var zip = ZipFile.Open(file.FullName, ZipArchiveMode.Update);
            using var stream = zip.CreateEntry(entryName).Open();
            await SerializeAsync(data, stream, formatted);
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

        public static string Format(string json)
        {
            using var doc = JsonDocument.Parse(json);
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            doc.WriteTo(writer);
            writer.Flush();
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static T CopyTo<T>(object source) => Deserialize<T>(Serialize(source));
        public static T DeepCopy<T>(T item) => CopyTo<T>(item);

        public static string Serialize(object value, bool formatted = false) =>
            JsonSerializer.Serialize(value, value.GetType(), formatted ? FormattedSettings : DefaultSettings);

        public static Task SerializeAsync(object value, Stream stream, bool formatted = true) =>
            JsonSerializer.SerializeAsync(stream, value, value.GetType(), formatted ? FormattedSettings : DefaultSettings);

        public static T Deserialize<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json, DefaultSettings);
            }
            catch (Exception e)
            {
                throw new Exception($"Could not Deserialize. Type: {typeof(T)}, Message: {e.Message}, Json: {Format(json)}", e);
            }
        }

        public static ValueTask<T> DeserializeAsync<T>(Stream stream) =>
            JsonSerializer.DeserializeAsync<T>(stream, DefaultSettings);
    }
}