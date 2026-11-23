namespace Noxy.NET.EntityManagement.Contract.Enums;

public enum ContractCommandEnum
{
    // Only compares. Writes nothing.
    Check,

    // Locks the lines that are new. Refuses when something locked was removed or changed.
    Add,

    // Makes the contract the definitions as they are, also when something locked was removed or changed.
    Accept
}
