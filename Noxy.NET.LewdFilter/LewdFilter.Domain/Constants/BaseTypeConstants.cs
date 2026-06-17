namespace LewdFilter.Domain.Constants;

public static class BaseTypeConstants
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> BaseTypes = new Dictionary<string, IReadOnlyList<string>>
    {
        { "Currency (Basic)", CurrencyBaseTypeConstants.CurrencyBase },
        { "Currency (Abyssal)", CurrencyBaseTypeConstants.CurrencyAbyssal },
        { "Currency (Aldur)", CurrencyBaseTypeConstants.CurrencyAldur },
        { "Currency (Vaal)", CurrencyBaseTypeConstants.CurrencyVaal },

        { "Catalysts (Basic)", CurrencyBaseTypeConstants.Catalyst },
        { "Catalysts (Refined)", CurrencyBaseTypeConstants.CatalystRefined },

        { "Liquid Emotions", CurrencyBaseTypeConstants.LiquidEmotion },

        { "Alloys", CurrencyBaseTypeConstants.Alloys },

        { "Essences (Unique)", CurrencyBaseTypeConstants.EssenceUnique },
        { "Essences (Perfect)", CurrencyBaseTypeConstants.EssencePerfect },
        { "Essences (Greater)", CurrencyBaseTypeConstants.EssenceGreater },
        { "Essences (Normal)", CurrencyBaseTypeConstants.EssenceNormal },
        { "Essences (Lesser)", CurrencyBaseTypeConstants.EssenceLesser },

        { "Augments (Aldur)", AugmentBaseTypeConstants.AugmentAldur },
        { "Augments (Ancient)", AugmentBaseTypeConstants.AugmentAncient },
        { "Augments (Basic)", AugmentBaseTypeConstants.AugmentBase },
        { "Augments (Idols)", AugmentBaseTypeConstants.AugmentIdol },
        { "Augments (Soul cores)", AugmentBaseTypeConstants.AugmentSoulCore },
        { "Augments (Unique)", AugmentBaseTypeConstants.AugmentUnique },

        { "Omens (Basic)", CurrencyBaseTypeConstants.OmenBase },
        { "Omens (Abyssal)", CurrencyBaseTypeConstants.OmenAbyssal },

        { "Fragments", MiscellaneousBaseTypeConstants.Fragment },
        { "Reliquary Keys", MiscellaneousBaseTypeConstants.ReliquaryKey },
        { "Tablets", MiscellaneousBaseTypeConstants.Tablet },
        { "Wombgifts", MiscellaneousBaseTypeConstants.Wombgift },
        { "Relics", MiscellaneousBaseTypeConstants.Relics },
        { "Flasks (Life)", MiscellaneousBaseTypeConstants.FlaskLife },
        { "Flasks (Mana)", MiscellaneousBaseTypeConstants.FlaskMana },

        { "Lineage Gems (Base)", MiscellaneousBaseTypeConstants.LineageSupportGemBase },
        { "Lineage Gems (Abyss)", MiscellaneousBaseTypeConstants.LineageSupportGemAbyss },
        { "Lineage Gems (Aldur)", MiscellaneousBaseTypeConstants.LineageSupportGemAldur },
        { "Lineage Gems (Anomaly)", MiscellaneousBaseTypeConstants.LineageSupportGemAnomaly },
        { "Lineage Gems (Arbiter)", MiscellaneousBaseTypeConstants.LineageSupportGemArbiter },
        { "Lineage Gems (Breach)", MiscellaneousBaseTypeConstants.LineageSupportGemBreach },
        { "Lineage Gems (Misc)", MiscellaneousBaseTypeConstants.LineageSupportGemMisc },
        { "Lineage Gems (Temple)", MiscellaneousBaseTypeConstants.LineageSupportGemTemple },

        { "Weapons: One-Hand Maces", WeaponBaseTypeConstants.OneHandedMace },
        { "Weapons: Two-Hand Maces", WeaponBaseTypeConstants.TwoHandedMace },
        { "Weapons: Staves", WeaponBaseTypeConstants.Staff },
        { "Weapons: Quarterstaves", WeaponBaseTypeConstants.Quarterstaff },
        { "Weapons: Bows", WeaponBaseTypeConstants.Bow },
        { "Weapons: Crossbows", WeaponBaseTypeConstants.Crossbow },
        { "Weapons: Wands", WeaponBaseTypeConstants.Wand },
        { "Weapons: Sceptres", WeaponBaseTypeConstants.Sceptre },
        { "Weapons: Spears", WeaponBaseTypeConstants.Spear },
        { "Weapons: Quivers", WeaponBaseTypeConstants.Quiver },
        { "Weapons: Talismans", WeaponBaseTypeConstants.Talismans },

        { "Amulets (Basic)", JewelleryBaseTypeConstants.AmuletBase },
        { "Amulets (Restricted)", JewelleryBaseTypeConstants.AmuletRestricted },
        { "Rings (Basic)", JewelleryBaseTypeConstants.RingBase },
        { "Rings (Restricted)", JewelleryBaseTypeConstants.RingRestricted },
        { "Belts", JewelleryBaseTypeConstants.Belt },
        { "Charms", JewelleryBaseTypeConstants.Charm },
        { "Jewels", MiscellaneousBaseTypeConstants.Jewel },

        { "Armor: Helmets", ArmorBaseTypeConstants.ArmorHelmet },
        { "Armor: Chests", ArmorBaseTypeConstants.ArmorChest },
        { "Armor: Gloves", ArmorBaseTypeConstants.ArmorGloves },
        { "Armor: Boots", ArmorBaseTypeConstants.ArmorBoots },
        { "Armor: Shields", ArmorBaseTypeConstants.ArmorShield },

        { "Evasion: Helmets", ArmorBaseTypeConstants.EvasionHelmet },
        { "Evasion: Chests", ArmorBaseTypeConstants.EvasionChest },
        { "Evasion: Gloves", ArmorBaseTypeConstants.EvasionGloves },
        { "Evasion: Boots", ArmorBaseTypeConstants.EvasionBoots },
        { "Evasion: Shields", ArmorBaseTypeConstants.EvasionShield },

        { "Energy Shield: Helmets", ArmorBaseTypeConstants.EnergyShieldHelmet },
        { "Energy Shield: Chests", ArmorBaseTypeConstants.EnergyShieldChest },
        { "Energy Shield: Gloves", ArmorBaseTypeConstants.EnergyShieldGloves },
        { "Energy Shield: Boots", ArmorBaseTypeConstants.EnergyShieldBoots },
        { "Energy Shield: Foci", ArmorBaseTypeConstants.EnergyShieldFocus },

        { "Armor/Evasion: Helmets", ArmorBaseTypeConstants.ArmorEvasionHelmet },
        { "Armor/Evasion: Chests", ArmorBaseTypeConstants.ArmorEvasionChest },
        { "Armor/Evasion: Gloves", ArmorBaseTypeConstants.ArmorEvasionGloves },
        { "Armor/Evasion: Boots", ArmorBaseTypeConstants.ArmorEvasionBoots },
        { "Armor/Evasion: Shields", ArmorBaseTypeConstants.ArmorEvasionShield },

        { "Armor/Energy Shield: Helmets", ArmorBaseTypeConstants.ArmorEnergyShieldHelmet },
        { "Armor/Energy Shield: Chests", ArmorBaseTypeConstants.ArmorEnergyShieldChest },
        { "Armor/Energy Shield: Gloves", ArmorBaseTypeConstants.ArmorEnergyShieldGloves },
        { "Armor/Energy Shield: Boots", ArmorBaseTypeConstants.ArmorEnergyShieldBoots },
        { "Armor/Energy Shield: Shields", ArmorBaseTypeConstants.ArmorEnergyShieldShield },

        { "Evasion/Energy Shield: Helmets", ArmorBaseTypeConstants.EvasionEnergyShieldHelmet },
        { "Evasion/Energy Shield: Chests", ArmorBaseTypeConstants.EvasionEnergyShieldChest },
        { "Evasion/Energy Shield: Gloves", ArmorBaseTypeConstants.EvasionEnergyShieldGloves },
        { "Evasion/Energy Shield: Boots", ArmorBaseTypeConstants.EvasionEnergyShieldBoots },
    };
}
