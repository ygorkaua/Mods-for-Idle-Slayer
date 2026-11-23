using System.Collections;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace OpenAllEggs;

/// <summary>
/// After the player opens one egg in Temporary Items, open the rest of them too.
///
/// How the game works (read from GameAssembly):
///  - TemporaryCraftingManager.CraftItem refuses loot-box items unless GameState is RunnerMode.
///  - LootBoxManager.OpenLootBox (without the Egg Master divinity) sets GameState = Cutscene,
///    plays the "open" animation and grants the reward; CloseLootBoxModal sets it back to RunnerMode.
///
/// v1.0.x pressed CraftItem again, which needed Close+Open in the same frame; sometimes the
/// "close" animation won, hiding the window while the game stayed in Cutscene (HUD gone,
/// character frozen). Now we call item.Craft() directly (after the same material check the
/// button does) while the window stays open, and never close it ourselves: the player closes
/// it once at the end, exactly like after a normal single open.
/// </summary>
[HarmonyPatch(typeof(TemporaryCraftingManager), nameof(TemporaryCraftingManager.CraftItem))]
public static class Patch_CraftItem
{
    private const int SafetyCap = 100000;

    private static bool _inLoop;
    private static bool _running;
    private static bool _watching;
    private static int _boxesOpened;
    private static string _lastReward;
    private static readonly Dictionary<string, int> _rewards = new();
    private static readonly HashSet<string> _errors = new();

    private static void Prefix()
    {
        if (_inLoop || _running || !Plugin.Settings.Enabled.Value) return;
        _watching = true;
        _boxesOpened = 0;
    }

    private static void Postfix(TemporaryCraftingManager __instance)
    {
        if (_inLoop || _running || !_watching) return;
        _watching = false;
        try
        {
            var item = __instance.selectedItem;
            if (item == null || _boxesOpened == 0) return; // not a loot-box item, or the game refused
            if (!item.HasRequiredMaterials()) return;      // that was the last one

            _rewards.Clear();
            AddReward(_lastReward); // reward of the player's own click
            _running = true;
            MelonCoroutines.Start(OpenRest(__instance, item));
        }
        catch (Exception e) { LogOnce("postfix", e); }
    }

    private static IEnumerator OpenRest(TemporaryCraftingManager mgr, TemporaryCraftableItem item)
    {
        int opened = 1; // the player's own click
        string stop = null;
        int perFrame = Math.Max(1, Plugin.Settings.OpensPerFrame.Value);
        var wm = WeaponsManager.instance;

        while (opened < SafetyCap)
        {
            bool hasMore;
            try { hasMore = item.HasRequiredMaterials(); }
            catch (Exception e) { LogOnce("check materials", e); break; }
            if (!hasMore) break;
            if (item.requiresFreeArmorySpace && wm != null && !wm.HasFreeSlot()) { stop = "armory is full"; break; }

            _boxesOpened = 0;
            _inLoop = true;
            try { item.Craft(); }
            catch (Exception e) { LogOnce("open", e); }
            finally { _inLoop = false; }

            if (_boxesOpened == 0) { stop = "the game did not open another one"; break; }
            opened++;

            if (opened % perFrame == 0) yield return null;
        }

        _running = false;
        string summary = _rewards.Count == 0 ? "" :
            " Rewards: " + string.Join(", ", _rewards.OrderByDescending(kv => kv.Value)
                .Select(kv => kv.Value > 1 ? $"{kv.Value}x {kv.Key}" : kv.Key));
        Plugin.Logger.Msg($"Opened {opened} '{item.name}'" + (stop != null ? $" (stopped: {stop})" : "") + "." + summary);
        try { Plugin.ModHelperInstance?.ShowNotification($"{opened} opened" + (stop == "armory is full" ? " (armory full)" : ""), false); } catch { }
    }

    [HarmonyPatch(typeof(LootBoxManager), nameof(LootBoxManager.OpenLootBox))]
    public static class Patch_OpenLootBox
    {
        private static void Postfix(LootBoxManager __instance)
        {
            if (!_watching && !_inLoop) return;
            _boxesOpened++;
            try
            {
                string r = __instance.currentLoot?.rewardString;
                r = string.IsNullOrEmpty(r) ? null : System.Text.RegularExpressions.Regex.Replace(r, "<.*?>", "").Trim();
                _lastReward = r;
                if (_running) AddReward(r);
            }
            catch { }
        }
    }

    private static void AddReward(string r)
    {
        if (string.IsNullOrEmpty(r)) return;
        _rewards[r] = _rewards.TryGetValue(r, out int c) ? c + 1 : 1;
    }

    private static void LogOnce(string where, Exception e)
    {
        if (_errors.Add(where + e.GetType().Name + e.Message))
            Plugin.Logger.Error($"OpenAllEggs {where} failed: {e}");
    }
}
