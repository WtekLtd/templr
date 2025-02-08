namespace TemplR;

public class Constant<T>(T? value) : Template<T>
{
    public T? Value { get; } = value;
}
