using System.Text.Json;
using TemplR.Configuration;

namespace TemplR.System.Text.Json;

internal static class TemplateJsonWriter
{
    public static void Write<T>(Utf8JsonWriter writer, Template<T> value, JsonSerializerOptions options, TemplRJsonOptions templrOptions)
    {
        switch (value)
        {
            case null:
            {
                WriteDefault<T>(writer, options);
                break;
            }
            case Variable<T> var:
            {
                WriteVariable(writer, var, options, templrOptions);
                break;
            }
            case Constant<T> val:
            {
                WriteConstant(writer, val, options);
                break;
            }
            case TypedTemplateObject<T> dyn:
            {
                WriteObject(writer, dyn.PropertyValues, options);
                break;
            }
            case UntypedTemplateObject obj:
            {
                WriteObject(writer, obj.PropertyValues, options);
                break;
            }
            default: 
            {
                WriteValue(writer, value, options);
                break;
            }
        }
    }

    private static void WriteVariable<T>(Utf8JsonWriter writer, Variable<T> variable, JsonSerializerOptions options, TemplRJsonOptions templrOptions)
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
            var variableNamingStrategy = templrOptions.VariableNamingStrategy ?? VariableNamingStrategy.Default;
            writer.WriteStringValue(variableNamingStrategy.FormatVariableName(variableName));
        }
    }

    private static void WriteConstant<T>(Utf8JsonWriter writer, Constant<T> val, JsonSerializerOptions options)
    {
        var valueType = val.Value?.GetType() ?? typeof(object);
        JsonSerializer.Serialize(writer, val.Value, valueType, options);
    }

    private static void WriteDefault<T>(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        var defaultValue = default(T);
        WriteValue(writer, defaultValue, options);
    }

    private static void WriteObject(Utf8JsonWriter writer, IReadOnlyDictionary<string, Template> properties, JsonSerializerOptions options) 
    {
        writer.WriteStartObject();
        foreach (var propertyValue in properties)
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
