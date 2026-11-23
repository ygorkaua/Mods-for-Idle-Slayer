using System.Collections;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace BreakAllWeapons;

/// <summary>
/// Breaks every armory weapon that is not equipped, using the game's own break logic:
/// for each weapon we call WeaponsManager.BreakPopup (which builds the confirm popup and its
/// "break" action), stop that popup from being shown, and run its confirm action directly.
/// So refunds (Divinity Points), loadout cleanup and saving work exactly as when breaking by hand.
/// </summary>
public class WeaponBreaker : MonoBehaviour
{
    private static bool _capturing;
    private static PopupData _captured;

    private bool _running;
    private bool _buttonCreated;
    private float _nextAutoCheck;
    private bool _slotsLogged;
    private bool _nothingLogged;

    private void Update()
    {
        if (!_buttonCreated && Plugin.ModHelperInstance != null)
        {
            _buttonCreated = true;
            if (Plugin.Settings.ShowPanelButton.Value)
            {
                try { Plugin.ModHelperInstance.CreatePanelButton("Break All", RequestBreakAll); }
                catch (Exception e) { Plugin.Logger.Warning($"Could not create panel button: {e.Message}"); }
            }
        }

        if (Input.GetKeyDown(Plugin.Settings.Hotkey.Value))
            RequestBreakAll();

        if (Plugin.Settings.AutoBreak.Value && !_running && Time.unscaledTime >= _nextAutoCheck)
        {
            _nextAutoCheck = Time.unscaledTime + 2f;
            try { CheckAutoBreak(); }
            catch (Exception e) { Plugin.Logger.Error($"Auto break check failed: {e}"); _nextAutoCheck = Time.unscaledTime + 60f; }
        }
    }

    /// <summary>Armory almost full -> break everything breakable, without asking.</summary>
    private void CheckAutoBreak()
    {
        var wm = WeaponsManager.instance;
        if (wm == null || wm.currentItemsByUid == null || wm.itemDragged != null) return;

        LogTypesOnce(wm);
        int maxSlots = (int)wm.GetMaxSlots();
        int used = wm.currentItemsByUid.Count;
        int free = maxSlots - used;
        if (!_slotsLogged)
        {
            _slotsLogged = true;
            Plugin.Logger.Msg($"Armory: {used}/{maxSlots} slots used, {free} free. Auto break at <= {Plugin.Settings.AutoBreakFreeSlots.Value} free.");
        }
        if (free > Plugin.Settings.AutoBreakFreeSlots.Value) { _nothingLogged = false; return; }

        if (GetTargets(wm).Count == 0)
        {
            if (!_nothingLogged)
            {
                _nothingLogged = true;
                Plugin.Logger.Msg($"Armory almost full ({free} free) but nothing can be broken (all equipped/kept).");
            }
            return;
        }

        Plugin.Logger.Msg($"Armory almost full ({free} free of {maxSlots}): auto breaking.");
        _running = true;
        MelonCoroutines.Start(BreakAll(auto: true));
    }

    private void RequestBreakAll()
    {
        if (_running) return;

        var wm = WeaponsManager.instance;
        if (wm == null) return;

        var targets = GetTargets(wm);
        if (targets.Count == 0)
        {
            Notify("No weapons to break.");
            return;
        }

        long dp = 0;
        foreach (var w in targets)
        {
            try { dp += w.CalculateDivinityPointsRefund(); } catch { /* estimate only */ }
        }

        if (!Plugin.Settings.AskConfirmation.Value || Plugin.ModHelperInstance == null)
        {
            StartBreaking();
            return;
        }

        string keep = "equipped" + (Plugin.Settings.KeepLoadoutWeapons.Value ? ", loadout" : "")
                      + (Plugin.Settings.KeepExcellent.Value ? ", Excellent"
                          : KeptExcellentTypes().Count > 0 ? $", Excellent {Plugin.Settings.KeepExcellentTypes.Value}" : "");
        Plugin.ModHelperInstance.ShowDialog(
            title: "Break all weapons",
            descriptionText: $"Break {targets.Count} armory weapon(s) for ~{dp:N0} Divinity Points?\n\nKept: {keep} weapons and key items.",
            centerDescription: true,
            confirmText: "Break",
            confirmAction: StartBreaking,
            cancelText: "Cancel");
    }

