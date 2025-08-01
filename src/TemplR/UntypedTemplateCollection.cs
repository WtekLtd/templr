using System.Collections;

namespace TemplR;

public class UntypedTemplateCollection(IEnumerable<Template> children) : Template, IEnumerable<Template>
{
    private readonly IList<Template> _children = [.. children];

    public override void SetContext(Template parent, string propertyName)
    {
        base.SetContext(parent, propertyName);

        var index = 0;
        foreach (var child in children) 
        {
            child.SetContext(this, $"{propertyName}_{index++}");
        }
    }

    public IEnumerator<Template> GetEnumerator()
    {
        return _children.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
