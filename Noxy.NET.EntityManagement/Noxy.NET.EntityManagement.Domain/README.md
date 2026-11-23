# Noxy.NET.EntityManagement.Domain

The data of an entity management system, as plain classes with no database code, and the contract of the repository that stores them.

- **`DataElement`:** one thing (a person, a case). It only says which element of the schema it is (`SchemaIdentifier`); everything about it is in its properties.
- **`DataProperty`:** one value of one element, held in the type the schema gave it: `DataPropertyBoolean`, `DataPropertyDateTime`, `DataPropertyDecimal`, `DataPropertyImage`, `DataPropertyInteger` or `DataPropertyString`. `OwnerID` says whose value it is and
  `PropertyIdentifier` which property of the schema.
- **`DataRelation`:** a link between two elements, of one relation of the schema (`RelationIdentifier`, `FromElementID`, `ToElementID`). It reads from the first to the second; the same row is the inverse view from the second. What the link itself has (a role,
  a note) is stored as ordinary properties with the ID of the relation as their owner. Like the owner of a property, the elements are not foreign keys.
- **`DataTextParameter`:** what a text parameter says in one culture (`TextIdentifier`, `Culture` and `Value`). The text parameter itself (its identifier and its kind) and its defaults are written in code; only the values people have edited are data, so a
  language can be added and a text edited without a deployment. Only what a person has edited is stored, so a text shows its default until somebody changes it.
- **History:** every row has a twin in the history (`HistoryElement`, `HistoryPropertyString`, `HistoryRelation`, `HistoryTextParameter`, and so on) with the same columns plus when it ended and who ended it.

A row is never changed. Changing a value creates a new row that becomes the current one, and the row it replaces moves to the history. The current tables therefore only ever hold what is current, so reading the current data never has to wade through the
history.

Every row records who made it (`CreatedByUserID`). `BaseData.SystemUserID` is used for what no user did.
