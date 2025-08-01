namespace TemplR.System.Text.Json.Tests.Models;

public class TestClassWrapper
{
    public required string WrapperStringProp { get; init; }

    public required Template<TestClass> Child { get; init; }
}
