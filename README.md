TemplR
========

Serializable, Type-safe Payload Enrichment for .NET

### Installing TemplR.

TemplR can be installed from nuget

    Install-Package TemplR

or using the dotnet clr

    dotnet add package TemplR

### Creating a template proxy.

TemplR can auto-generate proxies from your own data objects that allow properties to be replaced by strongly typed variables. To create a proxy from an existing record or class you first need to install TemplR's code generator library:

    Install-Package TemplR.Generator

Next define a new proxy by defining a new partial class decorated with the TemplateAttribute

```csharp
    [Template(typeof(Person))]
    public partial class PersonTemplate {}
```

The TemplateAttribute takes a single argument equal to the type that is being proxied. You should now be able to create a new PersonTemplate instance, which will have proxiable versions of all the original class' properties...

```csharp
    var person = new PersonTemplate
    {
        Name = "Santa Claus",
        Age = From.Variable("yearsSinceFirstChristmas"),
        IsImaginary = From.Variable()
    }
```

If no name is passed to From.Variable, then the variable will take the same name as the property it is assigned to.

This proxy can be passed to any method which accepts either Template or Template<Person> as a parameter.

### Json Serialization and Deserialization.

TemplR supports both serialization and deserialization using System.Text.Json. To enable this functionality you need to install the following library...

    Install-Package TemplR.System.Text.Json

To add the required converters, create a new instance of JsonSerializerOptions and call the UseTemplR() extension method on it.

```csharp
    JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    serializerOptions.UseTemplR();
```

from that point on these options should be used for all calls to the JsonSerializer.

### Serialization with variable values.

To serialize a template, with replacement values for any variables, first deserialize to the Template type, call SetVariables and then re-serialize it...

```csharp
    var template = JsonSerializer.Deserialize<Template>(json, serializerOptions);
    template.SetVariables(new()
    {
        { "yearsSinceFirstChristmas", 42 },
        { "isImaginary", false }
    });
    var jsonWithVariables = JsonSerializer.Serialize(template, serializerOptions);
```