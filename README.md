# TemplR

Serializable, Type-safe Payload Enrichment for .NET

TemplR is a .NET library designed to facilitate **API payload** and **event payload** templating. It allows developers to define strongly typed, serializable templates where certain properties can be replaced with variables. This enables dynamic payload generation while maintaining type safety.

### Installing TemplR

TemplR can be installed from NuGet:

```
Install-Package TemplR
```

or using the .NET CLI:

```
dotnet add package TemplR
```

### Code Generation vs. Runtime Reflection

TemplR supports **two different approaches** for creating templates:

1. **TemplR.Generator (Code Generation, Recommended)**

   - Uses **source generators** to create strongly typed proxies at compile-time.
   - Requires `[Template]` attributes on partial classes.
   - Provides better performance since no reflection is needed at runtime.

2. **From.Expression (Runtime Reflection, Alternative Approach)**

   - Allows defining templates dynamically using expressions.
   - Uses **reflection at runtime** to infer variable replacements.
   - Useful if you want to avoid additional compilation steps.

If you don't mind using **code generation**, the `[Template]` approach is generally **easier to use** than `From.Expression`.

### Using Code Generation

TemplR can **auto-generate proxies** from your existing data objects, allowing properties to be replaced by strongly typed variables. To create a proxy from an existing record or class, you first need to install TemplR's code generator library:

```
Install-Package TemplR.Generator
```

Next, define a new proxy by creating a **partial class** decorated with the `[Template]` attribute:

```csharp
    [Template(typeof(Person))]
    public partial class PersonTemplate {}
```

The `TemplateAttribute` takes a single argument equal to the type being proxied. After defining this, you can create a new `PersonTemplate` instance with properties that support variable substitution:

```csharp
    var person = new PersonTemplate
    {
        Name = "Santa Claus",
        Age = From.Variable("yearsSinceFirstChristmas"),
        IsImaginary = From.Variable()
    };
```

If no name is passed to `From.Variable`, the variable will take the same name as the property it is assigned to.

This proxy can be passed to any method that accepts either `Template` or `Template<Person>` as a parameter.

If reusable, type-safe variables are needed, it is often useful to define them in a static class:

```csharp
    public static class PersonVariables
    {
        public static Variable<int> YearsSinceFirstChristmas => From.Variable<int>("yearsSinceFirstChristmas");
        public static Variable<bool> IsImaginary => From.Variable<bool>("isImaginary");
    }
```

```csharp
    var person = new PersonTemplate
    {
        Name = "Santa Claus",
        Age = PersonVariables.YearsSinceFirstChristmas,
        IsImaginary = PersonVariables.IsImaginary
    };
```

### Using Runtime Reflection

If you want to avoid using **code generation**, you can use **runtime reflection** instead by utilizing the `From.Expression` method.

To use this approach, define a record or class that describes the variables you wish to use:

```csharp
    public record PersonVariables(int Age, bool Imaginary);
```

By default, variable names will be camel case versions of the property names. You can customize this behavior using attributes:

- **[TemplateVariable]** to rename a single variable:

```csharp
    public record PersonVariables(bool Imaginary)
    {
        [TemplateVariable("Age")]
        public required int Age { get; init; }
    }
```

- **[TemplateVariableSet]** to disable this behavior for all properties:

```csharp
    [TemplateVariableSet(UseCamelCase = false)]
    public record PersonVariables(int Age, bool Imaginary);
```

Now, you can call `From.Expression` to produce a new template:

```csharp
    var template = From.Expression<Person, PersonVariables>((vars) => new()
    {
        Name = "Santa Claus",
        Age = vars.Age,
        IsImaginary = vars.Imaginary
    });
```

Any call to `vars` will be replaced with a placeholder when serialized.

### JSON Serialization and Deserialization

TemplR supports both **serialization and deserialization** using `System.Text.Json`. To enable this functionality, install the required library:

```
Install-Package TemplR.System.Text.Json
```

To add the necessary converters, create an instance of `JsonSerializerOptions` and call the `UseTemplR()` extension method:

```csharp
    JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    serializerOptions.UseTemplR();
```

From that point on, these options should be used for all calls to `JsonSerializer`.

### Serialization with Variable Values

To **serialize a template with replacement values**, first deserialize it into the `Template` type, call `SetVariables`, and then re-serialize it:

```csharp
    var template = JsonSerializer.Deserialize<Template>(json, serializerOptions);
    template.SetVariables(new()
    {
        { "yearsSinceFirstChristmas", 42 },
        { "isImaginary", false }
    });
    var jsonWithVariables = JsonSerializer.Serialize(template, serializerOptions);
```

This replaces any placeholders in the template with the actual values during serialization.