    private void StartBreaking()
    {
        if (_running) return;
        _running = true;
        MelonCoroutines.Start(BreakAll(auto: false));
    }

    private IEnumerator BreakAll(bool auto)
    {
        // Let the confirmation popup finish closing first.
        if (!auto) yield return new WaitForSecondsRealtime(0.3f);

        int broken = 0, failed = 0;
        var wm = WeaponsManager.instance;
        var uids = new List<uint>();
        foreach (var w in GetTargets(wm)) uids.Add(w.uid);

        foreach (uint uid in uids)
        {
            bool ok = false;
            try { ok = BreakOne(wm, uid); }
            catch (Exception e) { Plugin.Logger.Error($"Failed to break weapon {uid}: {e}"); }

            if (ok) broken++; else failed++;
            yield return null; // one per frame so the game doesn't freeze
        }

        _running = false;
        _nextAutoCheck = Time.unscaledTime + (failed > 0 && broken == 0 ? 60f : 2f);
        string msg = (auto ? "Auto break: " : "") + $"{broken} weapon(s) broken" + (failed > 0 ? $", {failed} failed" : "") + ".";
        Plugin.Logger.Msg(msg);
        Notify(msg);
        try { LogArmory(wm); } catch (Exception e) { Plugin.Logger.Warning($"Armory report failed: {e.Message}"); }
    }

    private static bool BreakOne(WeaponsManager wm, uint uid)
    {
        if (!wm.TryGetItem(uid, out var item) || item == null || !IsBreakable(wm, item))
            return false;

        _captured = null;
        _capturing = true;
        try { wm.BreakPopup(item); }
        finally { _capturing = false; }

        var data = _captured;
        _captured = null;
        if (data?.confirmAction == null)
        {
            Plugin.Logger.Warning($"Break popup for weapon {uid} had no confirm action; skipped.");
            return false;
        }

        data.confirmAction.Invoke();
        return !wm.TryGetItem(uid, out _);
    }

    private static List<WeaponItemInventory> GetTargets(WeaponsManager wm)
    {
        var list = new List<WeaponItemInventory>();
        if (wm?.currentItemsByUid == null) return list;
        foreach (var item in wm.currentItemsByUid.Values)
            if (item != null && IsBreakable(wm, item))
                list.Add(item);
        return list;
    }

    private static bool IsBreakable(WeaponsManager wm, WeaponItemInventory item)
    {
        if (wm.IsKey(item)) return false;
        if (wm.IsEquippedEffective(item, false) || wm.IsEquippedForGameplay(item, false)) return false;
        if (Plugin.Settings.KeepLoadoutWeapons.Value && wm.IsItemInAnyLoadout(item.uid)) return false;
        if (item.isExcellent && (Plugin.Settings.KeepExcellent.Value || IsKeptExcellentType(item))) return false;

        bool keepNotEquipped = Plugin.Settings.KeepTypesNotEquipped.Value;
        bool keepUpgrades = Plugin.Settings.KeepExcellentUpgrades.Value;
        if (keepNotEquipped || keepUpgrades)
        {
            var equipped = EquippedTypes(wm);
            bool typeEquipped = equipped.TryGetValue(TypeKey(item), out bool equippedIsExcellent);
            // Any weapon of a type with nothing equipped.
            if (keepNotEquipped && !typeEquipped) return false;
            // Excellent weapon that would be an upgrade: nothing equipped of that type, or the equipped one is
            // neither Excellent nor a key item.
            if (keepUpgrades && item.isExcellent && (!typeEquipped || !equippedIsExcellent)) return false;
        }
        return true;
    }

