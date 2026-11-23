using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using UnityEngine.UI;

namespace Enhanced_Quests;

public class Enhanced_Quests : MonoBehaviour
{
    public static Enhanced_Quests Instance { get; private set; }

    private const float ClaimCheckInterval = 1f;      // seconds between auto-claim scans
    private const float RegenCheckInterval = 5f;      // seconds between "all quests done?" checks
    private const float RegenRetryCooldown = 60f;     // if a regenerate produced nothing, wait before retrying

    private float _nextClaimCheck;
    private float _nextRegenCheck;
    private float _dailyRetryAfter;
    private float _weeklyRetryAfter;

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        CheckToAllowQuestReroll();
        // First check shortly after load, once the save is in place.
        _nextRegenCheck = Time.unscaledTime + 2f;
    }

    public void Update()
    {
        float now = Time.unscaledTime;

        if (now >= _nextRegenCheck)
        {
            _nextRegenCheck = now + RegenCheckInterval;
            SafeRun(CheckToRegenerate, nameof(CheckToRegenerate));
        }

        if (now >= _nextClaimCheck && Plugin.Settings.AutoClaimQuests.Value && GameState.IsRunner())
        {
            _nextClaimCheck = now + ClaimCheckInterval;
            SafeRun(CheckQuestsForClaims, nameof(CheckQuestsForClaims));
        }
    }

    /// <summary>Ask for a regenerate check soon (e.g. right after a quest was claimed).</summary>
    public void ScheduleRegenerateCheck(float delay = 0.5f)
    {
        _nextRegenCheck = Mathf.Min(_nextRegenCheck, Time.unscaledTime + delay);
    }

    //////////////////////////////////////////////////////////////// Quests

    private static PlayerInventory Inventory => PlayerInventory.instance;

    private void CheckQuestsForClaims()
    {
        var inv = Inventory;
        if (inv == null) return;

        if (inv.quests != null)
            foreach (var quest in inv.quests)
                TryClaim(quest, isActive: true);

        if (inv.dailyQuests != null)
            foreach (var quest in inv.dailyQuests)
                if (quest != null) TryClaim(quest, quest.active);

        if (inv.weeklyQuests != null)
            foreach (var quest in inv.weeklyQuests)
                if (quest != null) TryClaim(quest, quest.active);
    }

    private static void TryClaim(Quest quest, bool isActive)
    {
        if (quest == null || !isActive || quest.isClaimed) return;
        // Same conditions the quest list uses to show a quest with a claim button.
        if (!quest.CanBeCompletedOrClaimed()) return;
        if (!quest.CanBeClaimed()) return;

        quest.Claim();
    }

    public void CheckToRegenerate()
    {
        var inv = Inventory;
        var craftables = CraftableItems.list;
        if (inv == null || craftables == null) return;

        float now = Time.unscaledTime;

        if (Plugin.Settings.ResetDailies.Value && now >= _dailyRetryAfter
            && craftables.DailyQuests != null && craftables.DailyQuests.IsActive()
            && inv.dailyQuests != null && inv.dailyQuests.Length > 0
            && DailyQuestsManager.instance != null)
        {
            if (CountRemaining(inv.dailyQuests) == 0)
            {
                Plugin.Logger.Msg("All daily quests done: regenerating dailies.");
                DailyQuestsManager.instance.RegenerateDailys();
                if (CountRemaining(inv.dailyQuests) == 0)
                    _dailyRetryAfter = now + RegenRetryCooldown;
            }

            TryGrantExtraDailies();
        }

        if (Plugin.Settings.ResetWeeklies.Value && now >= _weeklyRetryAfter
            && craftables.WeeklyQuests != null && craftables.WeeklyQuests.IsActive()
            && inv.weeklyQuests != null && inv.weeklyQuests.Length > 0
            && WeeklyQuestsManager.instance != null)
        {
            if (CountRemaining(inv.weeklyQuests) == 0)
            {
                Plugin.Logger.Msg("All weekly quests done: regenerating weeklies.");
                WeeklyQuestsManager.instance.RegenerateWeeklies();
                if (CountRemaining(inv.weeklyQuests) == 0)
                    _weeklyRetryAfter = now + RegenRetryCooldown;
            }

            TryGrantExtraWeeklies();
        }
    }

    /// <summary>
    /// Same reward as pressing the "+2" button on the Daily Quests header.
    /// The game's "granted" flag is only reset inside DailyQuestsManager.RegenerateDailys
    /// (via ExtraDailyQuests.Reset), so it reliably means "already used for this batch".
    /// </summary>
    internal static void TryGrantExtraDailies()
    {
        if (!Plugin.Settings.AutoExtraDailies.Value) return;
        var extra = ExtraDailyQuests.instance;
        var inv = Inventory;
        if (extra == null || extra.granted || inv?.dailyQuests == null) return;
        if (CountRemaining(inv.dailyQuests) == 0) return; // no batch to extend

        Plugin.Logger.Msg($"Granting +{extra.extraQuestsToAdd} extra daily quests.");
        extra.RewardForShowing();
        extra.granted = true;
    }

    /// <summary>Same as above for the "+1" button on the Weekly Quests header.</summary>
    internal static void TryGrantExtraWeeklies()
    {
        if (!Plugin.Settings.AutoExtraWeeklies.Value) return;
        var extra = ExtraWeeklyQuests.instance;
        var inv = Inventory;
        if (extra == null || extra.granted || inv?.weeklyQuests == null) return;
        if (CountRemaining(inv.weeklyQuests) == 0) return;

        Plugin.Logger.Msg($"Granting +{extra.extraQuestsToAdd} extra weekly quests.");
        extra.RewardForShowing();
        extra.granted = true;
    }

    /// <summary>Active quests that still haven't been claimed.</summary>
    private static int CountRemaining(Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<DailyQuest> quests)
    {
        int count = 0;
        foreach (var q in quests)
            if (q != null && q.active && !q.isClaimed) count++;
        return count;
    }

    private static int CountRemaining(Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<WeeklyQuest> quests)
    {
        int count = 0;
        foreach (var q in quests)
            if (q != null && q.active && !q.isClaimed) count++;
        return count;
    }

    //////////////////////////////////////////////////////////////// Portal / Reroll

    public void CheckToResetPortal()
    {
        var portal = PortalButton.instance;
        if (portal != null && portal.currentCd > 0 && Plugin.Settings.ResetPortal.Value)
            portal.currentCd = 0;
    }

    public void CheckToAllowQuestReroll()
    {
        if (!Plugin.Settings.ResetReroll.Value) return;

        if (DailyQuestReroll.instance != null) DailyQuestReroll.instance.rerollEnabled = true;
        if (WeeklyQuestReroll.instance != null) WeeklyQuestReroll.instance.rerollEnabled = true;
    }

    //////////////////////////////////////////////////////////////// Helpers

    private static readonly HashSet<string> _loggedErrors = new();

    private static void SafeRun(Action action, string name)
    {
        try { action(); }
        catch (Exception e)
        {
            // Log each distinct error once instead of spamming every frame.
            if (_loggedErrors.Add(name + e.GetType().Name + e.Message))
                Plugin.Logger.Error($"{name} failed: {e}");
        }
    }

    //////////////////////////////////////////////////////////////// Harmony patches

    [HarmonyPatch(typeof(DailyQuest), "Claim")]
    public static class Patch_DailyQuestClaim
    {
        static void Postfix() => Instance?.ScheduleRegenerateCheck();
    }

    [HarmonyPatch(typeof(WeeklyQuest), "Claim")]
    public static class Patch_WeeklyQuestClaim
    {
        static void Postfix() => Instance?.ScheduleRegenerateCheck();
    }

    [HarmonyPatch(typeof(DailyQuestsManager), "RegenerateDailys")]
    public static class Patch_RegenerateDailys
    {
        static void Postfix() => SafeRun(TryGrantExtraDailies, nameof(TryGrantExtraDailies));
    }

    [HarmonyPatch(typeof(WeeklyQuestsManager), "RegenerateWeeklies")]
    public static class Patch_RegenerateWeeklies
    {
        static void Postfix() => SafeRun(TryGrantExtraWeeklies, nameof(TryGrantExtraWeeklies));
    }

    [HarmonyPatch(typeof(DailyQuestReroll), "RewardForShowing")]
    public static class Patch_DailyQuestRerollReward
    {
        static void Postfix() => Instance?.CheckToAllowQuestReroll();
    }

    [HarmonyPatch(typeof(WeeklyQuestReroll), "RewardForShowing")]
    public static class Patch_WeeklyQuestRerollReward
    {
        static void Postfix() => Instance?.CheckToAllowQuestReroll();
    }

    [HarmonyPatch(typeof(Button), "Press")]
    public static class Patch_PortalButton
    {
        static void Postfix(Button __instance)
        {
            if (__instance == null) return;
            if (__instance.name != "Portal Button" && __instance.name != "Portal Button(Clone)") return;
            Instance?.CheckToResetPortal();
        }
    }
}
