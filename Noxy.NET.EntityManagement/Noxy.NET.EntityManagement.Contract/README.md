# Noxy.NET.EntityManagement.Contract

A small program that keeps the contract file of a definition library. The contract is the list of identifiers and kinds that stored data depends on (see the Definitions library), so a definition cannot be removed, renamed or retyped by accident, and
adding one is a single command.

## Asking

Run it without arguments and it asks for the command, the definition assembly and the contract file, and an empty answer takes the default shown in brackets. The defaults are what you used last time, and the first time the newest built `*.Definitions.dll`
below
the current folder, with `Contract.txt` in its project folder. It stays open at the end so the result can be read, so it can also be started by double-clicking `em-contract.exe`.

## With arguments

```
em-contract check  <definition assembly> <contract file>
em-contract add    <definition assembly> <contract file>
em-contract accept <definition assembly> <contract file>
```

- **check** compares and writes nothing. Exit code 1 when they differ.
- **add** locks what is new. It refuses (exit code 1, nothing written) when something locked was removed, renamed or retyped.
- **accept** makes the contract file the definitions as they are now. Use it only when the data that uses the old identifier can go: stored rows are not migrated.
- The assembly is the built `.dll` of the definition library. Every public `IDefinition` in it is used, or only the ones named with `--definition <type>` (repeat it for several).

Without installing it, from the repository root (or run `em-contract.exe` from its `bin` folder):

```
dotnet run --project Noxy.NET.EntityManagement/Noxy.NET.EntityManagement.Contract
```

As a tool: `dotnet pack`, then `dotnet tool install --global Noxy.NET.EntityManagement.Contract --add-source <the folder of the package>`.

Keep a test next to the definitions as well, so a build fails when the file is behind: `new DefinitionContractFile(path).Verify(DefinitionBuilder.Create(new MyDefinition()))`. The test only checks; this program is what writes.
