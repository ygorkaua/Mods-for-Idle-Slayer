using IdleSlayerMods.Common;
using IdleSlayerMods.Common.Config;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(AutoAscendingHeights.Plugin), "Ascending Heights Autopilot", "1.0.1", "ygorkaua")]
[assembly: MelonAdditionalDependencies("IdleSlayerMods.Common")]

namespace AutoAscendingHeights;

public class Plugin : MelonMod
{
    internal static readonly MelonLogger.Instance Logger = Melon<Plugin>.Logger;
    internal static Settings Settings;
    internal static ModHelper ModHelperInstance;

    public override void OnInitializeMelon()
    {
        Settings = new Settings("AscendingHeightsAutopilot");
        ModHelper.ModHelperMounted += h => ModHelperInstance = h;
        Logger.Msg("Plugin AscendingHeightsAutopilot is loaded!");
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName != "Game") return;
        ModUtils.RegisterComponent<AutoClimber>();
    }
}

internal sealed class Settings(string configName) : BaseConfig(configName)
{
    internal MelonPreferences_Entry<bool> Enabled;
    internal MelonPreferences_Entry<KeyCode> ToggleKey;
    internal MelonPreferences_Entry<bool> UseBoostForce;
    internal MelonPreferences_Entry<bool> LandOnFinish;
    internal MelonPreferences_Entry<bool> WalkToFinish;
    internal MelonPreferences_Entry<float> WalkSpeed;

    protected override void SetBindings()
    {
        Enabled = Bind("Ascending Heights Autopilot", "Enabled", true,
            "Automatically jump in Ascending Heights every time the player starts to fall, so you never fall and keep climbing to the finish. You can still move left/right to grab coins and souls.");
        ToggleKey = Bind("Ascending Heights Autopilot", "ToggleKey", KeyCode.F6,
            "Key that turns the auto climb on/off while playing");
        UseBoostForce = Bind("Ascending Heights Autopilot", "UseBoostForce", false,
            "Jump with the force of a boost platform (climbs faster) instead of a normal platform");
        LandOnFinish = Bind("Ascending Heights Autopilot", "LandOnFinish", true,
            "Stop auto jumping once the player is above the finish bridge, so the player lands on it and the run ends normally");
        WalkToFinish = Bind("Ascending Heights Autopilot", "WalkToFinish", true,
            "Once above the finish bridge, move the player to the finish/reward so the run ends by itself");
        WalkSpeed = Bind("Ascending Heights Autopilot", "WalkSpeed", 10f,
            "How fast (units per second) the player moves to the finish");
    }
}
