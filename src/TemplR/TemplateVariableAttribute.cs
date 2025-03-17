namespace TemplR;

[AttributeUsage(AttributeTargets.Property)]
public class TemplateVariableAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