    private static Dictionary<string, bool> _equippedTypes;
    private static int _equippedTypesFrame = -1;

    /// <summary>
    /// Types (equipment) that have a weapon equipped, normal or key item -> whether an equipped one is
    /// Excellent or a key item (key items count as better than Excellent).
    /// Cached per frame.
    /// </summary>
    private static Dictionary<string, bool> EquippedTypes(WeaponsManager wm)
    {
        if (_equippedTypes != null && _equippedTypesFrame == Time.frameCount) return _equippedTypes;
        var map = new Dictionary<string, bool>();
        // isKey: an equipped key item counts as better than Excellent.
        void Add(Il2CppSystem.Collections.Generic.Dictionary<uint, WeaponItemInventory> dict, bool isKey)
        {
            if (dict == null) return;
            foreach (var it in dict.Values)
            {
                if (it == null) continue;
                bool eq;
                try { eq = wm.IsEquippedEffective(it, false) || wm.IsEquippedForGameplay(it, false); }
                catch { eq = false; }
                if (!eq) continue;
                var k = TypeKey(it);
                map[k] = (map.TryGetValue(k, out var e) && e) || it.isExcellent || isKey;
            }
        }
        Add(wm.currentItemsByUid, false);
        try { Add(wm.currentKeyItemsByUid, true); } catch { /* key items optional */ }
        _equippedTypes = map;
        _equippedTypesFrame = Time.frameCount;
        return map;
    }

    /// <summary>Stable type id: the internal equipment name (e.g. equipment_3).</summary>
    private static string TypeKey(WeaponItemInventory item)
    {
        try { return item.item?.forEquipment?.name ?? item.item?.name ?? "?"; }
        catch { return "?"; }
    }

    /// <summary>Names (lowercase) from the KeepExcellentTypes setting, e.g. "bow, armor".</summary>
    private static HashSet<string> KeptExcellentTypes()
    {
        var set = new HashSet<string>();
        var raw = Plugin.Settings.KeepExcellentTypes.Value;
        if (string.IsNullOrWhiteSpace(raw)) return set;
        foreach (var part in raw.Split(',', ';'))
        {
            var t = part.Trim().ToLowerInvariant();
            if (t.Length > 0) set.Add(t);
        }
        return set;
    }

    /// <summary>
    /// The weapon type is the equipment it belongs to (WeaponItem.forEquipment: Bow, Armor, ...).
    /// Matches the equipment name (English or translated) or the weapon's own name.
    /// </summary>
    private static bool IsKeptExcellentType(WeaponItemInventory item)
    {
        var kept = KeptExcellentTypes();
        if (kept.Count == 0) return false;
        foreach (var name in TypeNames(item))
            if (kept.Contains(name.Trim().ToLowerInvariant())) return true;
        return false;
    }

    private static IEnumerable<string> TypeNames(WeaponItemInventory item)
    {
        var names = new List<string>();
        try
        {
            var w = item.item;
            if (w == null) return names;
            var eq = w.forEquipment;
            if (eq != null)
            {
                if (!string.IsNullOrEmpty(eq.name)) names.Add(eq.name);
                if (!string.IsNullOrEmpty(eq.localizedName)) names.Add(eq.localizedName);
            }
            if (!string.IsNullOrEmpty(w.name)) names.Add(w.name);
            if (!string.IsNullOrEmpty(w.localizedName)) names.Add(w.localizedName);
        }
        catch { /* name lookup is best effort */ }
        return names;
    }

    /// <summary>Equipment name as shown in the shop (e.g. "Armor"); falls back to the internal name.</summary>
    private static string TypeName(WeaponItemInventory item)
    {
        try
        {
            var eq = item.item?.forEquipment;
            if (eq != null)
                return !string.IsNullOrEmpty(eq.localizedName) ? eq.localizedName : eq.name;
            return item.item?.name ?? "?";
        }
        catch { return "?"; }
    }

