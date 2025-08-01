using System.Collections.ObjectModel;

namespace TemplR;

public class TypedTemplateObject<T>(IDictionary<string, Template> propertyValues) : Template<T>
{
    private readonly IDictionary<string, Template> _propertyValues = propertyValues;

    public IReadOnlyDictionary<string, Template> PropertyValues { get; } = new ReadOnlyDictionary<string, Template>(propertyValues);

    public override void SetContext(Template parent, string propertyName)
    {
        base.SetContext(parent, propertyName);

        foreach (var propertyValue in PropertyValues)
        {
            propertyValue.Value.SetContext(this, propertyValue.Key);
        }
    }

    public bool TryGetMember(string propertyName, out Template? result)
    {
        var propertyFound = _propertyValues.TryGetValue(propertyName, out var memberValue);
        result = memberValue;
        return propertyFound;
    }

    public void SetMember(string propertyName, Template value)
    {
        value.SetContext(this, propertyName);
        _propertyValues[propertyName] = value;
    }
}
