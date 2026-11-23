namespace Noxy.NET.EntityManagement.Application.Enums;

// What is wrong with what was given. A page can show its own text for each, so the message of the error is only for developers.
public enum DataErrorEnum
{
    // The property is not defined, or is not one of the properties of this element, or of the attributes of this relation.
    UnknownProperty,

    // The value is not of the type of the property.
    WrongType,

    // The property is required and has no value.
    Required,

    // A string, or the reference of an image, is longer than the property allows.
    TooLong,

    // An element a relation is made between does not exist (or has ended).
    ElementNotFound,

    // An element is not of the kind the relation reads from or to.
    WrongElement,

    // The two elements already have a current relation of this kind.
    AlreadyRelated,

    // An element already has as many relations of this kind as the relation allows.
    RelationLimitReached
}
