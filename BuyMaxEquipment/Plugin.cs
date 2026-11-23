using IdleSlayerMods.Common;
using IdleSlayerMods.Common.Config;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(BuyMaxEquipment.Plugin), "Buy Max Equipment", "1.0.0", "ygorkaua")]
[assembly: MelonAdditionalDependencies("IdleSlayerMods.Common")]

namespace BuyMaxEquipment;

public class Plugin : MelonMod
{
    internal static readonly MelonLogger.Instance Logger = Melon<Plugin>.Logger;
    internal static Settings Settings;
    internal static ModHelper ModHelperInstance;

    public override void OnInitializeMelon()
    {
        Settings = new Settings("BuyMaxEquipment");
        ModHelper.ModHelperMounted += h => ModHelperInstance = h;
        Logger.Msg("Plugin BuyMaxEquipment is loaded!");
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName != "Game") return;
        ModUtils.RegisterComponent<EquipmentBuyer>();
    }
}

internal sealed class Settings(string configName) : BaseConfig(configName)
{
    internal MelonPreferences_Entry<KeyCode> Hotkey;
    internal MelonPreferences_Entry<bool> ShowPanelButton;
    internal MelonPreferences_Entry<float> DelaySeconds;
    internal MelonPreferences_Entry<bool> WaitAfterNoPurchase;
    internal MelonPreferences_Entry<bool> BuyUpgrades;

    protected override void SetBindings()
    {
        Hotkey = Bind("Buy Max Equipment", "Hotkey", KeyCode.F7,
            "Key that starts buying Max of every unlocked equipment, from the last one to the first. Press again to cancel.");
        ShowPanelButton = Bind("Buy Max Equipment", "ShowPanelButton", true,
            "Add a 'Buy Max' button to the side panel (next to Settings)");
        DelaySeconds = Bind("Buy Max Equipment", "DelaySeconds", 0.5f,
            "Seconds to wait (real time) between one equipment and the next");
        WaitAfterNoPurchase = Bind("Buy Max Equipment", "WaitAfterNoPurchase", false,
            "Also wait when an equipment could not be bought (not enough gold). If false, it goes straight to the next one.");
        BuyUpgrades = Bind("Buy Max Equipment", "BuyUpgrades", true,
            "Also buy every affordable upgrade (the shop's 'Buy All' upgrades button), before the equipment and again after it");
    }
}
