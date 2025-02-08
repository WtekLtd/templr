using System.Text.Json;
using Quibble.Xunit;
using TemplR.System.Text.Json.Tests.Models;

namespace TemplR.System.Text.Json.Tests;

public class SerializationTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    static SerializationTests()
    {
        SerializerOptions.UseTemplR();
    }

    [Fact]
    public void SerializeGeneratedType_WithNamedTokens_TokensSerializedAsPlaceholdersWithSpecifiedName()
    {
        var template = new TestClassTemplate
        {
            StringProp = From.Variable("myString"),
            IntProp = From.Variable("myInt"),
            BoolProp = From.Variable("myBool"),
            NullableDecimalProp = From.Variable("myNullableDecimal")
        };

        var json = JsonSerializer.Serialize(template, SerializerOptions);
        
        var expectedJson = """
        {
            "stringProp": "${myString}",
            "intProp": "${myInt}",
            "boolProp": "${myBool}",
            "nullableDecimalProp": "${myNullableDecimal}"
        }
        """;
        JsonAssert.Equal(expectedJson, json);
    }

    [Fact]
    public void SerializeGenereatedType_WithUnamedVariables_TokensSerializedAsPlaceholdersWithPropertyName()
    {
        var template = new TestClassTemplate
        {
            StringProp = From.Variable(),
            IntProp = From.Variable(),
            BoolProp = From.Variable(),
            NullableDecimalProp = From.Variable()
        };

        var json = JsonSerializer.Serialize(template, SerializerOptions);

        var expectedJson = """
        {
            "stringProp": "${stringProp}",
            "intProp": "${intProp}",
            "boolProp": "${boolProp}",
            "nullableDecimalProp": "${nullableDecimalProp}"
        }
        """;
        JsonAssert.Equal(expectedJson, json);
    }

    [Fact]
    public void SerializeGeneratedType_WithConstantTokenValues_TokensSerializedUsingConstantValue()
    {
        var template = new TestClassTemplate
        {
            StringProp = "myConstantString",
            IntProp = 42,
            BoolProp = true,
            NullableDecimalProp = 42.5M
        };

        var json = JsonSerializer.Serialize(template, SerializerOptions);

        var expectedJson = """
        {
            "stringProp": "myConstantString",
            "intProp": 42,
            "boolProp": true,
            "nullableDecimalProp": 42.5
        }
        """;
        JsonAssert.Equal(expectedJson, json);
    }

    [Fact]
    public void SerializeGeneratedType_WithNamedTokensAndSomeValuesSpecified_TokensSerializedAsValuesWhereAvailable()
    {
        var template = new TestClassTemplate
        {
            StringProp = From.Variable("myString"),
            IntProp = From.Variable("myInt"),
            BoolProp = From.Variable("myBool"),
            NullableDecimalProp = From.Variable("myNullableDecimal")
        };
        template.SetVariables(new()
        {
            { "myInt", 42 }
        });

        var json = JsonSerializer.Serialize(template, SerializerOptions);

        var expectedJson = """
        {
            "stringProp": "${myString}",
            "intProp": 42,
            "boolProp": "${myBool}",
            "nullableDecimalProp": "${myNullableDecimal}"
        }
        """;
        JsonAssert.Equal(expectedJson, json);
    }

    [Fact]
    public void SerializeGeneratedType_WithPropertiesLeftWithDefaultValues_SerializesUsingDefaultValues()
    {
        var template = new TestClassTemplate
        {
            StringProp = From.Variable("myString")
        };

        var json = JsonSerializer.Serialize(template, SerializerOptions);

        var expectedJson = """
        {
            "stringProp": "${myString}",
            "intProp": 0,
            "boolProp": false,
            "nullableDecimalProp": null
        }
        """;
        JsonAssert.Equal(expectedJson, json);
    }

    [Fact]
    public void SerializeWrappedGeneratedType_WithNamedTokens_WrappedTemplateSerializedWithTokenPlaceholders()
    {
        var wrapper = new TestClassWrapper
        {
            WrapperStringProp = "stringValue",
            Child = new TestClassTemplate
            {
                StringProp = From.Variable("myString"),
                IntProp = From.Variable("myInt"),
                BoolProp = From.Variable("myBool"),
                NullableDecimalProp = From.Variable("myNullableDecimal")
            }
        };

        var json = JsonSerializer.Serialize(wrapper, SerializerOptions);

        var expectedJson = """
        {
            "wrapperStringProp": "stringValue",
            "child": {
                "stringProp": "${myString}",
                "intProp": "${myInt}",
                "boolProp": "${myBool}",
                "nullableDecimalProp": "${myNullableDecimal}"
            }
        }
        """;
        JsonAssert.Equal(expectedJson, json);
    }

    [Fact]
    public void SerialzeNestedGeneratedType_WithNamedTokens_TokensSerializedAsPlaceholdersForEntireObjectTree()
    {
        var nested = new TestClassContainerTemplate
        {
            ContainerStringProp = From.Variable("myContainerStringProp"),
            TestClassProp = new TestClassTemplate
            {
                StringProp = From.Variable("myStringProp"),
                IntProp = From.Variable("myIntProp"),
                BoolProp = From.Variable("myBoolProp"),
                NullableDecimalProp = From.Variable("myNullableDecimalProp")
            }
        };

        var json = JsonSerializer.Serialize(nested, SerializerOptions);

        var expectedJson = """
        {
            "containerStringProp": "${myContainerStringProp}",
            "testClassProp": {
                "stringProp": "${myStringProp}",
                "intProp": "${myIntProp}",
                "boolProp": "${myBoolProp}",
                "nullableDecimalProp": "${myNullableDecimalProp}"
            }
        }
        """;
        JsonAssert.Equal(expectedJson, json);
    }
}