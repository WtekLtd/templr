namespace TemplR;

public abstract class Template
{
    private Dictionary<string, object?>? _variables = [];

    protected string? PropertyName { get; private set; }

    protected Template? Parent { get; private set; }

    public void SetContext(Template parent, string propertyName) 
    {
        if (Parent != null)
        {
            throw new InvalidOperationException($"Context already set to {Parent}.");
        }
        Parent = parent;
        PropertyName = propertyName;
    }

    public virtual void SetVariables(Dictionary<string, object?> variables)
    {
        _variables = variables;
    }

    public bool TryGetVariableValue(string variableName, out object? value)
    {
        if (_variables != null && _variables.TryGetValue(variableName, out value))
        {
            return true;
        }

        if (Parent != null)
        {
            return Parent.TryGetVariableValue(variableName, out value);
        }

        value = null;
        return false;
    }
}

public abstract class Template<T> : Template
{
    public static implicit operator Template<T>(T value)
    {
        return new Constant<T>(value);
    }

    public static implicit operator Template<T>(Variable variable) {
        return variable.Name == null
            ? new DefaultVariable<T>()
            : new NamedVariable<T>(variable.Name);
    }
}
