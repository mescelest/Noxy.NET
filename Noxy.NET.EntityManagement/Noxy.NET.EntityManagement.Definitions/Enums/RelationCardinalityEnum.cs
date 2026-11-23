namespace Noxy.NET.EntityManagement.Definitions.Enums;

// Read from the first element to the second: OneToMany means one "from" element can be linked to many "to" elements.
public enum RelationCardinalityEnum
{
    ManyToMany,
    OneToMany,
    ManyToOne,
    OneToOne
}
