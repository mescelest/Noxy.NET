# Noxy.NET.EntityManagement.Application

The services of an entity management system, on top of the definitions and the repositories.

```csharp
builder.Services.AddEntityManagementPersistence(options => options.UseSqlite(connectionString));
builder.Services.AddEntityManagementApplication(options =>
{
    options.AddDefinition<CaseworkDefinition>();
    options.AddTexts<CaseworkEnglishTexts>();
});
```

The definitions and the texts are run and checked when the services are set up, so a mistake in them stops the application at startup, with every problem listed.

## Texts

`ITextService` gives the texts to whoever shows them, in the language of the person who looks.

- **An edit comes before the default.** A text is what a person has edited (kept in the database), or else what the code says. The database only holds edits, so a text shows its default until somebody changes it, and a newer default in the code reaches
  everybody who has not edited it.
- **The culture, its language, the fallback.** For `da-DK` it looks in `da-DK`, then `da`, then the fallback culture (`en` unless set otherwise), and in each of them an edit comes before the default. A culture that is missing or is not a language tag is not an
  error: the fallback is used.
- **`GetText`** gives one text and **`GetTextMap`** every text at once, for an application to load. A text that is not defined is a mistake of the code, so it throws.
- **`SetText`** edits a text in any culture (also one that has no default, which is how a language is added without a deployment). The text must exist, the culture must be a language tag, the value cannot be empty, and a line of text cannot hold a line break.
  Setting the value it already has changes nothing. The previous edit moves to the history.
- **`ResetText`** removes the edit, so the text shows its default again.

## Elements

`IElementService` creates, changes and ends the elements, and reads them, with every value checked against the definition. The values are given by the identifier of the property, and come back in the type of the property: `bool`, `DateTime` (always UTC),
`decimal`, `long`, or `string` (for a string and for the reference of an image).

```csharp
ElementModel person = await elements.CreateElement("person", new Dictionary<string, object?> { ["name"] = "Ann", ["age"] = 41 }, userID);
await elements.ChangeElement(person.ID, new Dictionary<string, object?> { ["age"] = 42, ["note"] = null }, userID);
```

- **Checked against the definition.** The property must be one of the element's, a required property must have a value, a value must be of the property's type, and a string cannot be longer than the property allows. Whole numbers of any type are accepted for
  an integer and a decimal; a `float` or `double` is not accepted for a decimal, because it would not keep its digits. A time is kept in UTC: a local time is converted, and a time of no kind is taken to be UTC.
- **Every problem at once.** A bad value does not throw a different exception for each problem: the call throws one `DataValidationException` whose `ErrorList` has every problem, each with the property and a code (`UnknownProperty`, `WrongType`, `Required`,
  `TooLong`), so a form can show them all, in its own words. Nothing is stored. An element that is not defined is a mistake of the code and throws `KeyNotFoundException`.
- **No value.** A value that is `null`, or a string with nothing in it (what an empty input of a form gives), means no value: on a new element the property is left out, and on a change it is cleared. A required property cannot be cleared.
- **All or nothing.** The element and its first values are stored together, and so are the several changes of one call. A change only goes through while the element exists, and that is checked in the same transaction.
- **A change is only a change.** Setting the value a property already has (`1.0` for `1.00`, the same moment in another time zone) leaves nothing in the history. What a change replaces moves to the history, with when and by whom.
- **`GetPropertyHistory`** gives the values that have ended, in the order they ended, also for an element that has ended. **`GetElementPage`** gives a page of the elements of a kind, each with its values, read in a few queries however many there are.

## Relations

`IRelationService` links elements to each other and reads the links. A link reads from one element to another, and from the element it reads to it is the same link: that is the inverse view, so "which cases is this person in" and "who is in this case" are the
same data, read from either end.

```csharp
RelationModel link = await relations.CreateRelation("person-involved-in-case", personID, caseID, new Dictionary<string, object?> { ["role"] = "Witness" }, userID);
IReadOnlyList<RelationModel> ofPerson = await relations.GetRelationList(personID);   // the links of the person, whichever way they read
IReadOnlyList<RelationModel> ofCase = await relations.GetRelationList(caseID);       // the same link, from the case
```

- **Checked against the definition**, and every problem reported at once, in a `DataValidationException`, as for elements: both elements must exist (`ElementNotFound`) and be of the kinds the relation joins (`WrongElement`); the attributes are checked like the
  values of an element, and a required one must have a value; the two must not already have the relation (`AlreadyRelated`); and the cardinality must hold (`RelationLimitReached`).
- **Cardinality.** ManyToMany has no limit. ManyToOne: an element it reads from can have only one. OneToMany: an element it reads to can have only one. OneToOne: both. The limits are checked in the same transaction as the insert.
- **A link of its own kind only once.** A person is linked to a case once. A second role in the same case is another relation in the schema, or another attribute.
- **`ChangeRelation`** changes the attributes that are given, as `ChangeElement` does for values. **`EndRelation`** ends the link and its attributes, which move to the history; the elements stay. Ending an element ends all of its links.
- **History:** `GetRelationHistory(elementID)` gives the links that have ended, in either direction, and `GetAttributeHistory(relationID)` the attributes that have ended.
