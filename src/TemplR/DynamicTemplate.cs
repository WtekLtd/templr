using System.Collections.ObjectModel;

namespace TemplR;

public class DynamicTemplate<T> : Template<T>
{
    public DynamicTemplate() 
    {
        _propertyValues = new Dictionary<string, Template?>();
        PropertyValues = new ReadOnlyDictionary<string, Template?>(_propertyValues)
;    }

    public DynamicTemplate(IDictionary<string, object?> memberValues) : this()
    {
        foreach (var memberValue in memberValues)
        {
            SetMember(memberValue.Key, memberValue.Value);
        }
    }

    private readonly IDictionary<string, Template?> _propertyValues;

    public IReadOnlyDictionary<string, Template?> PropertyValues { get; }

    public bool TryGetMember(string propertyName, out Template? result)
    {
        var propertyFound = _propertyValues.TryGetValue(propertyName, out var memberValue);
        result = memberValue;
        return propertyFound;
    }

    public void SetMember(string propertyName, object? value)
    {
        if (value is not Template template)
        {
            var type = value?.GetType() ?? typeof(object);
            var templateType = typeof(Constant<>).MakeGenericType(type);
            template = (Template)Activator.CreateInstance(templateType, value)!;
        }

        template.SetContext(this, propertyName);
        _propertyValues[propertyName] = template;
    }
}
