using IdleSlayerMods.Common;
using MelonLoader;
using BreakAllWeapons;

[assembly: MelonInfo(typeof(BreakAllWeapons.Plugin), "Break All Weapons", "1.0.0", "ygorkaua")]
[assembly: MelonAdditionalDependencies("IdleSlayerMods.Common")]

namespace BreakAllWeapons;

public class Plugin : MelonMod
{
    internal static readonly MelonLogger.Instance Logger = Melon<Plugin>.Logger;
    internal static Settings Settings;
    internal static ModHelper ModHelperInstance;

    public override void OnInitializeMelon()
    {
        Settings = new Settings("BreakAllWeapons");
        ModHelper.ModHelperMounted += h => ModHelperInstance = h;
        Logger.Msg("Plugin BreakAllWeapons is loaded!");
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName != "Game") return;
        ModUtils.RegisterComponent<WeaponBreaker>();
    }
}
