namespace TemplR;

public class DefaultVariable<T> : Variable<T>
{
    public override string Name => PropertyName ?? "default";

    public override bool UsePropertyNamingConvention => PropertyName != null;
}