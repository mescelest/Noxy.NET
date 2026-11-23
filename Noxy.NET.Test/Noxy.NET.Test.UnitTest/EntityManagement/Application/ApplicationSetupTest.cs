// These tests run against SQLite in memory and finish at once, so there is nothing to cancel: the service calls are left on their default token.

#pragma warning disable xUnit1051

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noxy.NET.EntityManagement.Application;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Models;
using Noxy.NET.EntityManagement.Application.Services;
using Noxy.NET.EntityManagement.Definitions;
using Noxy.NET.EntityManagement.Definitions.Enums;
using Noxy.NET.EntityManagement.Demo.Definitions;
using Noxy.NET.EntityManagement.Demo.Definitions.Constants;
using Noxy.NET.EntityManagement.Demo.Definitions.Texts;
using Noxy.NET.Test.EntityManagement.Definitions;

namespace Noxy.NET.Test.EntityManagement.Application;

public class ApplicationSetupTest
{
    private static readonly Guid User = new("00000000-0000-0000-0000-00000000000a");

    // An in-memory database that lives as long as one connection is open, and can be reached by name from the connections the services open.
    private static SqliteConnection KeepAlive(out string connectionString)
    {
        connectionString = $"Data Source=file:{Guid.NewGuid():N}?mode=memory&cache=shared";

        SqliteConnection connection = new(connectionString);
        connection.Open();
        return connection;
    }