    private static string WeaponName(WeaponItemInventory item)
    {
        try
        {
            var w = item.item;
            if (w == null) return "?";
            return !string.IsNullOrEmpty(w.localizedName) ? w.localizedName : w.name;
        }
        catch { return "?"; }
    }

    /// <summary>
    /// Logs every weapon in the armory once per game session (and again after each break),
    /// with its type and why it is kept, so the user can see what is equipped and choose KeepExcellentTypes.
    /// </summary>
    private static void LogTypesOnce(WeaponsManager wm)
    {
        if (_typesLogged || wm?.currentItemsByUid == null) return;
        _typesLogged = true;
        LogArmory(wm);
    }

    internal static void LogArmory(WeaponsManager wm)
    {
        if (wm?.currentItemsByUid == null) return;
        var counts = new SortedDictionary<string, int[]>();
        var lines = new List<string>();
        foreach (var item in wm.currentItemsByUid.Values)
        {
            if (item == null) continue;
            var t = TypeName(item);
            if (!counts.TryGetValue(t, out var c)) counts[t] = c = new int[2];
            c[0]++;
            if (item.isExcellent) c[1]++;

            var tags = new List<string>();
            try
            {
                if (wm.IsEquippedEffective(item, false) || wm.IsEquippedForGameplay(item, false)) tags.Add("EQUIPPED");
                if (wm.IsKey(item)) tags.Add("KEY");
                if (wm.IsItemInAnyLoadout(item.uid)) tags.Add("LOADOUT");
            }
            catch { /* tags are best effort */ }
            if (item.isExcellent) tags.Add("excellent");
            bool breakable = IsBreakable(wm, item);
            lines.Add($"  {t} | {WeaponName(item)} | lvl {item.currentLevel} | {(tags.Count > 0 ? string.Join(", ", tags) : "-")} | {(breakable ? "WILL BREAK" : "kept")}");
        }
        Plugin.Logger.Msg("Weapon types in the armory (total/excellent): "
                          + string.Join(", ", counts.Select(kv => $"{kv.Key} {kv.Value[0]}/{kv.Value[1]}")));
        // Key items live in their own list (never touched by Break All).
        try
        {
            if (wm.currentKeyItemsByUid != null)
            {
                foreach (var item in wm.currentKeyItemsByUid.Values)
                {
                    if (item == null) continue;
                    var tags = new List<string> { "KEY" };
                    try
                    {
                        if (wm.IsEquippedEffective(item, false) || wm.IsEquippedForGameplay(item, false)) tags.Insert(0, "EQUIPPED");
                    }
                    catch { /* best effort */ }
                    if (item.isExcellent) tags.Add("excellent");
                    lines.Add($"  {TypeName(item)} | {WeaponName(item)} | lvl {item.currentLevel} | {string.Join(", ", tags)} | kept (key item)");
                }
            }
        }
        catch (Exception e) { Plugin.Logger.Warning($"Could not read key items: {e.Message}"); }

        var kept = KeptExcellentTypes();
        if (kept.Count > 0) Plugin.Logger.Msg($"Keeping excellent: {string.Join(", ", kept)}");
        lines.Sort(StringComparer.OrdinalIgnoreCase);
        Plugin.Logger.Msg($"Armory + key items ({lines.Count} weapons): type | name | level | tags | break-all result\n" + string.Join("\n", lines));
    }

    private static bool _typesLogged;

    private static void Notify(string msg)
    {
        try { Plugin.ModHelperInstance?.ShowNotification(msg, false); } catch { }
    }

    /// <summary>While we are breaking, swallow the game's break popup and keep its data.</summary>
    [HarmonyPatch(typeof(Popup), nameof(Popup.Show))]
    public static class Patch_PopupShow
    {
        private static bool Prefix(PopupData data)
        {
            if (!_capturing) return true;
            _captured = data;
            return false;
        }
    }
}
