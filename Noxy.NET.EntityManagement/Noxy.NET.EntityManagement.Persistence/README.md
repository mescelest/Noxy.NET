# Noxy.NET.EntityManagement.Persistence

Stores the data of an entity management system with Entity Framework Core, and keeps its history.

```csharp
builder.Services.AddEntityManagementPersistence(options => options.UseSqlite(connectionString));

// at startup, so the tables exist:
await app.Services.MigrateEntityManagementAsync();
```

`IDataRepository` (elements and their properties) and `ITextRepository` (the values of the text parameters, one for each text and culture) are then available. The culture is compared without regard to case, in the database too. The migrations that create the
tables are part of this library and are written for SQLite.

- **Current and history are separate tables.** The current tables (`DataElement`, and one table for each property type) only hold what is current. A row that ends moves to its twin (`HistoryElement`, `HistoryPropertyString`, ...), in the same transaction as
  the change that ended it, so it either all happens or none of it does.
- **One current value of a property per owner.** A unique index on the owner and the property identifier keeps it so.
- **No foreign keys between the data tables.** A row in the history cannot point at a current row that may be gone, and a row that moves to the history must not be held back by the rows that point at it. The repository keeps them consistent, and the tests
  prove it.
- **The foundation tables change rarely, and only by adding.** While nothing is deployed the `Initial` migration is regenerated; once something is deployed, a migration only adds tables, columns and indexes.

To change the tables, change the classes in the Domain library and the mapping in `Configurations`, then (from the repository root):

```
dotnet ef migrations add <Name> --project Noxy.NET.EntityManagement/Noxy.NET.EntityManagement.Persistence
```

## Several changes in one step

`IDataRepository` can also create an element together with its first values (`CreateElement(element, propertyList)`), and set and clear several values of an element in one call (`ChangeElementProperties`). Both are all or nothing: the second one also checks,
in the same transaction, that the element still exists, so a value is never added to an element that has ended.

## Relations

`IRelationRepository` stores the links between elements. The attributes of a link are ordinary property rows owned by the relation, read with `IDataRepository.GetPropertyList`.

- **One current link of a kind between two elements.** A unique index on the relation, the element it reads from and the one it reads to keeps it so. Reading from either end has an index each.
- **The checks and the insert are one transaction.** Creating a relation checks that both elements exist, that the link is not already there, and the limits it is given (how many links of the kind each end may have, which is how the cardinality of the schema
  is kept), and then stores it. Two requests at once cannot both pass a limit, and a link is never made to an element that has ended.
- **Ending an element ends its relations**, in both directions, with their attributes, in the same step as the element. A link is never left pointing at an element that is gone. Ending a link leaves the elements.
