using IdleSlayerMods.Common.Config;
using MelonLoader;

namespace MinionManaging;

internal sealed class Settings(string configName) : BaseConfig(configName)
{
    internal MelonPreferences_Entry<bool> AutoClaim;
    internal MelonPreferences_Entry<bool> AutoSend;
    internal MelonPreferences_Entry<bool> AutoPrestige;
    internal MelonPreferences_Entry<bool> PrestigeAsSoonAsPossible;

    protected override void SetBindings()
    {
        AutoClaim = Bind("Minion Sender", "AutoClaim", true,
            "Automatically claim minion rewards when their mission is done");
        AutoSend = Bind("Minion Sender", "AutoSend", true,
            "Automatically send idle minions on a new mission");
        AutoPrestige = Bind("Minion Sender", "AutoPrestige", true,
            "Automatically prestige minions");
        PrestigeAsSoonAsPossible = Bind("Minion Sender", "PrestigeAsSoonAsPossible", true,
            "true: prestige after every finished mission (needs Enhanced_Divinities, which pays every prestige as max level). " +
            "false: only prestige when the minion reaches its max level");
    }
}
