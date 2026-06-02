using LewdFilter.Generator;
using Scriban;

string templateText = File.ReadAllText("Template.filter");
Template? template = Template.Parse(templateText);

string? result = template.Render(new
{
    BorderT1 = "SetBorderColor 235 35  40",
    BorderT2 = "SetBorderColor 240 90  35",
    BorderT3 = "SetBorderColor 165 95  235",
    BorderT4 = "SetBorderColor 50  95  255",
    BorderT5 = "SetBorderColor 70  190 50",
    BorderT6 = "SetBorderColor 255 255 255",
    FontSizeT1 = "SetFontSize 40",
    FontSizeT2 = "SetFontSize 38",
    FontSizeT3 = "SetFontSize 36",
    FontSizeT4 = "SetFontSize 34",
    FontSizeT5 = "SetFontSize 32",
    FontSizeT6 = "SetFontSize 30",
    BackgroundQuality = "SetBackgroundColor 115 125 140 150",
    
    RuneNamed = string.Join(" ", CurrencyConstants.RuneNamed.Select(x => $"\"{x}\"")),
    EssenceCorrupted = string.Join(" ", CurrencyConstants.EssenceCorrupted.Select(x => $"\"{x}\"")),
    AbyssalPreserved = string.Join(" ", CurrencyConstants.AbyssalPreserved.Select(x => $"\"{x}\"")),
    BreachCatalyst = string.Join(" ", CurrencyConstants.BreachCatalyst.Select(x => $"\"{x}\"")),
    AbyssalAncient = string.Join(" ", CurrencyConstants.AbyssalAncient.Select(x => $"\"{x}\"")),
    
    EquipmentArmor = string.Join(" ", EquipmentConstants.EquipmentArmor.Select(x => $"\"{x}\"")),
    EquipmentArmorEnergyShield = string.Join(" ", EquipmentConstants.EquipmentArmorEnergyShield.Select(x => $"\"{x}\"")),
    EquipmentArmorEvasion = string.Join(" ", EquipmentConstants.EquipmentArmorEvasion.Select(x => $"\"{x}\"")),
    EquipmentEnergyShield = string.Join(" ", EquipmentConstants.EquipmentEnergyShield.Select(x => $"\"{x}\"")),
    EquipmentEvasion = string.Join(" ", EquipmentConstants.EquipmentEvasion.Select(x => $"\"{x}\"")),
    EquipmentEvasionEnergyShield = string.Join(" ", EquipmentConstants.EquipmentEvasionEnergyShield.Select(x => $"\"{x}\"")),
    
    WeaponBow = string.Join(" ", WeaponConstants.WeaponBow.Select(x => $"\"{x}\"")),
    WeaponCrossbow = string.Join(" ", WeaponConstants.WeaponCrossbow.Select(x => $"\"{x}\"")),
    WeaponOneHandedMace = string.Join(" ", WeaponConstants.WeaponOneHandedMace.Select(x => $"\"{x}\"")),
    WeaponQuiver = string.Join(" ", WeaponConstants.WeaponQuiver.Select(x => $"\"{x}\"")),
    WeaponQuarterstaff = string.Join(" ", WeaponConstants.WeaponQuarterstaff.Select(x => $"\"{x}\"")),
    WeaponSceptre = string.Join(" ", WeaponConstants.WeaponSceptre.Select(x => $"\"{x}\"")),
    WeaponStaff = string.Join(" ", WeaponConstants.WeaponStaff.Select(x => $"\"{x}\"")),
    WeaponSpear = string.Join(" ", WeaponConstants.WeaponSpear.Select(x => $"\"{x}\"")),
    WeaponTalisman = string.Join(" ", WeaponConstants.WeaponTalisman.Select(x => $"\"{x}\"")),
    WeaponTwoHandedMace = string.Join(" ", WeaponConstants.WeaponTwoHandedMace.Select(x => $"\"{x}\"")),
    WeaponWand = string.Join(" ", WeaponConstants.WeaponWand.Select(x => $"\"{x}\"")),

}, member => member.Name);

File.WriteAllText(@"C:\Users\Noxy\Documents\My Games\Path of Exile 2\Lewd.filter", result);
