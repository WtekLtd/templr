namespace TemplR;

public class Variable(string? name)
{
    public string? Name { get; } = name;
}

public abstract class Variable<T> : Template<T>
{
    public abstract string Name { get; }

    public abstract bool UsePropertyNamingConvention { get; }
}
