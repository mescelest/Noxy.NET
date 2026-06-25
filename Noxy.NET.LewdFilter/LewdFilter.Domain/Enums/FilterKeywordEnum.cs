namespace LewdFilter.Domain.Enums;

public enum FilterKeywordEnum
{
    SetTextColor,
    SetBorderColor,
    SetBackgroundColor,
    SetFontSize,
    PlayEffect,
    MinimapIcon,

    Rarity,

    AreaLevel,
    DropLevel,
    ItemLevel,
    WaystoneTier,
    GemLevel,
    BaseArmour,
    BaseEnergyShield,
    BaseEvasion,
    BaseWard,
    Height,
    Width,
    Quality,
    Sockets,
    StackSize,

    Corrupted,
    Mirrored,
    FracturedItem,
    Identified,
    IsVaalUnique,
    TwiceCorrupted,
    AlwaysShow,

    PlayAlertSound,
    PlayAlertSoundPositional,
    CustomAlertSound,
    CustomAlertSoundOptional,

    DisableDropSound,
    EnableDropSound,
    // DisableDropSoundIfAlertSound,
    // EnableDropSoundIfAlertSound,
}

public static class FilterKeywordEnumExtensions
{
    extension(FilterKeywordEnum keyword)
    {
        public string ToFilterString() => keyword.ToString();

        public string ToTextString() => keyword switch
        {
            FilterKeywordEnum.SetTextColor => "Text Color",
            FilterKeywordEnum.SetBorderColor => "Border Color",
            FilterKeywordEnum.SetBackgroundColor => "Background Color",
            FilterKeywordEnum.SetFontSize => "Font Size",
            FilterKeywordEnum.PlayEffect => "Beam Effect",
            FilterKeywordEnum.MinimapIcon => "Minimap Icon",

            FilterKeywordEnum.Rarity => "Rarity",

            FilterKeywordEnum.AreaLevel => "Area Level",
            FilterKeywordEnum.DropLevel => "Drop Level",
            FilterKeywordEnum.ItemLevel => "Item Level",
            FilterKeywordEnum.StackSize => "Stack Size",
            FilterKeywordEnum.Sockets => "Sockets",
            FilterKeywordEnum.Quality => "Quality",
            FilterKeywordEnum.GemLevel => "Gem Level",
            FilterKeywordEnum.WaystoneTier => "Waystone Tier",
            FilterKeywordEnum.BaseArmour => "Base Armour",
            FilterKeywordEnum.BaseEnergyShield => "Base Energy Shield",
            FilterKeywordEnum.BaseEvasion => "Base Evasion",
            FilterKeywordEnum.BaseWard => "Base Ward",
            FilterKeywordEnum.Width => "Width",
            FilterKeywordEnum.Height => "Height",

            FilterKeywordEnum.AlwaysShow => "Always Show",
            FilterKeywordEnum.Mirrored => "Mirrored",
            FilterKeywordEnum.Corrupted => "Corrupted",
            FilterKeywordEnum.FracturedItem => "Fractured Item",
            FilterKeywordEnum.Identified => "Identified",
            FilterKeywordEnum.IsVaalUnique => "Is Vaal Unique",
            FilterKeywordEnum.TwiceCorrupted => "Twice Corrupted",

            FilterKeywordEnum.PlayAlertSound => "Alert Sound",
            FilterKeywordEnum.PlayAlertSoundPositional => "Alert Sound (Positional)",
            FilterKeywordEnum.CustomAlertSound => "Custom Alert Sound",
            FilterKeywordEnum.CustomAlertSoundOptional => "Custom Alert Sound (Optional)",

            FilterKeywordEnum.DisableDropSound => "Disable Drop Sound",
            FilterKeywordEnum.EnableDropSound => "Enable Drop Sound",
            // FilterKeywordEnum.DisableDropSoundIfAlertSound => "Disable Drop Sound If Alert Played",
            // FilterKeywordEnum.EnableDropSoundIfAlertSound => "Enable Drop Sound If Alert Played",

            _ => string.Empty
        };
    }
}
