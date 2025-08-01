using System.Text.Json;
using TemplR.System.Text.Json.Tests.Models;
using TemplR.System.Text.Json.Tests.Utils;

namespace TemplR.System.Text.Json.Tests;

public class DeserializationTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    static DeserializationTests()
    {
        SerializerOptions.UseTemplR();
    }

    [Fact]
    public void DeserializeToGeneratedType_WithNamedTokens_SetsAllPropertiesToVariables()
    {
        var json = """
        {
            "stringProp": "${myString}",
            "intProp": "${myInt}",
            "boolProp": "${myBool}",
            "nullableDecimalProp": "${myNullableDecimal}"
        }
        """;
        
        var template = JsonSerializer.Deserialize<TestClassTemplate>(json, SerializerOptions);

        Assert.NotNull(template);

        var stringPropVariable = Assert.IsAssignableFrom<Variable<string>>(template.StringProp);
        Assert.Equal("myString", stringPropVariable.Name);

        var intPropVariable = Assert.IsAssignableFrom<Variable<int>>(template.IntProp);
        Assert.Equal("myInt", intPropVariable.Name);

        var boolPropVariable = Assert.IsAssignableFrom<Variable<bool>>(template.BoolProp);
        Assert.Equal("myBool", boolPropVariable.Name);

        var nullableDecimalPropVariable = Assert.IsAssignableFrom<Variable<decimal?>>(template.NullableDecimalProp);
        Assert.Equal("myNullableDecimal", nullableDecimalPropVariable.Name);
    }

    [Fact]
    public void DeserializeToGeneratedType_WithConstantValues_SetsAllPropertiesToConstants()
    {
        var json = """
        {
            "stringProp": "myString",
            "intProp": 42,
            "boolProp": false,
            "nullableDecimalProp": 42.5
        }
        """;

        var template = JsonSerializer.Deserialize<TestClassTemplate>(json, SerializerOptions);

        Assert.NotNull(template);
        
        var stringPropConstant = Assert.IsAssignableFrom<Constant<string>>(template.StringProp);
        Assert.Equal("myString", stringPropConstant.Value);
        
        var intPropConstant = Assert.IsAssignableFrom<Constant<int>>(template.IntProp);
        Assert.Equal(42, intPropConstant.Value);

        var boolPropConstant = Assert.IsAssignableFrom<Constant<bool>>(template.BoolProp);
        Assert.False(boolPropConstant.Value);

        var nullableDecimalConstant = Assert.IsAssignableFrom<Constant<decimal?>>(template.NullableDecimalProp);
        Assert.Equal(42.5M, nullableDecimalConstant.Value);
    }

    [Fact]
    public void DeserializeToGeneratedType_WithMissingProperties_SetsAllMissingPropertiesToDefaultConstants()
    {
        var json = """
        {
            "stringProp": "myString"
        }
        """;

        var template = JsonSerializer.Deserialize<TestClassTemplate>(json, SerializerOptions);

        Assert.NotNull(template);

        var intPropConstant = Assert.IsAssignableFrom<Constant<int>>(template.IntProp);
        Assert.Equal(0, intPropConstant.Value);

        var boolPropConstant = Assert.IsAssignableFrom<Constant<bool>>(template.BoolProp);
        Assert.False(boolPropConstant.Value);

        var nullableDecimalConstant = Assert.IsAssignableFrom<Constant<decimal?>>(template.NullableDecimalProp);
        Assert.Null(nullableDecimalConstant.Value);
    }

    [Fact]
    public void DeserializeToTemplate_WithNamedTokens_CreatesDynamicTemplateWithVariables()
    {
        var json = """
        {
            "stringProp": "${myString}",
            "intProp": "${myInt}",
            "boolProp": "${myBool}",
            "nullableDecimalProp": "${myNullableDecimal}"
        }
        """;

        var template = JsonSerializer.Deserialize<Template>(json, SerializerOptions);

        var dynamicTemplate = Assert.IsAssignableFrom<UntypedTemplateObject>(template);
        Assert.Collection(
            dynamicTemplate.PropertyValues,
            (prop) => TemplateAssert.IsDynamicVariableProperty<object>(prop, "stringProp", "myString"),
            (prop) => TemplateAssert.IsDynamicVariableProperty<object>(prop, "intProp", "myInt"),
            (prop) => TemplateAssert.IsDynamicVariableProperty<object>(prop, "boolProp", "myBool"),
            (prop) => TemplateAssert.IsDynamicVariableProperty<object>(prop, "nullableDecimalProp", "myNullableDecimal")
        );
    }

    [Fact]
    public void DeserializeToTemplate_WithConstantValues_CreatesDynamicTemplateWithConstants()
    {
        var json = """
        {
            "stringProp": "myString",
            "boolProp": true,
            "nullableDecimalProp": 42.5
        }
        """;

        var template = JsonSerializer.Deserialize<Template>(json, SerializerOptions);

        var dynamicTemplate = Assert.IsAssignableFrom<UntypedTemplateObject>(template);
        Assert.Collection(
            dynamicTemplate.PropertyValues,
            (prop) => TemplateAssert.IsDynamicConstantProperty(prop, "stringProp", "myString"),
            (prop) => TemplateAssert.IsDynamicConstantProperty(prop, "boolProp", true),
            (prop) => TemplateAssert.IsDynamicConstantProperty(prop, "nullableDecimalProp", 42.5M)
        );
    }

    [Fact]
    public void DeserializeToUntypedTemplate_WithCollection_CreatesUntypedTemplateWithCollection()
    {
        var json = """
        {
            "stringsProp": [
                "${myString1}",
                "myConstant",
                "${myString2}"
            ]
        }
        """;

        var template = JsonSerializer.Deserialize<Template>(json, SerializerOptions);

        var dynamicTemplate = Assert.IsAssignableFrom<UntypedTemplateObject>(template);
        var propertyValue = Assert.Single(dynamicTemplate.PropertyValues.Values);
        var templateCollection = Assert.IsAssignableFrom<UntypedTemplateCollection>(propertyValue);

        Assert.Collection(
            templateCollection,
            (x) => TemplateAssert.IsDynamicVariable<object>(x, "myString1"),
            (x) => TemplateAssert.IsDynamicConstant(x, "myConstant"),
            (x) => TemplateAssert.IsDynamicVariable<object>(x, "myString2")
        );
    }

    [Fact]
    public void DeserializeToTypedTemplate_WithCollection_CreatesUntypedTemplateWithCollection()
    {
        var json = """
        {
            "stringsProp": [
                "${myString1}",
                "myConstant",
                "${myString2}"
            ]
        }
        """;

        var template = JsonSerializer.Deserialize<Template<TestClassWithCollection>>(json, SerializerOptions);

        var dynamicTemplate = Assert.IsAssignableFrom<TypedTemplateObject<TestClassWithCollection>>(template);
        var propertyValue = Assert.Single(dynamicTemplate.PropertyValues.Values);
        var templateCollection = Assert.IsAssignableFrom<UntypedTemplateCollection>(propertyValue);

        Assert.Collection(
            templateCollection,
            (x) => TemplateAssert.IsDynamicVariable<object>(x, "myString1"),
            (x) => TemplateAssert.IsDynamicConstant(x, "myConstant"),
            (x) => TemplateAssert.IsDynamicVariable<object>(x, "myString2")
        );
    }

    [Fact]
    public void DeserializeToGeneratedType_WithCollection_CreatesDynamicTypeWithCollection()
    {
        var json = """
        {
            "stringsProp": [
                "${myString1}",
                "myConstant",
                "${myString2}"
            ]
        }
        """;

        var template = JsonSerializer.Deserialize<TestClassWithCollectionTemplate>(json, SerializerOptions);

        Assert.NotNull(template);
        var templateCollection = Assert.IsAssignableFrom<TypedTemplateCollection<string>>(template.StringsProp);

        Assert.Collection(
            templateCollection,
            (x) => TemplateAssert.IsDynamicVariable<string>(x, "myString1"),
            (x) => TemplateAssert.IsDynamicConstant(x, "myConstant"),
            (x) => TemplateAssert.IsDynamicVariable<string>(x, "myString2")
        );
    }
}
