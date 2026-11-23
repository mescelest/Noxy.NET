using Noxy.NET.EntityManagement.Definitions.Models;

namespace Noxy.NET.EntityManagement.Definitions.Extensions;

public static class DefinitionRegistryExtensions
{
    extension(DefinitionRegistry registry)
    {
        // The contract of the definitions: one line for each fact that data in the database depends on, sorted so the same definitions always give the same lines.
        // These are the identifiers of every kind of definition, the kind of each property and text, which properties an element and a relation have, and what a relation joins and how many it allows. Titles, order, required and maximum length are not in it.
        // Keep the lines in a file next to the definitions and check them with GetContractBreakList and GetContractAdditionList.
        public List<string> GetContractLineList()
        {
            List<string> lineList = [];

            lineList.AddRange(registry.TextParameterList.Select(x => $"text {x.Identifier} {x.Kind}"));
            lineList.AddRange(registry.PropertyList.Select(x => $"property {x.Identifier} {x.Kind}"));

            foreach (DefinitionElement element in registry.ElementList)
            {
                lineList.Add($"element {element.Identifier}");
                lineList.AddRange(element.PropertyList.Select(x => $"element {element.Identifier} property {x}"));
            }

            foreach (DefinitionRelation relation in registry.RelationList)
            {
                lineList.Add($"relation {relation.Identifier} {relation.FromElement} {relation.ToElement} {relation.Cardinality}");
                lineList.AddRange(relation.AttributeList.Select(x => $"relation {relation.Identifier} attribute {x}"));
            }

            lineList.AddRange(registry.ContextList.Select(x => $"context {x.Identifier}"));

            return lineList.Order(StringComparer.Ordinal).ToList();
        }

        // What was locked and is no longer there: an identifier that was removed, a property that changed kind, a relation that joins other elements or allows another number. Each of them would leave stored data without a meaning.
        public List<string> GetContractBreakList(IEnumerable<string> lockedLineList)
        {
            ArgumentNullException.ThrowIfNull(lockedLineList);

            HashSet<string> currentSet = registry.GetContractLineList().ToHashSet(StringComparer.Ordinal);
            return lockedLineList.Where(x => !currentSet.Contains(x)).ToList();
        }

        // What is in the definitions and not yet locked. Definitions may grow, but the new lines have to be added to the locked ones, so they are locked from then on.
        public List<string> GetContractAdditionList(IEnumerable<string> lockedLineList)
        {
            ArgumentNullException.ThrowIfNull(lockedLineList);

            HashSet<string> lockedSet = lockedLineList.ToHashSet(StringComparer.Ordinal);
            return registry.GetContractLineList().Where(x => !lockedSet.Contains(x)).ToList();
        }
    }
}
