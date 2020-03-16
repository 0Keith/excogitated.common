using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public class JsonClassGenerator
    {
        public enum Visibility { @public, @private, @internal, @protected }

        private readonly StringBuilder _builder = new StringBuilder();
        private int _depth = 0;

        public Visibility ClassVisibility { get; set; } = Visibility.@public;
        public Visibility PropertyVisibility { get; set; } = Visibility.@public;
        public string RootName { get; set; } = "Root";
        public char Separator { get; set; } = ' ';

        public string FromString(string json)
        {
            using var doc = JsonDocument.Parse(json);
            return FromDocument(doc);
        }

        public async Task<string> FromUrl(string url)
        {
            using var client = new HttpClient();
            using var stream = await client.GetStreamAsync(url);
            return await FromStream(stream);
        }

        public async Task<string> FromStream(Stream stream)
        {
            using var doc = await JsonDocument.ParseAsync(stream);
            return FromDocument(doc);
        }

        public string FromDocument(JsonDocument doc)
        {
            _builder.Clear();
            FromElement(doc.NotNull(nameof(doc)).RootElement, RootName);
            return _builder.ToString();
        }

        private string FromElement(JsonElement element, string name)
        {
            if (element.ValueKind == JsonValueKind.Object)
                return FromObject(element, name);
            if (element.ValueKind == JsonValueKind.Array)
                return FromArray(element, name);
            return GetTypeName(element);
        }

        private string GetTypeName(JsonElement element)
        {
            var raw = element.ToString();
            if (int.TryParse(raw, out _)) return "int";
            if (long.TryParse(raw, out _)) return "long";
            if (decimal.TryParse(raw, out _)) return "decimal";
            if (double.TryParse(raw, out _)) return "double";
            if (bool.TryParse(raw, out _)) return "bool";
            if (DateTime.TryParse(raw, out var _)) return nameof(DateTime);
            if (DateTimeOffset.TryParse(raw, out var _)) return nameof(DateTimeOffset);
            if (Guid.TryParse(raw, out var _)) return nameof(Guid);
            if (Currency.TryParse(raw, out var _)) return nameof(Currency);
            return "string";
        }

        private string FromObject(JsonElement element, string className)
        {
            className = className.NotNullOrWhitespace(nameof(className)).Trim();
            if (char.IsLower(className[0]))
            {
                var first = char.ToUpper(className[0]);
                if (className.Length > 1)
                    className = first + className.Substring(1);
            }
            _builder.Append(Separator, _depth)
                .Append(PropertyVisibility.ToString()).Append(" class ")
                .Append(className).AppendLine(" {");
            _depth++;
            using var objs = element.EnumerateObject();
            while (objs.MoveNext())
            {
                var name = objs.Current.Name;
                var typeName = FromElement(objs.Current.Value, name);
                if (typeName is string)
                    _builder.Append(Separator, _depth)
                        .Append(PropertyVisibility.ToString()).Append(' ')
                        .Append(typeName).Append(' ')
                        .Append(name).AppendLine(" { get; set; }");
            }
            _depth--;
            _builder.Append(Separator, _depth).Append('}').AppendLine();
            return className;
        }

        private string FromArray(JsonElement element, string name)
        {
            using var objs = element.EnumerateArray();
            if (objs.MoveNext())
                return FromElement(objs.Current, name) + "[]";
            return null;
        }
    }
}