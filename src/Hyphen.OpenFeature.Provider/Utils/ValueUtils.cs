using System;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using OpenFeature.Model;

namespace Hyphen.OpenFeature.Provider.Utils
{
    public static class ValueUtils
    {
        public static Value ConvertJsonElementToValue(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return new Value(element.GetString());

                case JsonValueKind.Number:
                    if (element.TryGetInt64(out long longValue))
                        return new Value(longValue);
                    return new Value(element.GetDouble());

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return new Value(element.GetBoolean());

                case JsonValueKind.Array:
                    var list = element.EnumerateArray()
                        .Select(ConvertJsonElementToValue)
                        .ToList();
                    return new Value(list);

                case JsonValueKind.Object:
                    var structure = Structure.Builder();
                    foreach (var prop in element.EnumerateObject())
                    {
                        structure.Set(prop.Name, ConvertJsonElementToValue(prop.Value));
                    }
                    return new Value(structure.Build());

                case JsonValueKind.Null:
                    return new Value((string?)null);

                default:
                    throw new ArgumentException($"Unsupported JSON value kind: {element.ValueKind}");
            }
        }

        public static object ConvertToNative(Value value)
        {
            if (value.IsBoolean) return value.AsBoolean;
            if (value.IsNumber) return value.AsDouble ?? value.AsInteger;
            if (value.IsList) return value.AsList?.Select(ConvertToNative).ToList();
            if (value.IsStructure)
            {
                var dict = new Dictionary<string, object>();
                foreach (var item in value.AsStructure)
                {
                    dict[item.Key] = ConvertToNative(item.Value);
                }
                return dict;
            }
            if (value.IsNull) return null;
            if (value.IsString) return value.AsString;
            return value.AsObject;
        }
    }
}
