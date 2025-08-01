using System.Text.Json;
using System.Text.Json.Serialization;
using TemplR.Configuration;

namespace TemplR.System.Text.Json;

public class TemplateJsonConverterFactory(TemplRJsonOptions templrOptions = default) : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert == typeof(Template) || typeToConvert == typeof(UntypedTemplateObject))
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
                genericType == typeof(TypedTemplateObject<>);
        }

        return false;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var genericType = typeToConvert?.GetGenericArguments().FirstOrDefault();
        if (genericType == null)
        {
            return new UntypedTemplateJsonConverter(templrOptions);
        }
        else
        {
            var converterType = typeof(TypedTemplateJsonConverter<>).MakeGenericType(genericType);
            return (JsonConverter)Activator.CreateInstance(converterType, templrOptions)!;
        }
    }
}
