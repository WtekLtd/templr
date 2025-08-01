using System.Text.Json;

namespace TemplR.System.Text.Json.Tests.Utils;

public static class TemplateAssert
{
    public static void IsDynamicConstantProperty<T>(KeyValuePair<string, Template> property, string expectedName, T expectedValue)
    {
        Assert.Equal(expectedName, property.Key);
        IsDynamicConstant(property.Value, expectedValue);
    }

    public static void IsDynamicConstant<T>(Template template, T expectedValue)
    {
        var constant = Assert.IsAssignableFrom<Constant<T>>(template);
        Assert.Equal(expectedValue, constant.Value);
    }

    public static void IsDynamicVariableProperty<T>(KeyValuePair<string, Template> property, string expectedName, string expectedVariableName)
    {
        Assert.Equal(expectedName, property.Key);
        IsDynamicVariable<T>(property.Value, expectedVariableName);
    }

    public static void IsDynamicVariable<T>(Template template, string expectedVariableName)
    {
        var variable = Assert.IsAssignableFrom<Variable<T>>(template);
        Assert.Equal(expectedVariableName, variable.Name);
    }
}
