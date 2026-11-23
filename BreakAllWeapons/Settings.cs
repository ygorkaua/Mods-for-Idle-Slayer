using IdleSlayerMods.Common.Config;
using MelonLoader;
using UnityEngine;

namespace BreakAllWeapons;

internal sealed class Settings(string configName) : BaseConfig(configName)
{
    internal MelonPreferences_Entry<KeyCode> Hotkey;
    internal MelonPreferences_Entry<bool> ShowPanelButton;
    internal MelonPreferences_Entry<bool> AskConfirmation;
    internal MelonPreferences_Entry<bool> KeepLoadoutWeapons;
    internal MelonPreferences_Entry<bool> KeepExcellent;
    internal MelonPreferences_Entry<string> KeepExcellentTypes;
    internal MelonPreferences_Entry<bool> KeepTypesNotEquipped;
    internal MelonPreferences_Entry<bool> KeepExcellentUpgrades;
    internal MelonPreferences_Entry<bool> AutoBreak;
    internal MelonPreferences_Entry<int> AutoBreakFreeSlots;

    protected override void SetBindings()
    {
        Hotkey = Bind("Break All Weapons", "Hotkey", KeyCode.F8,
            "Key that breaks every weapon in the armory that is not equipped");
        ShowPanelButton = Bind("Break All Weapons", "ShowPanelButton", true,
            "Add a 'Break All' button to the side panel (next to Settings)");
        AskConfirmation = Bind("Break All Weapons", "AskConfirmation", true,
            "Show a confirmation popup before breaking");
        KeepLoadoutWeapons = Bind("Break All Weapons", "KeepLoadoutWeapons", true,
            "Do not break weapons that are saved in a loadout, even if not equipped right now");
        KeepExcellent = Bind("Break All Weapons", "KeepExcellent", false,
            "Do not break Excellent weapons (all types)");
        KeepExcellentTypes = Bind("Break All Weapons", "KeepExcellentTypes", "",
            "Do not break Excellent weapons of these types only, comma separated equipment names, e.g. \"Bow, Armor\". The log lists the types in your armory. Empty = none (unless KeepExcellent is true)");
        KeepTypesNotEquipped = Bind("Break All Weapons", "KeepTypesNotEquipped", false,
            "Do not break any weapon (any rarity) of a type you have nothing equipped in (e.g. Gloves when no Gloves is equipped)");
        KeepExcellentUpgrades = Bind("Break All Weapons", "KeepExcellentUpgrades", false,
            "Do not break Excellent weapons of a type where nothing is equipped or the equipped weapon is not Excellent. An equipped key item counts as better than Excellent, so Excellent weapons of that type are broken.");
        AutoBreak = Bind("Break All Weapons", "AutoBreak", true,
            "Automatically break all weapons (same rules as above, no confirmation) when the armory is almost full");
        AutoBreakFreeSlots = Bind("Break All Weapons", "AutoBreakFreeSlots", 2,
            "Auto break triggers when the number of empty armory slots is this value or less");
    }
}
