namespace TemplR;

[AttributeUsage(AttributeTargets.Class)]
public class TemplateVariableSet() : Attribute
{
    public bool UseCamelCase { get; set; }
}