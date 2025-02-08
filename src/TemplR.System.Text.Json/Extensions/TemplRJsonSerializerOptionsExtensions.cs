using System.Text.Json;
using TemplR.System.Text.Json;

namespace TemplR;

public static class TemplRJsonSerializerOptionsExtensions 
{
    public static void UseTemplR(this JsonSerializerOptions options)
    {
        if (!options.Converters.Any((c) => c is TemplateJsonConverterFactory))
        {
            options.Converters.Add(new TemplateJsonConverterFactory());
        }
    }
}