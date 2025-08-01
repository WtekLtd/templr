using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace TemplR.Configuration;

internal static partial class DollarOptionalBracesRegex
{
    [GeneratedRegex(@"\$(\{([a-zA-Z0-9_-]+)\}|([a-zA-Z0-9_-]+))")]
    public static partial Regex GetRegex();
}

public class DollarOptionalBracesVariableNamingStrategy : VariableNamingStrategy
{
    public override string FormatVariableName(string variableName)
    {
        return "${" + variableName + "}";
    }

    public override bool TryConvertStringToVariable<T>(string str, [NotNullWhen(true)] out Variable<T>? variable)
    {
        var match = DollarOptionalBracesRegex.GetRegex().Match(str);
        if (match.Success)
        {
            var variableName = match.Groups[2].Success
                ? match.Groups[2].Value
                : match.Groups[3].Value;
            variable = new NamedVariable<T>(variableName);
            return true;
        }

        variable = null;
        return false;
    }
}