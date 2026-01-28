using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.Models;

namespace Noxy.NET.CaseManagement.Domain.Forms.Schemas.AssociationForms;

public class FormModelAssociationSchemaActionInputHasAttribute<T> : FormModelAssociationSchemaActionInputHasAttribute
{
    public required List<T> Value { get; set; }
    
    [JsonConstructor]
    public FormModelAssociationSchemaActionInputHasAttribute()
    {
    }

    [SetsRequiredMembers]
    public FormModelAssociationSchemaActionInputHasAttribute(EntitySchemaActionInput? entity, EntitySchemaAttribute? relation, IEnumerable<T> value) : base(entity, relation)
    {
        Value = value.ToList();
    }

    public override string APIEndpoint => "Association/Schema/ActionInput/Attribute/" + typeof(T) switch
    {
        { } t when t == typeof(int?) => "Integer",
        { } t when t == typeof(string) => "String",
        { } t when t == typeof(decimal?) => "Decimal",
        { } t when t == typeof(GenericUUID<EntitySchemaDynamicValue>) => "DynamicValue",
        _ => "Unknown"
    };
    
    public override HttpMethod HttpMethod => HttpMethod.Post;
    
}

public abstract class FormModelAssociationSchemaActionInputHasAttribute : BaseFormModelEntityManyToMany<EntitySchemaActionInput, EntitySchemaAttribute>
{
    protected FormModelAssociationSchemaActionInputHasAttribute(EntitySchemaActionInput? entity, EntitySchemaAttribute? relation) : base(entity, relation)
    {
    }
    
    [SetsRequiredMembers]
    protected FormModelAssociationSchemaActionInputHasAttribute(EntityAssociationSchemaActionInputHasAttribute? entity) : base(entity)
    {
    }

    [JsonConstructor]
    protected FormModelAssociationSchemaActionInputHasAttribute()
    {
    }
    
    public class Discriminator
    {
        public Guid ID { get; set; }
        public Guid EntityID { get; set; }
        public Guid RelationID { get; set; }

        public FormModelAssociationSchemaActionInputHasAttribute<string?>? String { get; init; }
        public FormModelAssociationSchemaActionInputHasAttribute<int?>? Integer { get; init; }
        public FormModelAssociationSchemaActionInputHasAttribute<decimal?>? Decimal { get; init; }
        public FormModelAssociationSchemaActionInputHasAttribute<GenericUUID<EntitySchemaDynamicValue>?>? DynamicValue { get; init; }

        [JsonConstructor]
        public Discriminator()
        {
        }

        public Discriminator(FormModelAssociationSchemaActionInputHasAttribute? entity)
        {
            switch (entity)
            {
                case FormModelAssociationSchemaActionInputHasAttribute<string?> value:
                    String = value;
                    break;
                case FormModelAssociationSchemaActionInputHasAttribute<int?> value:
                    Integer = value;
                    break;
                case FormModelAssociationSchemaActionInputHasAttribute<decimal?> value:
                    Decimal = value;
                    break;
                case FormModelAssociationSchemaActionInputHasAttribute<GenericUUID<EntitySchemaDynamicValue>?> value:
                    DynamicValue = value;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            ID = entity.ID;
            EntityID = entity.EntityID;
            RelationID = entity.RelationID;
        }

        public FormModelAssociationSchemaActionInputHasAttribute GetValue()
        {
            if (String != null) return String;
            if (Integer != null) return Integer;
            if (Decimal != null) return Decimal;
            if (DynamicValue != null) return DynamicValue;
            throw new();
        }
    }
}
