using IdleSlayerMods.Common;
using IdleSlayerMods.Common.Config;
using MelonLoader;

[assembly: MelonInfo(typeof(OpenAllEggs.Plugin), "Open All Eggs", "1.0.0", "ygorkaua")]
[assembly: MelonAdditionalDependencies("IdleSlayerMods.Common")]

namespace OpenAllEggs;

public class Plugin : MelonMod
{
    internal static readonly MelonLogger.Instance Logger = Melon<Plugin>.Logger;
    internal static Settings Settings;
    internal static ModHelper ModHelperInstance;

    public override void OnInitializeMelon()
    {
        Settings = new Settings("OpenAllEggs");
        ModHelper.ModHelperMounted += h => ModHelperInstance = h;
        Logger.Msg("Plugin OpenAllEggs is loaded!");
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName != "Game") return;
        ModUtils.RegisterComponent<Unstuck>();
    }
}

/// <summary>
/// Safety net: if the game is ever left in Cutscene mode by a loot box (HUD hidden, character
/// frozen), pressing the key runs the game's own CloseLootBoxModal, which restores RunnerMode.
/// </summary>
public class Unstuck : UnityEngine.MonoBehaviour
{
    private void Update()
    {
        if (!UnityEngine.Input.GetKeyDown(Plugin.Settings.UnstuckKey.Value)) return;
        try
        {
            var state = Il2Cpp.GameState.current;
            var lbm = Il2Cpp.LootBoxManager.instance;
            if (state == Il2Cpp.GameStates.Cutscene && lbm != null)
            {
                lbm.CloseLootBoxModal();
                Plugin.Logger.Msg($"Unstuck: closed loot box window, game state {state} -> {Il2Cpp.GameState.current}.");
            }
            else
                Plugin.Logger.Msg($"Unstuck: nothing to do (game state {state}).");
        }
        catch (System.Exception e) { Plugin.Logger.Error($"Unstuck failed: {e}"); }
    }
}

internal sealed class Settings(string configName) : BaseConfig(configName)
{
    internal MelonPreferences_Entry<bool> Enabled;
    internal MelonPreferences_Entry<int> OpensPerFrame;
    internal MelonPreferences_Entry<UnityEngine.KeyCode> UnstuckKey;

    protected override void SetBindings()
    {
        Enabled = Bind("Open All Eggs", "Enabled", true,
            "Clicking 'Open' on an egg (or any temporary item that opens a loot box) opens all of them");
        OpensPerFrame = Bind("Open All Eggs", "OpensPerFrame", 5,
            "How many to open per frame (higher = faster, but may stutter)");
        UnstuckKey = Bind("Open All Eggs", "UnstuckKey", UnityEngine.KeyCode.F9,
            "If the game gets stuck after opening a loot box (no HUD, character frozen), press this to close the loot box window");
    }
}
