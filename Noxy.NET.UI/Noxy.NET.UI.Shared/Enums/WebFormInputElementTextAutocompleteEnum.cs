using System.ComponentModel;

namespace Noxy.NET.UI.Enums;

public enum WebFormInputElementTextAutocompleteEnum
{
    On,
    Off,
    OneTimeCode,

    // Personal Names
    Name,
    HonorificPrefix,
    GivenName,
    AdditionalName,
    FamilyName,
    HonorificSuffix,
    Nickname,
    Username,

    // Contact Info
    Email,
    Url,

    // Phone
    Tel,
    TelCountryCode,
    TelNational,
    TelAreaCode,
    TelLocal,
    TelLocalPrefix,
    TelLocalSuffix,
    TelExtension,
    Impp,

    // Address & Location
    Organization,
    StreetAddress,
    AddressLine1,
    AddressLine2,
    AddressLine3,
    AddressLevel4,
    AddressLevel3,
    AddressLevel2,
    AddressLevel1,
    Country,
    CountryName,
    PostalCode,

    // Commerce / Credit Card
    CcName,
    CcGivenName,
    CcAdditionalName,
    CcFamilyName,
    CcNumber,
    CcExp,
    CcExpMonth,
    CcExpYear,
    CcCsc,
    CcType,
    TransactionCurrency,
    TransactionAmount,

    // Demographics
    Bday,
    BdayDay,
    BdayMonth,
    BdayYear,
    Sex,
    Language
}

public static class WebFormInputElementTextAutocompleteEnumExtensions
{
    public static string ToAttributeValue(this WebFormInputElementTextAutocompleteEnum value)
    {
        return value switch
        {
            WebFormInputElementTextAutocompleteEnum.On => "on",
            WebFormInputElementTextAutocompleteEnum.Off => "off",
            WebFormInputElementTextAutocompleteEnum.OneTimeCode => "one-time-code",

            // Personal Names
            WebFormInputElementTextAutocompleteEnum.Name => "name",
            WebFormInputElementTextAutocompleteEnum.HonorificPrefix => "honorific-prefix",
            WebFormInputElementTextAutocompleteEnum.GivenName => "given-name",
            WebFormInputElementTextAutocompleteEnum.AdditionalName => "additional-name",
            WebFormInputElementTextAutocompleteEnum.FamilyName => "family-name",
            WebFormInputElementTextAutocompleteEnum.HonorificSuffix => "honorific-suffix",
            WebFormInputElementTextAutocompleteEnum.Nickname => "nickname",
            WebFormInputElementTextAutocompleteEnum.Username => "username",

            // Contact Info
            WebFormInputElementTextAutocompleteEnum.Email => "email",
            WebFormInputElementTextAutocompleteEnum.Url => "url",

            // Phone
            WebFormInputElementTextAutocompleteEnum.Tel => "tel",
            WebFormInputElementTextAutocompleteEnum.TelCountryCode => "tel-country-code",
            WebFormInputElementTextAutocompleteEnum.TelNational => "tel-national",
            WebFormInputElementTextAutocompleteEnum.TelAreaCode => "tel-area-code",
            WebFormInputElementTextAutocompleteEnum.TelLocal => "tel-local",
            WebFormInputElementTextAutocompleteEnum.TelLocalPrefix => "tel-local-prefix",
            WebFormInputElementTextAutocompleteEnum.TelLocalSuffix => "tel-local-suffix",
            WebFormInputElementTextAutocompleteEnum.TelExtension => "tel-extension",
            WebFormInputElementTextAutocompleteEnum.Impp => "impp",

            // Address & Location
            WebFormInputElementTextAutocompleteEnum.Organization => "organization",
            WebFormInputElementTextAutocompleteEnum.StreetAddress => "street-address",
            WebFormInputElementTextAutocompleteEnum.AddressLine1 => "address-line1",
            WebFormInputElementTextAutocompleteEnum.AddressLine2 => "address-line2",
            WebFormInputElementTextAutocompleteEnum.AddressLine3 => "address-line3",
            WebFormInputElementTextAutocompleteEnum.AddressLevel4 => "address-level4",
            WebFormInputElementTextAutocompleteEnum.AddressLevel3 => "address-level3",
            WebFormInputElementTextAutocompleteEnum.AddressLevel2 => "address-level2",
            WebFormInputElementTextAutocompleteEnum.AddressLevel1 => "address-level1",
            WebFormInputElementTextAutocompleteEnum.Country => "country",
            WebFormInputElementTextAutocompleteEnum.CountryName => "country-name",
            WebFormInputElementTextAutocompleteEnum.PostalCode => "postal-code",

            // Commerce / Credit Card
            WebFormInputElementTextAutocompleteEnum.CcName => "cc-name",
            WebFormInputElementTextAutocompleteEnum.CcGivenName => "cc-given-name",
            WebFormInputElementTextAutocompleteEnum.CcAdditionalName => "cc-additional-name",
            WebFormInputElementTextAutocompleteEnum.CcFamilyName => "cc-family-name",
            WebFormInputElementTextAutocompleteEnum.CcNumber => "cc-number",
            WebFormInputElementTextAutocompleteEnum.CcExp => "cc-exp",
            WebFormInputElementTextAutocompleteEnum.CcExpMonth => "cc-exp-month",
            WebFormInputElementTextAutocompleteEnum.CcExpYear => "cc-exp-year",
            WebFormInputElementTextAutocompleteEnum.CcCsc => "cc-csc",
            WebFormInputElementTextAutocompleteEnum.CcType => "cc-type",
            WebFormInputElementTextAutocompleteEnum.TransactionCurrency => "transaction-currency",
            WebFormInputElementTextAutocompleteEnum.TransactionAmount => "transaction-amount",

            // Demographics
            WebFormInputElementTextAutocompleteEnum.Bday => "bday",
            WebFormInputElementTextAutocompleteEnum.BdayDay => "bday-day",
            WebFormInputElementTextAutocompleteEnum.BdayMonth => "bday-month",
            WebFormInputElementTextAutocompleteEnum.BdayYear => "bday-year",
            WebFormInputElementTextAutocompleteEnum.Sex => "sex",
            WebFormInputElementTextAutocompleteEnum.Language => "language",

            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(WebFormInputElementTextAutocompleteEnum))
        };
    }
}
