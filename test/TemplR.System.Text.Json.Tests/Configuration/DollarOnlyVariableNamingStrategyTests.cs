using TemplR.Configuration;

namespace TemplR.System.Text.Json.Tests.Configuration;

public class DollarOnlyVariableNamingStrategyTests
{
    [Fact]
    public void FormatVariableName_FormatsWithOnlyDollar()
    {
        var variableNamingStrategy = new DollarOnlyVariableNamingStrategy();
        var formattedName = variableNamingStrategy.FormatVariableName("myVariable");
        Assert.Equal("$myVariable", formattedName);
    }

    [Fact]
    public void TryConvertStringToVariable_WithCorrectlyFormattedVariable_ReturnsTrueAndVariable()
    {
        var variableNamingStrategy = new DollarOnlyVariableNamingStrategy();
        var isVariable = variableNamingStrategy.TryConvertStringToVariable<object>("$myVariable", out var variable);
        Assert.True(isVariable);
        Assert.Equal("myVariable", variable!.Name);
    }

    [Fact]
    public void TryConvertStringToVariable_WithIncorrectlyFormattedVariable_ReturnsFalseAndNullVariable()
    {
        var variableNamingStrategy = new DollarOnlyVariableNamingStrategy();
        var isVariable = variableNamingStrategy.TryConvertStringToVariable<object>("myVariable", out var variable);
        Assert.False(isVariable);
        Assert.Null(variable);
    }

    [Fact]
    public void TryConvertStringToVariable_WithBraces_ReturnsFalseAndNullVariable()
    {
        var variableNamingStrategy = new DollarOnlyVariableNamingStrategy();
        var isVariable = variableNamingStrategy.TryConvertStringToVariable<object>("${myVariable}", out var variable);
        Assert.False(isVariable);
        Assert.Null(variable);
    }
}
