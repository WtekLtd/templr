using System.Linq.Expressions;
using System.Reflection;

namespace TemplR;

public static class From
{
    public static Constant<T> Constant<T>(T value)
    {
        return new Constant<T>(value);
    }

    public static Variable Variable(string? variableName = null)
    {
        return new Variable(variableName);
    }

    public static Variable<T> Variable<T>(string? variableName = null)
    {
        return variableName != null
            ? new NamedVariable<T>(variableName)
            : new DefaultVariable<T>();
    }

    public static Template<TTemplate> Expression<TTemplate, TVariables>(
        Expression<Func<TVariables, TTemplate>> expression
    )
    {
        var varsParameter = expression.Parameters.First();
        var useCamelCase = varsParameter.Type.GetCustomAttribute<TemplateVariableSet>(true)?.UseCamelCase ?? true;
        return ConvertExpressionToTemplate<TTemplate>(expression.Body, varsParameter, useCamelCase);
    }

    private static Template<T> ConvertExpressionToTemplate<T>(Expression expression, ParameterExpression varsParameter, bool useCamelCase)
    {
        Template<T> template;
        if (expression is MemberInitExpression memberInitExpr)
        {
            var memberValues = new Dictionary<string, object?>();
            foreach (var memberAssignmentExpr in memberInitExpr.Bindings.OfType<MemberAssignment>())
            {
                var propertyName = memberAssignmentExpr.Member.Name;
                var memberTemplate = ConvertExpressionToTemplate<object>(memberAssignmentExpr.Expression, varsParameter, useCamelCase);
                memberValues.Add(propertyName, memberTemplate);
            }
            
            template = new DynamicTemplate<T>(memberValues);
        }
        else if (expression is MemberExpression varsExpr && varsExpr.Expression == varsParameter)
        {
            var templateVariableAttribute = varsExpr.Member.GetCustomAttribute<TemplateVariableAttribute>();
            var variableName = varsExpr.Member.Name;
            if (templateVariableAttribute != null)
            {
                variableName = templateVariableAttribute.Name;
            }
            else if (useCamelCase)
            {
                variableName = new string([..variableName.Select((c, i) => i == 0 ? char.ToLowerInvariant(c) : c)]);
            }

            template = new NamedVariable<T>(variableName);
        }
        else
        {
            var valueLambda = System.Linq.Expressions.Expression.Lambda(expression).Compile();
            var value = (T)valueLambda.DynamicInvoke();

            template = new Constant<T>(value);
        }
        return template;
    }
}
