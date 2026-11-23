using System.Collections;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace BuyMaxEquipment;

/// <summary>
/// Buys all upgrades (before and after the equipment) and "Max" of every unlocked equipment (Powers in the game code), starting from the last
/// unlocked one and going back to the first, waiting DelaySeconds between them.
/// Each purchase is the game's own ShopManager.BuyPower (the same call the shop's buy button makes),
/// with the stack selector temporarily set to Max (-1), so gold checks, achievements and UI updates
/// work exactly as when buying by hand.
/// </summary>
public class EquipmentBuyer : MonoBehaviour
{
    private const int StackMax = -1;

    private bool _buttonCreated;
    private bool _running;
    private bool _cancel;

    private void Update()
    {
        if (!_buttonCreated && Plugin.ModHelperInstance != null)
        {
            _buttonCreated = true;
            if (Plugin.Settings.ShowPanelButton.Value)
            {
                try { Plugin.ModHelperInstance.CreatePanelButton("Buy Max", Toggle); }
                catch (Exception e) { Plugin.Logger.Warning($"Could not create panel button: {e.Message}"); }
            }
        }

        if (Input.GetKeyDown(Plugin.Settings.Hotkey.Value))
            Toggle();
    }

    private void Toggle()
    {
        if (_running)
        {
            _cancel = true;
            Notify("Purchase cancelled");
            return;
        }
        _running = true;
        _cancel = false;
        MelonCoroutines.Start(BuyAll());
    }

    private static List<Power> GetUnlockedLastToFirst()
    {
        var result = new List<Power>();
        var inv = PlayerInventory.instance;
        if (inv == null || inv.powers == null) return result;

        var list = PowersList.instance;
        foreach (var p in inv.powers)
        {
            if (p == null) continue;
            bool unlocked = list != null
                ? list.CheckRequirements(p)
                : p.level > 0 || (p.lockedBehindUpgrade != null && p.lockedBehindUpgrade.bought);
            if (unlocked) result.Add(p);
        }
        result.Reverse();
        return result;
    }

    private IEnumerator BuyAll()
    {
        int totalItems = 0, totalLevels = 0, totalUpgrades = 0;
        var summary = new List<string>();
        float delay = Math.Max(0f, Plugin.Settings.DelaySeconds.Value);

        if (ShopManager.instance == null)
        {
            Plugin.Logger.Msg("Shop not ready.");
            _running = false;
            yield break;
        }

        Notify("Buying...");

        // Upgrades first: they can unlock new equipment and make it stronger.
        if (Plugin.Settings.BuyUpgrades.Value)
        {
            int n = BuyAllUpgrades();
            totalUpgrades += n;
            if (n > 0 && !_cancel) yield return new WaitForSecondsRealtime(delay);
        }

        List<Power> powers;
        try { powers = GetUnlockedLastToFirst(); }
        catch (Exception e)
        {
            Plugin.Logger.Error($"Could not read the equipment list: {e}");
            powers = new List<Power>();
        }

        if (powers.Count == 0)
            Plugin.Logger.Msg("No unlocked equipment found.");
        else
            Plugin.Logger.Msg($"Buying Max of {powers.Count} equipment, last to first: {string.Join(" -> ", powers.Select(Name))}");

        for (int i = 0; i < powers.Count && !_cancel; i++)
        {
            var power = powers[i];
            int bought = 0;
            try { bought = BuyMax(power); }
            catch (Exception e) { Plugin.Logger.Error($"Buying {Name(power)} failed: {e}"); }

            if (bought > 0)
            {
                totalItems++;
                totalLevels += bought;
                summary.Add($"{Name(power)} +{bought}");
                Plugin.Logger.Msg($"{Name(power)}: +{bought} (level {power.level})");
            }
            else
            {
                Plugin.Logger.Msg($"{Name(power)}: not enough gold for 1 level");
            }

            bool last = i == powers.Count - 1;
            if (!last && (bought > 0 || Plugin.Settings.WaitAfterNoPurchase.Value))
                yield return new WaitForSecondsRealtime(delay);
        }

        // Upgrades again: new equipment levels can unlock more upgrades.
        if (Plugin.Settings.BuyUpgrades.Value && !_cancel && totalLevels > 0)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (!_cancel) totalUpgrades += BuyAllUpgrades();
        }

        string end = _cancel ? "cancelled" : "done";
        Plugin.Logger.Msg($"Buy Max {end}: {totalUpgrades} upgrades, {totalItems} equipment, {totalLevels} levels. {string.Join(", ", summary)}");
        if (!_cancel)
        {
            var parts = new List<string>();
            if (totalUpgrades > 0) parts.Add($"{totalUpgrades} upgrades");
            if (totalLevels > 0) parts.Add($"{totalLevels} levels on {totalItems} equipment");
            Notify(parts.Count > 0 ? "Bought: " + string.Join(" + ", parts) : "Not enough gold");
        }
        _running = false;
        _cancel = false;
    }

    /// <summary>
    /// The shop's own "Buy All" upgrades button (ShopManager.BuyAllUpgrades).
    /// Returns how many upgrades went from not bought to bought.
    /// </summary>
    private static int BuyAllUpgrades()
    {
        try
        {
            var shop = ShopManager.instance;
            if (shop == null) return 0;
            int before = CountBoughtUpgrades();
            shop.BuyAllUpgrades();
            int bought = Math.Max(0, CountBoughtUpgrades() - before);
            Plugin.Logger.Msg($"Upgrades: {bought} bought");
            return bought;
        }
        catch (Exception e)
        {
            Plugin.Logger.Error($"Buying upgrades failed: {e}");
            return 0;
        }
    }

    private static int CountBoughtUpgrades()
    {
        var inv = PlayerInventory.instance;
        if (inv == null || inv.upgrades == null) return 0;
        int n = 0;
        foreach (var u in inv.upgrades)
            if (u != null && u.bought) n++;
        return n;
    }

    /// <summary>Buys Max of one equipment with the game's own buy action. Returns levels bought.</summary>
    private static int BuyMax(Power power)
    {
        var shop = ShopManager.instance;
        if (shop == null || power == null) return 0;

        int before = power.level;
        int previousStack = shop.stackSelected;
        try
        {
            shop.stackSelected = StackMax;
            shop.BuyPower(power);
        }
        finally
        {
            shop.stackSelected = previousStack;
        }
        return power.level - before;
    }

    private static string Name(Power p)
    {
        try
        {
            var n = p.localizedName;
            return string.IsNullOrEmpty(n) ? p.name : n;
        }
        catch { return p.name; }
    }

    private static void Notify(string msg)
    {
        try { Plugin.ModHelperInstance?.ShowNotification(msg, false); }
        catch { /* notification is optional */ }
    }
}
