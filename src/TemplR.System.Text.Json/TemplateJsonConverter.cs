using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TemplR.System.Text.Json;

public partial class TemplateJsonConverter<T> : JsonConverter<Template<T>>
{
    [GeneratedRegex(@"\${([a-zA-Z0-9_-]+)}")]
    private static partial Regex VariableRegex();

    public override Template<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (stringValue != null)
            {
                var variableMatch = VariableRegex().Match(stringValue);
                if (variableMatch.Success) 
                {
                    var variableName = variableMatch.Groups[1].Value;
                    return new NamedVariable<T>(variableName);
                }
            }
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            var typeInfo = options.GetTypeInfo(typeToConvert);
            if (typeInfo.Properties.Count > 0)
            {
                var template = (Template<T>)JsonSerializer.Deserialize(
                    ref reader,
                    jsonTypeInfo: typeInfo
                )!;
                return template;
            }
            else
            {
                var properties = JsonSerializer.Deserialize<Dictionary<string, Template>>(ref reader, options);
                var template = new DynamicTemplate<T>();
                foreach (var property in properties ?? [])
                {
                    template.SetMember(property.Key, property.Value);
                }
                return template;
            }
        }

        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return new Constant<T>(value);
    }
    
    public override void Write(Utf8JsonWriter writer, Template<T> value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case null:
            {
                WriteDefault(writer, options);
                break;
            }
            case Variable<T> var:
            {
                WriteVariable(writer, var, options);
                break;
            }
            case Constant<T> val:
            {
                WriteConstant(writer, val, options);
                break;
            }
            case DynamicTemplate<T> dyn:
            {
                WriteDynamic(writer, dyn, options);
                break;
            }
            default: 
            {
                WriteValue(writer, value, options);
                break;
            }
        }
    }

    private static void WriteVariable(Utf8JsonWriter writer, Variable<T> variable, JsonSerializerOptions options)
    {
        var variableName = variable.Name;
        if (variable.UsePropertyNamingConvention && options.PropertyNamingPolicy != null)
        {
            variableName = options.PropertyNamingPolicy.ConvertName(variableName);
        }

        if (variable.TryGetVariableValue(variableName, out var variableValue))
        {
            WriteValue(writer, variableValue, options);
        }
        else 
        {
            writer.WriteStringValue($"${{{variableName}}}");
        }
    }

    private static void WriteConstant(Utf8JsonWriter writer, Constant<T> val, JsonSerializerOptions options)
    {
        var valueType = val.Value?.GetType() ?? typeof(object);
        JsonSerializer.Serialize(writer, val.Value, valueType, options);
    }

    private static void WriteDefault(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        var defaultValue = default(T);
        WriteValue(writer, defaultValue, options);
    }

    private static void WriteDynamic(Utf8JsonWriter writer, DynamicTemplate<T> val, JsonSerializerOptions options) 
    {
        writer.WriteStartObject();
        foreach (var propertyValue in val.PropertyValues)
        {
            var propertyName = options.PropertyNamingPolicy?.ConvertName(propertyValue.Key) ?? propertyValue.Key;
            writer.WritePropertyName(propertyName);
            if (propertyValue.Value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                var templateType = propertyValue.Value.GetType().BaseType!;
                JsonSerializer.Serialize(writer, propertyValue.Value, templateType, options);
            }
        }
        writer.WriteEndObject();
    }

    private static void WriteValue(Utf8JsonWriter writer, object? val, JsonSerializerOptions options)
    {
        if (val == null) {
            writer.WriteNullValue();
        } 
        else
        {
            var targetType = val.GetType();
            JsonSerializer.Serialize(writer, val, targetType, options);
        }
    }
}
