namespace TemplR;

[AttributeUsage(AttributeTargets.Class)]
public class TemplateVariableSetAttribute() : Attribute
{
    public bool UseCamelCase { get; set; }
}