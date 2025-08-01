using System.Collections.ObjectModel;

namespace TemplR;

public class UntypedTemplateObject(IDictionary<string, Template> properties) : Template
{
    public IReadOnlyDictionary<string, Template> PropertyValues { get; } = new ReadOnlyDictionary<string, Template>(properties);

    public override void SetContext(Template parent, string propertyName)
    {
        base.SetContext(parent, propertyName);

        foreach (var property in PropertyValues)
        {
            property.Value.SetContext(this, property.Key);
        }
    }
}
