using System.Text.Json;
using System.Text.Json.Serialization;
using TemplR.Configuration;

namespace TemplR.System.Text.Json;

internal class TypedTemplateJsonConverter<T>(TemplRJsonOptions templrOptions) : JsonConverter<Template<T>>
{
    public override Template<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (stringValue != null)
            {
                var variableNamingStrategy = templrOptions.VariableNamingStrategy ?? VariableNamingStrategy.Default;
                if (variableNamingStrategy.TryConvertStringToVariable<T>(stringValue, out var variable))
                {
                    return variable;
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
                var properties = JsonSerializer.Deserialize<Dictionary<string, Template>>(ref reader, options)!;
                var template = new TypedTemplateObject<T>(properties);
                return template;
            }
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            if (typeof(T).GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var elementType = typeof(T).GetGenericArguments().First();
                var elementTemplateType = typeof(Template<>).MakeGenericType(elementType);
                var collectionType = typeof(IEnumerable<>).MakeGenericType(elementTemplateType);

                var elements = JsonSerializer.Deserialize(ref reader, collectionType, options);

                var templateCollectionType = typeof(TypedTemplateCollection<>).MakeGenericType(elementType);
                var templateCollection = Activator.CreateInstance(templateCollectionType, elements);

                if (templateCollection != null) 
                {
                    return (Template<T>)templateCollection;
                }
            }
        }

        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return new Constant<T>(value);
    }
    
    public override void Write(Utf8JsonWriter writer, Template<T> value, JsonSerializerOptions options)
    {
        TemplateJsonWriter.Write(writer, value, options, templrOptions);
    }
}
