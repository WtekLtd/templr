namespace TemplR;

public class NamedVariable<T>(string name) : Variable<T>
{
    public override string Name { get; } = name;

    public override bool UsePropertyNamingConvention => false;
}
