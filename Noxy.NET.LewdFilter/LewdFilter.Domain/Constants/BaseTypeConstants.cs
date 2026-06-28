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
        { "Tablets", MiscellaneousBaseTypeConstants.Tablet },
        { "Wombgifts", MiscellaneousBaseTypeConstants.Wombgift },
        { "Relics", MiscellaneousBaseTypeConstants.Relics },
        { "Flasks (Life)", MiscellaneousBaseTypeConstants.FlaskLife },
        { "Flasks (Mana)", MiscellaneousBaseTypeConstants.FlaskMana },

        { "Skill Gems", MiscellaneousBaseTypeConstants.SkillGem },
        { "Skill Gems (Kalguuran)", MiscellaneousBaseTypeConstants.SkillGemKalguuran },
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

        { "Armor: Helmets", HelmetBaseTypeConstants.Armor },
        { "Armor: Chests", BodyArmourBaseTypeConstants.Armor },
        { "Armor: Gloves", GlovesBaseTypeConstants.Armor },
        { "Armor: Boots", BootsBaseTypeConstants.Armor },
        { "Armor: Shields", OffhandBaseTypeConstants.Armor },

        { "Evasion: Helmets", HelmetBaseTypeConstants.Evasion },
        { "Evasion: Chests", BodyArmourBaseTypeConstants.Evasion },
        { "Evasion: Gloves", GlovesBaseTypeConstants.Evasion },
        { "Evasion: Boots", BootsBaseTypeConstants.Evasion },
        { "Evasion: Shields", OffhandBaseTypeConstants.Evasion },

        { "Energy Shield: Helmets", HelmetBaseTypeConstants.EnergyShield },
        { "Energy Shield: Chests", BodyArmourBaseTypeConstants.EnergyShield },
        { "Energy Shield: Gloves", GlovesBaseTypeConstants.EnergyShield },
        { "Energy Shield: Boots", BootsBaseTypeConstants.EnergyShield },
        { "Energy Shield: Foci", OffhandBaseTypeConstants.EnergyShield },

        { "Armor/Evasion: Helmets", HelmetBaseTypeConstants.ArmorEvasion },
        { "Armor/Evasion: Chests", BodyArmourBaseTypeConstants.ArmorEvasion },
        { "Armor/Evasion: Gloves", GlovesBaseTypeConstants.ArmorEvasion },
        { "Armor/Evasion: Boots", BootsBaseTypeConstants.ArmorEvasion },
        { "Armor/Evasion: Shields", OffhandBaseTypeConstants.ArmorEvasion },

        { "Armor/Energy Shield: Helmets", HelmetBaseTypeConstants.ArmorEnergyShield },
        { "Armor/Energy Shield: Chests", BodyArmourBaseTypeConstants.ArmorEnergyShield },
        { "Armor/Energy Shield: Gloves", GlovesBaseTypeConstants.ArmorEnergyShield },
        { "Armor/Energy Shield: Boots", BootsBaseTypeConstants.ArmorEnergyShield },
        { "Armor/Energy Shield: Shields", OffhandBaseTypeConstants.ArmorEnergyShield },

        { "Evasion/Energy Shield: Helmets", HelmetBaseTypeConstants.EvasionEnergyShield },
        { "Evasion/Energy Shield: Chests", BodyArmourBaseTypeConstants.EvasionEnergyShield },
        { "Evasion/Energy Shield: Gloves", GlovesBaseTypeConstants.EvasionEnergyShield },
        { "Evasion/Energy Shield: Boots", BootsBaseTypeConstants.EvasionEnergyShield },
    };
}
