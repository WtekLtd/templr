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

    public static TypedTemplateCollection<T> Collection<T>(IEnumerable<Template<T>> templates)
    {
        return new TypedTemplateCollection<T>(templates);
    }

    public static Template<TTemplate> Expression<TTemplate, TVariables>(
        Expression<Func<TVariables, TTemplate>> expression
    )
    {
        var varsParameter = expression.Parameters.First();
        var useCamelCase = varsParameter.Type.GetCustomAttribute<TemplateVariableSetAttribute>(true)?.UseCamelCase ?? true;
        return (Template<TTemplate>)ConvertExpressionToTemplate(expression.Body, varsParameter, typeof(TTemplate), useCamelCase);
    }

    private static Template ConvertExpressionToTemplate(Expression expression, ParameterExpression varsParameter, Type type, bool useCamelCase)
    {
        Type templateGenericType;
        object templateParameter;
        if (expression is MemberInitExpression memberInitExpr)
        {
            var memberValues = new Dictionary<string, Template>();
            foreach (var memberAssignmentExpr in memberInitExpr.Bindings.OfType<MemberAssignment>())
            {
                var propertyName = memberAssignmentExpr.Member.Name;
                var propertyType = memberAssignmentExpr.Expression.Type;
                var memberTemplate = ConvertExpressionToTemplate(memberAssignmentExpr.Expression, varsParameter, propertyType, useCamelCase);
                memberValues.Add(propertyName, memberTemplate);
            }
            
            templateGenericType = typeof(TypedTemplateObject<>);
            templateParameter = memberValues;
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

            templateGenericType = typeof(NamedVariable<>);
            templateParameter = variableName;
        }
        else
        {
            var valueLambda = System.Linq.Expressions.Expression.Lambda(expression).Compile();
            var value = valueLambda.DynamicInvoke();

            templateGenericType = typeof(Constant<>);
            templateParameter = value;
        }

        var templateType = templateGenericType.MakeGenericType(type);
        var template = (Template)Activator.CreateInstance(templateType, templateParameter);

        return template;
    }
}
