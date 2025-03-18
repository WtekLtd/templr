using TemplR.Configuration;

namespace TemplR.System.Text.Json.Tests.Configuration;

public class DollarOptionalBracesVariableNamingStrategyTests
{
    [Fact]
    public void FormatVariableName_FormatsWithDollarAndBraces()
    {
        var variableNamingStrategy = new DollarOptionalBracesVariableNamingStrategy();
        var formattedName = variableNamingStrategy.FormatVariableName("myVariable");
        Assert.Equal("${myVariable}", formattedName);
    }

    [Fact]
    public void TryConvertStringToVariable_WithDollarAndBraces_ReturnsTrueAndVariable()
    {
        var variableNamingStrategy = new DollarOptionalBracesVariableNamingStrategy();
        var isVariable = variableNamingStrategy.TryConvertStringToVariable<object>("${myVariable}", out var variable);
        Assert.True(isVariable);
        Assert.Equal("myVariable", variable!.Name);
    }

    [Fact]
    public void TryConvertStringToVariable_WithDollarOnly_ReturnsTrueAndVariable()
    {
        var variableNamingStrategy = new DollarOptionalBracesVariableNamingStrategy();
        var isVariable = variableNamingStrategy.TryConvertStringToVariable<object>("$myVariable", out var variable);
        Assert.True(isVariable);
        Assert.Equal("myVariable", variable!.Name);
    }

    [Fact]
    public void TryConvertStringToVariable_WithIncorrectlyFormattedVariable_ReturnsFalseAndNullVariable()
    {
        var variableNamingStrategy = new DollarOptionalBracesVariableNamingStrategy();
        var isVariable = variableNamingStrategy.TryConvertStringToVariable<object>("myVariable", out var variable);
        Assert.False(isVariable);
        Assert.Null(variable);
    }
}