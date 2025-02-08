namespace TemplR;

public static class From {
    public static Constant<T> Constant<T>(T value)
    {
        return new Constant<T>(value);
    }

    public static Variable Variable(string? variableName = null)
    {
        return new Variable(variableName);
    }
}
