using UnityEngine;
using Il2Cpp;

namespace MinionManaging;

/// <summary>
/// Keeps minions working on their own: claims finished missions, prestiges minions that
/// reached their max level, and sends idle minions on a new mission.
///
/// Uses its own timer instead of hooking Minion.CanBeClaimed / IsStandingBy: those are
/// only called by the game while the Minions panel is refreshing, so the old hooks did
/// nothing most of the time.
/// </summary>
public class MinionSender : MonoBehaviour
{
    public static MinionSender Instance { get; private set; }

    private const float TickInterval = 1f;
    private const float SummaryInterval = 300f; // log a short activity summary every 5 minutes
    private float _nextTick;
    private float _nextSummary;
    private bool _statusLogged;
    private static int _claimed, _sent, _prestiged;

    private static readonly HashSet<string> _loggedErrors = new();
    private static readonly HashSet<string> _prestigeBlocked = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Time.unscaledTime < _nextTick) return;
        _nextTick = Time.unscaledTime + TickInterval;

        try
        {
            if (!_statusLogged) _statusLogged = LogStatus();
            Tick();
            if (Time.unscaledTime >= _nextSummary)
            {
                if (_nextSummary > 0 && (_claimed + _sent + _prestiged) > 0)
                    Plugin.Logger.Msg($"Last 5 min: claimed {_claimed}, sent {_sent}, prestiged {_prestiged}.");
                _claimed = _sent = _prestiged = 0;
                _nextSummary = Time.unscaledTime + SummaryInterval;
            }
        }
        catch (Exception e)
        {
            if (_loggedErrors.Add(e.GetType().Name + e.Message))
                Plugin.Logger.Error($"MinionSender tick failed: {e}");
        }
    }

    private static void Tick()
    {
        var inventory = PlayerInventory.instance;
        if (inventory == null || inventory.minions == null) return;

        foreach (var minion in inventory.minions)
        {
            if (minion == null || !minion.IsUnlocked()) continue;

            try { HandleMinion(minion); }
            catch (Exception e)
            {
                if (_loggedErrors.Add(minion.name + e.GetType().Name + e.Message))
                    Plugin.Logger.Error($"Minion '{minion.name}' failed: {e}");
            }
        }
    }

    private static void HandleMinion(Minion minion)
    {
        // 1) Mission finished -> claim the reward (this is also what levels the minion up).
        if (Plugin.Settings.AutoClaim.Value && minion.CanBeClaimed())
        {
            minion.ClaimQuest(true);
            _claimed++;
        }

        // 2) Prestige before sending it again.
        if (Plugin.Settings.AutoPrestige.Value && minion.IsStandingBy() && ShouldPrestige(minion))
        {
            var divinities = DivinitiesManager.instance;
            string id = minion.name ?? "";
            if (divinities != null && !_prestigeBlocked.Contains(id))
            {
                int before = minion.level;
                if (!PrestigeEveryMission)
                    Plugin.Logger.Msg($"Prestiging minion '{minion.localizedName ?? id}' (level {before}).");
                divinities.PrestigeMinion(minion);
                _prestiged++;

                // Safety: if the game didn't reset the level, don't keep trying every second.
                if (minion.level >= before)
                {
                    _prestigeBlocked.Add(id);
                    Plugin.Logger.Warning($"Prestige of '{id}' had no effect; auto-prestige disabled for it this session.");
                }
            }
        }

        // 3) Idle -> send on a new mission if we can pay for it.
        if (Plugin.Settings.AutoSend.Value && minion.IsStandingBy() && minion.GetCost() <= SlayerPoints.points)
        {
            minion.GiveQuest();
            _sent++;
        }
    }

    /// <summary>One-time overview of every unlocked minion, to make the log easy to check.</summary>
    private static bool LogStatus()
    {
        var inventory = PlayerInventory.instance;
        if (inventory == null || inventory.minions == null || inventory.minions.Length == 0) return false;

        var parts = new List<string>();
        foreach (var m in inventory.minions)
        {
            if (m == null || !m.IsUnlocked()) continue;
            string state = m.CanBeClaimed() ? "done" : m.IsDoingQuest() ? "working" : m.IsStandingBy() ? "idle" : "?";
            parts.Add($"{m.name} {m.level}/{m.GetCurrentMaxLevel()} {state}");
        }
        Plugin.Logger.Msg($"Minions ({parts.Count}): " + string.Join(", ", parts));
        if (Plugin.Settings.AutoPrestige.Value)
            Plugin.Logger.Msg(PrestigeEveryMission
                ? "Auto prestige: after every mission (Enhanced_Divinities pays it as max level)."
                : Plugin.Settings.PrestigeAsSoonAsPossible.Value
                    ? "Auto prestige: at max level only (Enhanced_Divinities not found)."
                    : "Auto prestige: at max level only.");
        return true;
    }

    /// <summary>
    /// Enhanced_Divinities pays every prestige as if the minion were at max level, so with it
    /// installed the best play is to prestige after every mission. Level > 1 means the minion
    /// finished at least one mission since its last prestige (one prestige per mission).
    /// </summary>
    private static bool? _enhancedDivinities;
    private static bool PrestigeEveryMission
    {
        get
        {
            _enhancedDivinities ??= MelonLoader.MelonBase.RegisteredMelons.Any(m => m.Info.Name == "Enhanced_Divinities");
            return Plugin.Settings.PrestigeAsSoonAsPossible.Value && _enhancedDivinities.Value;
        }
    }

    private static bool ShouldPrestige(Minion minion)
        => PrestigeEveryMission ? minion.level > 1 : IsAtMaxLevel(minion);

    private static bool IsAtMaxLevel(Minion minion)
    {
        int max = minion.GetCurrentMaxLevel();
        return max > 1 && minion.level >= max;
    }
}
