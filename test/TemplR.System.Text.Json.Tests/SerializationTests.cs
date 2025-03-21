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
            StringProp = StronglyTypedVariables.MyString,
            IntProp = StronglyTypedVariables.MyInt,
            BoolProp = StronglyTypedVariables.MyBool,
            NullableDecimalProp = StronglyTypedVariables.MyNullableDecimal
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
            StringProp = StronglyTypedVariables.MyString,
            IntProp = StronglyTypedVariables.MyInt,
            BoolProp = StronglyTypedVariables.MyBool,
            NullableDecimalProp = StronglyTypedVariables.MyNullableDecimal
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
            StringProp = StronglyTypedVariables.MyString
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
                StringProp = StronglyTypedVariables.MyString,
                IntProp = StronglyTypedVariables.MyInt,
                BoolProp = StronglyTypedVariables.MyBool,
                NullableDecimalProp = StronglyTypedVariables.MyNullableDecimal
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
            ContainerStringProp = StronglyTypedVariables.MyContainerString,
            TestClassProp = new TestClassTemplate
            {
                StringProp = StronglyTypedVariables.MyString,
                IntProp = StronglyTypedVariables.MyInt,
                BoolProp = StronglyTypedVariables.MyBool,
                NullableDecimalProp = StronglyTypedVariables.MyNullableDecimal
            }
        };

        var json = JsonSerializer.Serialize(nested, SerializerOptions);

        var expectedJson = """
        {
            "containerStringProp": "${myContainerString}",
            "testClassProp": {
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
    public void SerializeExpressionType_WithDefaultVariables_TokensSerializedAsPlaceholders()
    {
        var template = From.Expression<TestClass, DefaultVariables>((vars) => new()
        {
            StringProp = vars.MyStringProp,
            BoolProp = vars.MyBoolProp,
            IntProp = 100
        });

        var json = JsonSerializer.Serialize(template, SerializerOptions);

        var expectedJson = """
        {
            "stringProp": "${myStringProp}",
            "intProp": 100,
            "boolProp": "${myBoolProp}"
        }
        """;
        JsonAssert.Equal(expectedJson, json);
    }

    [Fact]
    public void SerializeExpressionType_WithIndividuallyNamedVariables_TokensSerializedAsPlaceholders()
    {
        var template = From.Expression<TestClass, IndividuallyNamedVariables>((vars) => new()
        {
            StringProp = vars.MyStringProp,
            BoolProp = vars.MyBoolProp,
            IntProp = 100
        });

        var json = JsonSerializer.Serialize(template, SerializerOptions);

        var expectedJson = """
        {
            "stringProp": "${MyStringProp}",
            "intProp": 100,
            "boolProp": "${myBoolProp}"
        }
        """;
        JsonAssert.Equal(expectedJson, json);
    }

    [Fact]
    public void SerializeNestedExpressionType_NonCamelCaseVariables_TokensSerializedAsPlaceholders()
    {
        var template = From.Expression<TestClassContainer, NonCamelCaseVariables>((vars) => new()
        {
            ContainerStringProp = vars.MyStringProp,
            TestClassProp = new()
            {
                StringProp = vars.MyStringProp,
                BoolProp = vars.MyBoolProp,
                IntProp = 100
            }
        });

        var json = JsonSerializer.Serialize(template, SerializerOptions);

        var expectedJson = """
        {
            "containerStringProp": "${MyStringProp}",
            "testClassProp": {
                "stringProp": "${MyStringProp}",
                "intProp": 100,
                "boolProp": "${MyBoolProp}"
            }
        }
        """;
        JsonAssert.Equal(expectedJson, json);
    }

    [Fact]
    public void SerializeGeneratedTypeWithCollection_WithTokenForEntireCollection_TokenSerializedAsPlaceholders()
    {
        var template = new TestClassWithCollectionTemplate
        {
            StringsProp = StronglyTypedVariables.MyStrings
        };

        var json = JsonSerializer.Serialize(template, SerializerOptions);

        var expectedJson = """
        {
            "stringsProp": "${myStrings}"
        }
        """;
        JsonAssert.Equal(expectedJson, json);
    }

    [Fact]
    public void SerializeGeneratedTypeWithCollection_WithChildTokens_ChildTokensSerializedAsList()
    {
        var template = new TestClassWithCollectionTemplate
        {
            StringsProp = From.Collection([
                StronglyTypedVariables.MyString,
                "constantString"
            ])
        };

        var json = JsonSerializer.Serialize(template, SerializerOptions);

        var expectedJson = """
        {
            "stringsProp": [
                "${myString}",
                "constantString"
            ]
        }
        """;
        JsonAssert.Equal(expectedJson , json);
    }

    [Fact]
    public void SerializeGeneratedTypeWithCollection_WithChildDefaultVariable_ChildTokensSerializedAsListUsingIndex()
    {
        var template = new TestClassWithCollectionTemplate
        {
            StringsProp = From.Collection([
                From.Variable<string>(),
                "constantString",
                From.Variable<string>(),
            ])
        };

        var json = JsonSerializer.Serialize(template, SerializerOptions);

        var expectedJson = """
        {
            "stringsProp": [
                "${stringsProp_0}",
                "constantString",
                "${stringsProp_2}"
            ]
        }
        """;
        JsonAssert.Equal(expectedJson, json);
    }

    private static class StronglyTypedVariables
    {
        public static Variable<string> MyString => From.Variable<string>("myString");

        public static Variable<bool> MyBool => From.Variable<bool>("myBool");

        public static Variable<int> MyInt => From.Variable<int>("myInt");

        public static Variable<decimal?> MyNullableDecimal => From.Variable<decimal?>("myNullableDecimal");

        public static Variable<string> MyContainerString => From.Variable<string>("myContainerString");

        public static Variable<IEnumerable<string>> MyStrings => From.Variable<IEnumerable<string>>("myStrings");
    }

    [TemplateVariableSet(UseCamelCase = false)]
    private record NonCamelCaseVariables(string MyStringProp, bool MyBoolProp);

    private record DefaultVariables(string MyStringProp, bool MyBoolProp);

    private record IndividuallyNamedVariables(bool MyBoolProp)
    {
        [TemplateVariable("MyStringProp")]
        public required string MyStringProp { get; init; }
    }
}
