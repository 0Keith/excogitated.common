using Excogitated.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public class JsonClassGeneratorSettings
    {
        public enum Visibility { Public, Private, Internal, Protected }
        public Visibility ClassVisibility { get; set; }
        public Visibility PropertyVisibility { get; set; }
        public string RootName { get; set; } = "Root";
        public char IndentChar { get; set; } = '\t';
    }

    public class JsonClassGenerator
    {
        private readonly Dictionary<string, GeneratedClass> _generatedClasses = new Dictionary<string, GeneratedClass>();

        public JsonClassGeneratorSettings Settings { get; set; }

        public string FromString(string json)
        {
            using var doc = JsonDocument.Parse(json);
            return FromDocument(doc);
        }

        public async ValueTask<string> FromUrl(string url)
        {
            using var client = new HttpClient();
            using var stream = await client.GetStreamAsync(url);
            return await FromStream(stream);
        }

        public async ValueTask<string> FromFile(string jsonFile)
        {
            using var file = File.OpenRead(jsonFile);
            return await FromStream(file);
        }

        public async ValueTask<string> FromStream(Stream stream)
        {
            using var doc = await JsonDocument.ParseAsync(stream);
            return FromDocument(doc);
        }

        public string FromDocument(JsonDocument doc)
        {
            var generated = FromElement(doc.NotNull(nameof(doc)).RootElement, Settings.RootName);
            return generated.ToString(Settings);
        }

        private GeneratedClass FromElement(JsonElement element, string name)
        {
            if (element.ValueKind == JsonValueKind.Object)
                return FromObject(element, name);
            if (element.ValueKind == JsonValueKind.Array)
                return FromArray(element, name);
            var typeName = GetTypeName(element);
            var generated = new GeneratedClass { ClassName = typeName };
            generated.ExampleValues.Add(element.ToString());
            generated.IsPrimitive = true;
            return generated;
        }

        private string GetTypeName(JsonElement element)
        {
            var raw = element.ToString();
            if (int.TryParse(raw, out _)) return "int";
            if (long.TryParse(raw, out _)) return "long";
            if (decimal.TryParse(raw, out _)) return "decimal";
            if (double.TryParse(raw, out _)) return "double";
            if (bool.TryParse(raw, out _)) return "bool";
            //if (Date.TryParse(raw, out _)) return nameof(Date); // TryParse needs to be improved to recognize invalid dates better
            if (DateTime.TryParse(raw, out var _)) return nameof(DateTime);
            if (DateTimeOffset.TryParse(raw, out var _)) return nameof(DateTimeOffset);
            if (Guid.TryParse(raw, out var _)) return nameof(Guid);
            if (Currency.TryParse(raw, out var _)) return nameof(Currency);
            if (Uri.TryCreate(raw, UriKind.Absolute, out _)) return nameof(Uri);
            return "string";
        }

        private GeneratedClass FromObject(JsonElement element, string propertyName)
        {
            var className = GetClassName(propertyName);
            var generated = new GeneratedClass { ClassName = className };
            using var objs = element.EnumerateObject();
            while (objs.MoveNext())
            {
                var name = objs.Current.Name;
                var nested = FromElement(objs.Current.Value, name);
                if (!nested.IsPrimitive)
                {
                    var key = nested.ToString(Settings, 0, true);
                    if (_generatedClasses.TryGetValue(key, out var n))
                    {
                        n.ExampleValues.AddRange(nested.ExampleValues);
                        nested = n;
                    }
                    else
                    {
                        while (_generatedClasses.ContainsKey(nested.ClassName + nested.ClassId))
                            nested.ClassId++;
                        _generatedClasses.Add(nested.ClassName + nested.ClassId, nested);
                        if (nested.ClassId > 0)
                            nested.ClassName += nested.ClassId;
                        _generatedClasses.Add(key, nested);
                    }
                }
                generated.Properties.Add(name, nested);
            }
            return generated;
        }

        private GeneratedClass FromArray(JsonElement element, string propertyName)
        {
            using var objs = element.EnumerateArray();
            var className = propertyName;
            if (className.EndsWith("es"))
                className = className.Substring(0, className.Length - 2);
            else if (className.EndsWith("s"))
                className = className.Substring(0, className.Length - 1);
            className = GetClassName(className);
            var generated = new GeneratedClass { ClassName = className, IsArray = true };
            while (objs.MoveNext())
            {
                var nested = FromElement(objs.Current, propertyName);
                if (nested.IsPrimitive)
                {
                    generated.ExampleValues.AddRange(nested.ExampleValues);
                    if (generated.IsPrimitive && generated.ClassName != nested.ClassName)
                        generated.ClassName = "string";
                    else if (!generated.IsPrimitive)
                    {
                        generated.ClassName = nested.ClassName;
                        generated.IsPrimitive = true;
                    }
                }
                else
                    foreach (var p in nested.Properties)
                        if (!generated.Properties.ContainsKey(p.Key))
                            generated.Properties.Add(p.Key, p.Value);
            }
            return generated;
        }

        private static string GetClassName(string propertyName)
        {
            var className = string.Join(string.Empty, propertyName.Split('_')
                .Select(s => $"{s.Substring(0, 1).ToUpper()}{s.Substring(1, s.Length - 1)}"));
            className += className.EndsWith("Data") ? "Info" : "Data";
            if (!className[0].IsLetter())
            {
                var names = className.TakeWhile(c => !c.IsLetter()).GetCharacterNames().ToList();
                names.Add(className.Substring(names.Count, className.Length - names.Count));
                className = string.Join("_", names);
            }
            return className;
        }
    }

    internal class GeneratedClass
    {
        public Dictionary<string, GeneratedClass> Properties { get; } = new Dictionary<string, GeneratedClass>();
        public HashSet<string> ExampleValues { get; } = new HashSet<string>();
        public string ClassName { get; set; }
        public int ClassId { get; set; }
        public bool IsArray { get; set; }
        public bool IsPrimitive { get; set; }

        public override string ToString()
        {
            return ToString(new JsonClassGeneratorSettings());
        }

        public string ToString(JsonClassGeneratorSettings settings, int depth = 0, bool asKey = false)
        {
            var builder = new StringBuilder();
            if (!asKey)
                builder.Append(settings.IndentChar, depth)
                    .Append($"{settings.ClassVisibility.ToString().ToLower()} class {ClassName}")
                    .AppendLine();
            builder.Append(settings.IndentChar, depth).Append('{')
                .AppendLine();
            depth++;
            foreach (var p in Properties)
            {
                builder.Append(settings.IndentChar, depth)
                    .Append($"{settings.PropertyVisibility.ToString().ToLower()} {p.Value.ClassName}");
                if (p.Value.IsArray)
                    builder.Append("[]");
                builder.Append($" {p.Key} {{ get; set; }}");
                if (!asKey)
                    foreach (var v in p.Value.ExampleValues)
                        builder.Append($" // {v}");
                builder.AppendLine();
            }
            if (depth == 1)
            {
                foreach (var p in GetAllClasses().Distinct())
                    if (!p.IsPrimitive || p.Properties.Count > 0)
                    {
                        builder.AppendLine().Append(p.ToString(settings, depth, asKey));
                    }
            }
            depth--;
            builder.Append(settings.IndentChar, depth).Append('}').AppendLine();
            return builder.ToString();
        }

        private IEnumerable<GeneratedClass> GetAllClasses()
        {
            foreach (var p in Properties)
            {
                yield return p.Value;
                foreach (var n in p.Value.GetAllClasses())
                    yield return n;
            }
        }
    }
}