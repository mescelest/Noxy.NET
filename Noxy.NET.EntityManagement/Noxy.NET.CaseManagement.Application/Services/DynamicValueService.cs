using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Domain.Entities.Data;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Application.Services;

public class DynamicValueService(IDynamicValueAPIService serviceDynamicValueApi) : IDynamicValueService
{
    private AssemblyLoadContext? AssemblyLoadContext { get; set; }
    private Type? CompiledClass { get; set; }
    private object? CompiledInstance { get; set; }

    private Dictionary<string, SortedList<DateTime, object?>> SystemParameterContext { get; set; } = [];
    private Dictionary<string, SortedList<DateTime, string>> TextParameterContext { get; set; } = [];

    private const string DynamicClassName = "DynamicClass";
    private const string DynamicAssemblyName = "DynamicAssembly";
    private const string DynamicContextName = "DynamicContext";
    private const string Namespace = "Noxy.NET.Test";

    public void Initialize(EntitySchema schema, List<EntityDataSystemParameter> listSystemParameter, List<EntityDataTextParameter> listTextParameter)
    {
        AssemblyLoadContext?.Unload();
        AssemblyLoadContext = new(DynamicContextName, isCollectible: true);

        List<EntitySchemaDynamicValueCode> context = ExtractDynamicValueCodeContext(schema);
        string code = GenerateCode(context);
        CompiledClass = Compile(code);
        CompiledInstance = Activator.CreateInstance(CompiledClass, [serviceDynamicValueApi]);

        SystemParameterContext = ExtractDynamicValueSystemParameterContext(listSystemParameter);
        TextParameterContext = ExtractDynamicValueTextParameterContext(listTextParameter);
    }

    public object? Resolve(EntitySchemaDynamicValue? value, object? data = null, object? context = null)
    {
        return value switch
        {
            null => null,
            EntitySchemaDynamicValueCode => Execute(value.SchemaIdentifier, data, context),
            EntitySchemaDynamicValueSystemParameter => SystemParameterContext[value.SchemaIdentifier].First(x => x.Key <= DateTime.UtcNow).Value,
            EntitySchemaDynamicValueTextParameter => TextParameterContext[value.SchemaIdentifier].First(x => x.Key <= DateTime.UtcNow).Value,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };
    }

    public object? Resolve(EntitySchemaDynamicValue.Discriminator? value, object? data = null, object? context = null)
    {
        return Resolve(value?.GetValue(), data, context);
    }

    public string? ResolveAsString(EntitySchemaDynamicValue? value, object? data = null, object? context = null)
    {
        object? resolved = Resolve(value, data, context);
        return resolved switch
        {
            null => null,
            string parsed => parsed,
            _ => resolved.ToString()
        };
    }

    public string? ResolveAsString(EntitySchemaDynamicValue.Discriminator? value, object? data = null, object? context = null)
    {
        return ResolveAsString(value?.GetValue(), data, context);
    }

    public object? Execute(string identifier, object? data, object? context = null)
    {
        MethodInfo? method = CompiledClass?.GetMethod(identifier);
        try
        {
            return method?.Invoke(CompiledInstance, [data, context]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    private Type Compile(string code)
    {
#if DEBUG
        Console.Write(code);
#endif

        CSharpCompilation compilation = GetCompilation(code);

        using MemoryStream ms = new();
        EmitResult result = compilation.Emit(ms);
        if (!result.Success)
        {
            foreach (Diagnostic failure in result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
            {
                Console.WriteLine(failure.GetMessage());
            }

            throw new("Compilation failed");
        }

        ms.Seek(0, SeekOrigin.Begin);
        Assembly assembly = AssemblyLoadContext?.LoadFromStream(ms) ?? throw new NullReferenceException(nameof(AssemblyLoadContext));

        foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
        {
            Console.WriteLine(reference.FullName);
        }

        return assembly.GetType(DynamicClassName) ?? throw new NullReferenceException(nameof(DynamicClassName));
    }

    private static List<EntitySchemaDynamicValueCode> ExtractDynamicValueCodeContext(EntitySchema schema)
    {
        List<EntitySchemaDynamicValueCode> result = [];
        foreach (EntitySchemaDynamicValue.Discriminator item in schema.DynamicValueList ?? [])
        {
            if (item.GetValue() is EntitySchemaDynamicValueCode code)
            {
                result.Add(code);
            }
        }

        return result;
    }

    private static string GenerateCode(List<EntitySchemaDynamicValueCode> context)
    {
        StringBuilder sb = new();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Dynamic;");
        sb.AppendLine("using System.Globalization;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using System.Runtime;");
        sb.AppendLine("using Noxy.NET.Test.Application.Interfaces.Services;");

        sb.AppendLine($"public class {DynamicClassName}(IDynamicValueAPIService API)");
        sb.AppendLine("{");

        foreach (EntitySchemaDynamicValueCode item in context)
        {
            if (item.IsAsynchronous)
            {
                sb.AppendLine($"public async Task<object?> {item.SchemaIdentifier}(dynamic data, dynamic context) {{ {item.Value} }}");
            }
            else
            {
                sb.AppendLine($"public object? {item.SchemaIdentifier}(dynamic data, dynamic context) {{ {item.Value} }}");
            }
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static CSharpCompilation GetCompilation(string code)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
        return CSharpCompilation.Create(DynamicAssemblyName)
            .WithOptions(new(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(
                MetadataReference.CreateFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Namespace}.Application.dll")),
                MetadataReference.CreateFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Namespace}.Domain.dll")),
                MetadataReference.CreateFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Namespace}.Persistence.dll")),
                MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "System.Private.CoreLib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "Microsoft.CSharp.dll")),
                MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "System.Collections.dll")),
                MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "System.Console.dll")),
                MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "System.Linq.dll")),
                MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "System.Linq.Expressions.dll")), // This is for Dynamics
                MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.dll"))
            )
            .AddSyntaxTrees(syntaxTree);
    }

    private static Dictionary<string, SortedList<DateTime, object?>> ExtractDynamicValueSystemParameterContext(List<EntityDataSystemParameter> list)
    {
        Dictionary<string, SortedList<DateTime, object?>> result = [];

        foreach (EntityDataSystemParameter item in list)
        {
            if (!result.TryGetValue(item.SchemaIdentifier, out SortedList<DateTime, object?>? value))
            {
                result[item.SchemaIdentifier] = value = [];
            }

            value.Add(item.TimeEffective, item.Value);
        }

        return result;
    }

    private static Dictionary<string, SortedList<DateTime, string>> ExtractDynamicValueTextParameterContext(List<EntityDataTextParameter> list)
    {
        Dictionary<string, SortedList<DateTime, string>> result = [];

        foreach (EntityDataTextParameter item in list)
        {
            if (!result.TryGetValue(item.SchemaIdentifier, out SortedList<DateTime, string>? value))
            {
                result[item.SchemaIdentifier] = value = [];
            }

            value.Add(item.TimeEffective, item.Value);
        }

        return result;
    }
}
