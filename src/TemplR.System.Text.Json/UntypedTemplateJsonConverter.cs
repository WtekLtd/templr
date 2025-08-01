using System.Text.Json;
using System.Text.Json.Serialization;
using TemplR.Configuration;

namespace TemplR.System.Text.Json;

internal class UntypedTemplateJsonConverter(TemplRJsonOptions templrOptions) : JsonConverter<Template>
{
    public override Template? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch(reader.TokenType)
        {
            case JsonTokenType.StartObject:
            {
                var propertyValues = JsonSerializer.Deserialize<Dictionary<string, Template>>(ref reader, options)!;
                return new UntypedTemplateObject(propertyValues);
            }

            case JsonTokenType.StartArray:
            {
                var collectionElements = JsonSerializer.Deserialize<IEnumerable<Template>>(ref reader, options)!;
                return new UntypedTemplateCollection(collectionElements);
            }

            case JsonTokenType.False:
            case JsonTokenType.True:
            {
                return new Constant<bool>(reader.GetBoolean());
            }

            case JsonTokenType.Number:
            {
                return new Constant<decimal>(reader.GetDecimal());
            }

            case JsonTokenType.String:
            {
                var stringValue = reader.GetString()!;
                var variableNamingPolicy = templrOptions.VariableNamingStrategy ?? VariableNamingStrategy.Default;
                if (variableNamingPolicy.TryConvertStringToVariable<object>(stringValue, out var variable))
                {
                    return variable;
                }
                else
                {
                    return new Constant<string>(reader.GetString());
                }
            }

            default:
            {
                throw new JsonException($"Unexpected json token ${reader.TokenType} at position ${reader.Position}");
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, Template value, JsonSerializerOptions options)
    {
        TemplateJsonWriter.Write<object>(writer, value, options, templrOptions);
    }
}
