# Noxy.NET.EntityManagement.Definitions

The schema of an entity management system, written in code. A project describes what its data looks like with a file of `Add...` calls, and this library checks the calls and holds the result.

- **Text parameter:** a text a user will see. It only says that the text exists and what kind it is (line, text or rich text). What it says is registered apart, in each language, so everything shown to a user can be translated.
- **Property:** a typed field (`Boolean`, `DateTime`, `Decimal`, `Image`, `Integer`, `String`), optionally required, and with a maximum length for a string. Properties are defined once and can belong to several elements.
- **Element:** a kind of thing (a person, a location), made of properties.
- **Relation:** a link between two kinds of element, with a title for each direction, and optionally properties of its own (a role, a note).
- **Context:** a group of elements that belong together, and so the data to query for a page.

Identifiers are plain strings: lower case letters and digits, separated by single `-` or `.` (`person`, `person-involved-in-case`, `acme.weapon`). They are what is stored with the data, so they never change once data exists.

```csharp
public sealed class PersonDefinition : IDefinition
{
    public void Define(IDefinitionBuilder builder)
    {
        builder.AddTextParameter("person-title", TextKindEnum.Line);
        builder.AddTextParameter("name-title", TextKindEnum.Line);

        builder.AddProperty("name", PropertyKindEnum.String, "name-title", isRequired: true, maxLength: 200);
        builder.AddElement("person", "person-title", ["name"]);
    }
}

DefinitionRegistry registry = DefinitionBuilder.Create(new PersonDefinition());
```

The calls only collect. `Build` (or `Create`) checks everything at once and either returns a read-only `DefinitionRegistry`, or throws a `DefinitionException` that lists every problem it found: a bad or duplicate identifier, a text, property, element or
relation that is used but not defined, a maximum length on a property that is not a string, a context without elements. The calls can be made in any order.

## The texts

What the texts say is written in files of their own, one call for each text and culture, so a system can ship any number of languages. A language is a file; adding one is adding a file.

```csharp
public sealed class EnglishTexts : IDefinitionTexts
{
    public void Define(IDefinitionTextBuilder builder)
    {
        builder.AddText("person-title", "en", "Person");
        builder.AddText("name-title", "en", "Name");
    }
}

DefinitionTextRegistry texts = DefinitionTextBuilder.Create(registry, new EnglishTexts(), new DanishTexts());
```

These are defaults. A person can edit a text in the running system, and the edit is kept in the database and is never replaced by a newer default. The texts are checked against the definition when they are built, and every problem is reported together: a text
of a text parameter that is not defined, a culture that is not a language tag (`en`, `da-DK`, `zh-Hans-CN`), an empty text, a text twice in one culture, and a text parameter that has no text in the fallback culture (`en` unless another is given), so there is
always something to show. Cultures are compared without regard to case.

## The contract

Data in a database is stored under the identifiers of the definitions, so a definition library may grow but not change by accident. `GetContractLineList()` (in `Definitions.Extensions`) gives one sorted line for each fact the data depends on: every identifier,
the kind of each property and text, the properties of each element, and what each relation joins and how many it allows. Titles, order, required and maximum length are not in it.

The lines are kept in a text file next to the definitions (the demo's is `Contract.txt`), and a test checks them (see `CaseworkContractTest`):

```csharp
[Fact]
public void TheDefinitions_MatchTheContract() => new DefinitionContractFile(pathOfTheContractFile).Verify(DefinitionBuilder.Create(new MyDefinition()));
```

- **By accident nothing changes.** The test fails on any difference, and says what it is. It never writes.
- **Adding is one command.** After adding a property, element, relation, context or text, run the contract program (`Noxy.NET.EntityManagement.Contract`) and answer `add`. The new lines are written to the file and are locked from then on. A removal or a change
  is refused.
- **Removing or changing is a different, named command.** `accept` makes the file the definitions as they are now. Use it only when the data that uses the old identifier can go: stored rows are not migrated.
- **Without the program:** `DefinitionContractFile.Check` shows the difference, and `Update(registry, acceptBreaks)` does what `add` and `accept` do.
