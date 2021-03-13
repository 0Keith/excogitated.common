using Excogitated.Common.Atomic.Collections;
using Excogitated.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace Excogitated.Common
{
    public static class ObjectMapper
    {
        private static readonly CowDictionary<(Type, Type), Func<object, object>> _convertors = new();
        public static void Map<Source, Target>(Func<Source, Target> convertor)
        {
            _convertors[(typeof(Source), typeof(Target))] = source => convertor((Source)source);
        }

        public static void CopyTo(this object source, object target)
        {
            source.NotNull(nameof(source));
            target.NotNull(nameof(target));
            var sourceType = source.GetType();
            var targetType = target.GetType();
            var sourceProps = sourceType.GetRuntimeProperties().Where(p => p.CanRead);
            var targetProps = targetType.GetRuntimeProperties().Where(p => p.CanWrite)
                .ToDictionary(p => p.Name);
            foreach (var sourceProp in sourceProps)
                if (targetProps.TryGetValue(sourceProp.Name, out var targetProp))
                    if (targetProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                    {
                        var value = sourceProp.GetValue(source);
                        targetProp.SetValue(target, value);
                    }
                    else if (_convertors.TryGetValue((sourceProp.PropertyType, targetProp.PropertyType), out var convertor))
                    {
                        var value = sourceProp.GetValue(source);
                        var conversion = convertor(value);
                        targetProp.SetValue(target, conversion);
                    }
        }
    }
}