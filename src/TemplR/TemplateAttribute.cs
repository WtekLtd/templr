namespace TemplR;

[AttributeUsage(AttributeTargets.Class)]
public class TemplateAttribute(Type targetType) : Attribute 
{
    public Type TargetType { get; } = targetType;
}
