using System.Diagnostics.CodeAnalysis;

namespace TemplR.Configuration;

public abstract class VariableNamingStrategy
{
    public static VariableNamingStrategy Default { get; } = new DollarBracesVariableNamingStrategy();

    public static VariableNamingStrategy DollarOnly { get; } = new DollarOnlyVariableNamingStrategy();

    public static VariableNamingStrategy DollarOptionalBraces { get; } = new DollarOptionalBracesVariableNamingStrategy();

    public abstract string FormatVariableName(string variableName);

    public abstract bool TryConvertStringToVariable<T>(string str, [NotNullWhen(true)]out Variable<T>? variable);
}