    private static ServiceProvider Build(string connectionString, Action<EntityManagementApplicationOptions> configure)
    {
        ServiceCollection services = new();
        services.AddEntityManagementPersistence(options => options.UseSqlite(connectionString));
        services.AddEntityManagementApplication(configure);

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task TheDemo_GivesItsTexts_ThroughTheSetupMethods_AndKeepsAnEditAcrossRequests()
    {
        using SqliteConnection keepAlive = KeepAlive(out string connectionString);
        await using ServiceProvider provider = Build(connectionString, options => options.AddDefinition<CaseworkDefinition>().AddTexts<CaseworkEnglishTexts>());
        await provider.MigrateEntityManagementAsync();

        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            ITextService texts = scope.ServiceProvider.GetRequiredService<ITextService>();
            Assert.IsType<TextService>(texts);

            Assert.Equal("Person", await texts.GetText(CaseworkTexts.PersonTitle, "en"));
            Assert.Equal("Person", await texts.GetText(CaseworkTexts.PersonTitle, "da-DK"));

            await texts.SetText(CaseworkTexts.PersonTitle, "en", "Human", User);
        }

        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            Assert.Equal("Human", await scope.ServiceProvider.GetRequiredService<ITextService>().GetText(CaseworkTexts.PersonTitle, "en"));
        }
    }

    [Fact]
    public async Task ThePersonOfTheDemo_IsCreatedAndRead_ThroughTheSetupMethods()
    {
        using SqliteConnection keepAlive = KeepAlive(out string connectionString);
        await using ServiceProvider provider = Build(connectionString, options => options.AddDefinition<CaseworkDefinition>().AddTexts<CaseworkEnglishTexts>());
        await provider.MigrateEntityManagementAsync();

        Guid id;
        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            IElementService elements = scope.ServiceProvider.GetRequiredService<IElementService>();
            Assert.IsType<ElementService>(elements);

            ElementModel person = await elements.CreateElement(CaseworkElements.Person, new Dictionary<string, object?> { [CaseworkProperties.Name] = "Ann", [CaseworkProperties.Age] = 41 }, User);
            id = person.ID;

            // The name is required, and the schema says it can be 200 characters.
            await Assert.ThrowsAsync<DataValidationException>(() => elements.CreateElement(CaseworkElements.Person, new Dictionary<string, object?> { [CaseworkProperties.Age] = 41 }, User));
            await Assert.ThrowsAsync<DataValidationException>(() => elements.CreateElement(CaseworkElements.Person, new Dictionary<string, object?> { [CaseworkProperties.Name] = new string('a', 201) }, User));
        }

        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            ElementModel? person = await scope.ServiceProvider.GetRequiredService<IElementService>().FindElement(id);

            Assert.NotNull(person);
            Assert.Equal(("Ann", 41L), (person.PropertyMap[CaseworkProperties.Name].Value, person.PropertyMap[CaseworkProperties.Age].Value));
            Assert.Equal(1, (await scope.ServiceProvider.GetRequiredService<IElementService>().GetElementPage(CaseworkElements.Person, 0, 10)).TotalCount);
        }
    }

    [Fact]
    public async Task APersonAndACase_AreRelated_ThroughTheSetupMethods_AndSeenFromBothEnds()
    {
        using SqliteConnection keepAlive = KeepAlive(out string connectionString);
        await using ServiceProvider provider = Build(connectionString, options => options.AddDefinition<CaseworkDefinition>().AddTexts<CaseworkEnglishTexts>());
        await provider.MigrateEntityManagementAsync();

        Guid personID;
        Guid caseID;
        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            IElementService elements = scope.ServiceProvider.GetRequiredService<IElementService>();
            IRelationService relations = scope.ServiceProvider.GetRequiredService<IRelationService>();
            Assert.IsType<RelationService>(relations);

            personID = (await elements.CreateElement(CaseworkElements.Person, new Dictionary<string, object?> { [CaseworkProperties.Name] = "Ann" }, User)).ID;
            caseID = (await elements.CreateElement(CaseworkElements.Case, new Dictionary<string, object?> { [CaseworkProperties.Name] = "Case 1" }, User)).ID;

            // The role is required.
            await Assert.ThrowsAsync<DataValidationException>(() => relations.CreateRelation(CaseworkRelations.PersonInvolvedInCase, personID, caseID, new Dictionary<string, object?>(), User));
            await relations.CreateRelation(CaseworkRelations.PersonInvolvedInCase, personID, caseID, new Dictionary<string, object?> { [CaseworkProperties.Role] = "Witness" }, User);
        }

        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            IRelationService relations = scope.ServiceProvider.GetRequiredService<IRelationService>();

            // The same link, from the person and from the case: "which cases is this person in" and "who is in this case".
            RelationModel ofPerson = Assert.Single(await relations.GetRelationList(personID));
            RelationModel ofCase = Assert.Single(await relations.GetRelationList(caseID));
            Assert.Equal(ofPerson.ID, ofCase.ID);
            Assert.Equal((personID, caseID, "Witness"), (ofPerson.FromElementID, ofPerson.ToElementID, ofPerson.AttributeMap[CaseworkProperties.Role].Value));

            // Ending the person ends the link too.
            await scope.ServiceProvider.GetRequiredService<IElementService>().EndElement(personID, User);
            Assert.Empty(await relations.GetRelationList(caseID));
            Assert.Single(await relations.GetRelationHistory(caseID));
        }
    }

    [Fact]
    public async Task TheRegistries_AreSingletons_AndTheServiceIsScoped()
    {
        using SqliteConnection keepAlive = KeepAlive(out string connectionString);
        await using ServiceProvider provider = Build(connectionString, options => options.AddDefinition<CaseworkDefinition>().AddTexts<CaseworkEnglishTexts>());

        await using AsyncServiceScope first = provider.CreateAsyncScope();
        await using AsyncServiceScope second = provider.CreateAsyncScope();

        Assert.Same(first.ServiceProvider.GetRequiredService<DefinitionRegistry>(), second.ServiceProvider.GetRequiredService<DefinitionRegistry>());
        Assert.Same(first.ServiceProvider.GetRequiredService<DefinitionTextRegistry>(), second.ServiceProvider.GetRequiredService<DefinitionTextRegistry>());
        Assert.NotSame(first.ServiceProvider.GetRequiredService<ITextService>(), second.ServiceProvider.GetRequiredService<ITextService>());
        Assert.Contains(CaseworkElements.Person, first.ServiceProvider.GetRequiredService<DefinitionRegistry>().ElementList.Select(x => x.Identifier));
    }

    [Fact]
    public void ADefinitionThatIsNotValid_StopsTheApplicationAtStartup_WithEveryProblem()
    {
        ServiceCollection services = new();
        ActionDefinition bad = new(builder =>
        {
            builder.AddTextParameter("Bad Text", TextKindEnum.Line);
            builder.AddElement("person", "missing", ["nothing"]);
        });

        DefinitionException exception = Assert.Throws<DefinitionException>(() => services.AddEntityManagementApplication(options => options.AddDefinition(bad)));

        Assert.Equal(3, exception.ErrorList.Count);
    }

    [Fact]
    public void ATextParameterWithoutAText_StopsTheApplicationAtStartup()
    {
        ServiceCollection services = new();

        DefinitionException exception = Assert.Throws<DefinitionException>(() => services.AddEntityManagementApplication(options => options.AddDefinition<CaseworkDefinition>()));

        Assert.All(exception.ErrorList, x => Assert.Contains("there is no text in the fallback culture 'en'", x));
        Assert.Equal(DefinitionBuilder.Create(new CaseworkDefinition()).TextParameterList.Count, exception.ErrorList.Count);
    }

    [Fact]
    public void ATextOfATextParameterThatDoesNotExist_StopsTheApplicationAtStartup()
    {
        ServiceCollection services = new();
        ActionTexts extra = new(builder => builder.AddText("nothing", "en", "x"));

        DefinitionException exception = Assert.Throws<DefinitionException>(() => services.AddEntityManagementApplication(options => options.AddDefinition<CaseworkDefinition>().AddTexts<CaseworkEnglishTexts>().AddTexts(extra)));

        Assert.Equal(["Text 'nothing' (en): the text parameter is not defined."], exception.ErrorList);
    }

    [Fact]
    public async Task TheFallbackCulture_CanBeSet_AndIsWhatAnUnknownCultureGets()
    {
        ActionDefinition definition = new(builder => builder.AddTextParameter("greeting", TextKindEnum.Line));
        ActionTexts danish = new(builder => builder.AddText("greeting", "da", "Goddag"));

        using SqliteConnection keepAlive = KeepAlive(out string connectionString);
        await using ServiceProvider provider = Build(connectionString, options =>
        {
            options.FallbackCulture = "da";
            options.AddDefinition(definition).AddTexts(danish);
        });
        await provider.MigrateEntityManagementAsync();

        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        Assert.Equal("Goddag", await scope.ServiceProvider.GetRequiredService<ITextService>().GetText("greeting", "sv"));
    }

    [Fact]
    public async Task SeveralDefinitionsAndFilesOfTexts_AreBuiltAsOne()
    {
        ActionDefinition project = new(builder =>
        {
            builder.AddTextParameter("acme.title", TextKindEnum.Line);
            builder.AddElement("acme.weapon", "acme.title", []);
        });
        ActionTexts texts = new(builder => builder.AddText("acme.title", "en", "Weapon"));

        using SqliteConnection keepAlive = KeepAlive(out string connectionString);
        await using ServiceProvider provider = Build(connectionString, options => options
            .AddDefinition<CaseworkDefinition>().AddDefinition(project)
            .AddTexts<CaseworkEnglishTexts>().AddTexts(texts));
        await provider.MigrateEntityManagementAsync();

        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        ITextService service = scope.ServiceProvider.GetRequiredService<ITextService>();
        Assert.Equal("Weapon", await service.GetText("acme.title", "en"));
        Assert.Equal("Person", await service.GetText(CaseworkTexts.PersonTitle, "en"));
        Assert.Equal(DefinitionBuilder.Create(new CaseworkDefinition()).TextParameterList.Count + 1, (await service.GetTextMap("en")).Count);
    }

    [Fact]
    public void NothingIsAddedWhenTheOptionsAreNull()
    {
        ServiceCollection services = new();

        Assert.Throws<ArgumentNullException>(() => services.AddEntityManagementApplication(null!));
    }
}
