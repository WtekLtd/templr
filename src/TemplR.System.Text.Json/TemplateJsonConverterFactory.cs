using System.Text.Json;
using System.Text.Json.Serialization;
using TemplR.Configuration;

namespace TemplR.System.Text.Json;

public class TemplateJsonConverterFactory(TemplRJsonOptions templrOptions = default) : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert == typeof(Template))
        {
            return true;
        }
        else if (typeToConvert.IsGenericType)
        {
            var genericType = typeToConvert.GetGenericTypeDefinition();
            return
                genericType == typeof(Template<>) ||
                genericType == typeof(Constant<>) ||
                genericType == typeof(NamedVariable<>) ||
                genericType == typeof(DefaultVariable<>) ||
                genericType == typeof(Variable<>) ||
                genericType == typeof(DynamicTemplate<>);
        }

        return false;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var genericType = typeToConvert?.GetGenericArguments().FirstOrDefault() ?? typeof(object);
        var converterType = typeof(TemplateJsonConverter<>).MakeGenericType(genericType);
        return (JsonConverter)Activator.CreateInstance(converterType, templrOptions)!;
    }
}
