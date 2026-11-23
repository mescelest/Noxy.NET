using System.Text;

namespace Noxy.NET.EntityManagement.Definitions;

// The definitions do not match the contract file. The message says what differs, and how to settle it on purpose.
public sealed class DefinitionContractException(string path, DefinitionContractResult result) : Exception(CreateMessage(path, result))
{
    public DefinitionContractResult Result { get; } = result;

    private static string CreateMessage(string path, DefinitionContractResult result)
    {
        StringBuilder message = new($"The definitions do not match the contract in '{path}'.");

        if (result.BreakList.Count > 0)
        {
            message.AppendLine().AppendLine().AppendLine("Locked, and removed or changed (stored data uses these identifiers):");
            foreach (string line in result.BreakList) message.Append("  ").AppendLine(line);
        }

        if (result.AdditionList.Count > 0)
        {
            message.AppendLine().AppendLine().AppendLine("New, and not locked yet:");
            foreach (string line in result.AdditionList) message.Append("  ").AppendLine(line);
        }

        message.AppendLine();
        if (result.BreakList.Count == 0)
        {
            message.Append("To lock the new lines, run the contract program with 'add'.");
        }
        else
        {
            message.Append("If this was a mistake, undo it in the definitions. If it was meant, and the data that uses it can go, run the contract program with 'accept' to lock the definitions as they are now.");
        }

        return message.ToString();
    }
}
