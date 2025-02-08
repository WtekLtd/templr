using System.Text.Json;

namespace TemplR.System.Text.Json.Tests.Utils;

public static class TemplateAssert
{
    public static void IsDynamicConstant<T>(KeyValuePair<string, Template?> property, string expectedName, T expectedValue)
    {
        Assert.Equal(expectedName, property.Key);
        var constant = Assert.IsAssignableFrom<Constant<object>>(property.Value);
        var constantJsonElement = Assert.IsAssignableFrom<JsonElement>(constant.Value);
        var constantValue = constantJsonElement.Deserialize<T>();
        Assert.Equal(expectedValue, constantValue);
    }

    public static void IsDynamicVariable<T>(KeyValuePair<string, Template?> property, string expectedPropertyName, string expectedVariableName)
    {
        Assert.Equal(expectedPropertyName, property.Key);
        var variable = Assert.IsAssignableFrom<Variable<T>>(property.Value);
        Assert.Equal(expectedVariableName, variable.Name);
    }
}
