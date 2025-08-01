using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace TemplR.Configuration;

internal static partial class DollarBracesRegex
{
    [GeneratedRegex(@"\${([a-zA-Z0-9_-]+)}")]
    public static partial Regex GetRegex();
}

public class DollarBracesVariableNamingStrategy : VariableNamingStrategy
{
    public override string FormatVariableName(string variableName)
    {
        return "${" + variableName + "}";
    }

    public override bool TryConvertStringToVariable<T>(string str, [NotNullWhen(true)] out Variable<T>? variable)
    {
        var match = DollarBracesRegex.GetRegex().Match(str);
        if (match.Success)
        {
            var variableName = match.Groups[1].Value;
            variable = new NamedVariable<T>(variableName);
            return true;
        }

        variable = null;
        return false;
    }
}
