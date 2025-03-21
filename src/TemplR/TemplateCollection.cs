using System.Collections;

namespace TemplR;

public class TemplateCollection<T>(IEnumerable<Template<T>> templates) : Template<IEnumerable<T>>, IEnumerable<Template<T>>
{
    private readonly IList<Template<T>> _templates = templates.ToList();

    public override void SetContext(Template parent, string propertyName)
    {
        base.SetContext(parent, propertyName);

        var index = 0;
        foreach (var template in _templates)
        {
            template.SetContext(this, $"{propertyName}_{index++}");
        }
    }

    public IEnumerator<Template<T>> GetEnumerator()
    {
        return _templates.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
