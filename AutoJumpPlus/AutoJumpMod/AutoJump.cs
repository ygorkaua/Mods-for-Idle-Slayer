using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoJumpMod;

public class AutoJump : MonoBehaviour
{
	[HarmonyPatch(typeof(RandomBox), "OnObjectSpawn")]
	public class Patch_RandomBox_OnObjectSpawn
	{
		[HarmonyPostfix]
		public static void Postfix(RandomBox __instance)
		{
			if (!(__instance == null))
			{
				if (GameState.IsBonus() && Instance.BootsPurchased)
				{
					Instance.LockBoots();
				}
				if (Instance._bonusSection == 3)
				{
					Instance._bonusSection3Completed = true;
				}
			}
		}
	}

	[HarmonyPatch(typeof(Arrow), "Awake")]
	private static class Patch_Arrow_Awake
	{
		private static void Postfix(Arrow __instance)
		{
			if (OriginalArrowSpeed < 0f)
			{
				OriginalArrowSpeed = __instance.speed;
				OriginalElectroSpeed = __instance.electroShotSpeed;
				NewArrowSpeed = __instance.speed * 1.5f;
				NewElectroSpeed = __instance.electroShotSpeed * 1.5f;
				Instance.ChangeArrowSpeed(__instance);
			}
		}
	}

	[HarmonyPatch(typeof(Arrow), "OnObjectSpawnOverride")]
	public class Patch_Projectile_OnObjectSpawn
	{
		[HarmonyPostfix]
		public static void Postfix(Arrow __instance)
		{
			if ((bool)__instance)
			{
				if (NewArrowSpeed > 0f)
				{
					Instance.ChangeArrowSpeed(__instance);
				}
				_activeArrows.Add(__instance);
			}
		}
	}

	[HarmonyPatch(typeof(RandomEvent), "Activate")]
	public class Patch_RandomEvent_Activate
	{
		[HarmonyPostfix]
		public static void Postfix(RandomEvent __instance)
		{
			if (!(__instance == null) && IsCustomBuild && __instance.name == "Dual Randomness")
			{
				Instance?.RegisterDualEvent();
			}
		}
	}

	[HarmonyPatch(typeof(Horde), "OnEventStart")]
	public class HordeEventStartPatch
	{
		[HarmonyPostfix]
		public static void OnHordeEventStart()
		{
			if (IsCustomBuild && Instance._autoJump && Instance._shootingDisabled)
			{
				Instance._autoJump = false;
				Instance._hordeEventPause = true;
			}
		}
	}

	[HarmonyPatch(typeof(Horde), "OnEventEnd")]
	public class HordeEventEndPatch
	{
		[HarmonyPostfix]
		public static void OnHordeEventEnd()
		{
			if (IsCustomBuild && Instance._hordeEventPause)
			{
				Instance._autoJump = true;
				Instance._hordeEventPause = false;
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003CAttackGiant_003Ed__88 : System.Collections.Generic.IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AutoJump _003C_003E4__this;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CAttackGiant_003Ed__88(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			AutoJump autoJump = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				autoJump._jumpPanel.OnPointerDown(autoJump._jumpSpot);
				autoJump._jumpPanel.OnPointerUp(autoJump._jumpSpot);
				autoJump._isAttacking = false;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CLargeSingleJump_003Ed__85 : System.Collections.Generic.IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AutoJump _003C_003E4__this;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CLargeSingleJump_003Ed__85(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			AutoJump autoJump = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				autoJump._jumpPanel.OnPointerDown(autoJump._jumpSpot);
				_003C_003E2__current = new WaitForSeconds(0.15f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				autoJump._jumpPanel.OnPointerUp(autoJump._jumpSpot);
				autoJump._isJumping = false;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CPostThresholdDelay_003Ed__116 : System.Collections.Generic.IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AutoJump _003C_003E4__this;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CPostThresholdDelay_003Ed__116(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			AutoJump autoJump = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSeconds(autoJump.ResetTimer);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				autoJump._DualPaused = false;
				autoJump._windowStart = -1f;
				autoJump._count = 0;
				Plugin.Logger.Msg("Dual Randomness restarted");
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CShootArrows_003Ed__87 : System.Collections.Generic.IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AutoJump _003C_003E4__this;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CShootArrows_003Ed__87(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			AutoJump autoJump = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				autoJump._pm.ShootArrow();
				_003C_003E2__current = new WaitForSeconds(0.1f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				autoJump._isShooting = false;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CStage1Or2DelayAndJump_003Ed__86 : System.Collections.Generic.IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AutoJump _003C_003E4__this;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CStage1Or2DelayAndJump_003Ed__86(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			AutoJump autoJump = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSeconds(0.5f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				break;
			case 2:
				_003C_003E1__state = -1;
				break;
			}
			if (autoJump._pm.IsGrounded())
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
			}
			autoJump.ShortSingleJump();
			autoJump._isJumping = false;
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSwitchStage2_003Ed__95 : System.Collections.Generic.IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AutoJump _003C_003E4__this;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSwitchStage2_003Ed__95(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			AutoJump autoJump = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (autoJump._bonusStage2Skill.unlocked)
				{
					if (autoJump._bonusGaps2Skill.unlocked)
					{
						autoJump._bonusGaps2Skill.unlocked = false;
						_003C_003E2__current = new WaitForSeconds(0.5f);
						_003C_003E1__state = 1;
						return true;
					}
				}
				else if (!autoJump._bonusGaps2Skill.unlocked)
				{
					autoJump._bonusStage2Skill.unlocked = true;
					_003C_003E2__current = new WaitForSeconds(0.5f);
					_003C_003E1__state = 2;
					return true;
				}
				break;
			case 1:
				_003C_003E1__state = -1;
				autoJump._bonusStage2Skill.unlocked = false;
				break;
			case 2:
				_003C_003E1__state = -1;
				autoJump._bonusGaps2Skill.unlocked = true;
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private const float JumpSpotXRatio = 0.8f;

	private const string SkillBootsName = "ascension_upgrade_climbing_boots";

	private const string SkillBonusStage2Name = "map_bonus_stage_2";

	private const string SkillBonusGaps2Name = "ascension_upgrade_protect";

	private const string SkillBonusStage3Name = "map_bonus_stage_3";

	private const string SkillBonusGaps3Name = "ascension_upgrade_board_the_platforms";

	private const string SkillBowPurchasedName = "ascension_upgrade_sacred_book_of_projectiles";

	private const string SkillBowBoostName = "ascension_upgrade_stability";

	public static float OriginalArrowSpeed = -1f;

	public static float OriginalElectroSpeed = -1f;

	public static float NewArrowSpeed = -1f;

	public static float NewElectroSpeed = -1f;

	private static readonly System.Collections.Generic.List<Arrow> _activeArrows = new System.Collections.Generic.List<Arrow>();

	private bool _prevBootsUnlocked;

	private bool _bootsUnlockHandled;

	private bool _autoJump;

	private bool _isJumping;

	private bool _isShooting;

	private bool _didStage1Delay;

	private bool _didStage2Delay;

	private bool _isAttacking;

	private bool _isBreaking;

	private bool _wasClockVisibleLastFrame;

	private bool _bootsPurchased;

	private bool _bootsChanged;

	private bool dummy;

	private bool _bonusSection3Completed;

	private float dualChance;

	private int _bonusSection;

	private readonly ModFlagChecker _bscChecker = new ModFlagChecker("BonusStageCompleter.BonusStageCompleter, BonusStageCompleter", "_skipAtSpiritBoostEnabled");

	private readonly ModFlagChecker _autoBoostChecker = new ModFlagChecker("AutoBoost.AutoBoost, AutoBoost", "_windDashEnabled");

	private readonly ModFlagChecker _armoryManagerChecker = new ModFlagChecker("ArmoryManager.BreakWeapons, ArmoryManager");

	private Boost _boost;

	private JumpPanel _jumpPanel;

	private PlayerMovement _pm;

	private PointerEventData _jumpSpot;

	private RageModeManager _rageModeManager;

	private WindDash _windDash;

	private MapController _mapCtrl;

	private BonusMapController _bonusMapCtrl;

	private PlayerInventory _playerInventory;

	private WeaponsManager _weaponsManager;

	private AscensionSkill _bootsSkill;

	private AscensionSkill _bonusStage2Skill;

	private AscensionSkill _bonusGaps2Skill;

	private AscensionSkill _bonusStage3Skill;

	private AscensionSkill _bonusGaps3Skill;

	private AscensionSkill _bowPurchasedSkill;

	private AscensionSkill _bowBoostSkill;

	private RandomEvent dual;

	private float _lastCleanupTime;

	private float _lastModCheckTime;

	private bool _isBscLoadedCached;

	private bool _isAutoBoostLoadedCached;

	private bool _isArmoryManagerLoadedCached;

	private GameObject _confirmButtonGO;

	private bool _hordeEventPause;

	private bool _prevWindDashActive;

	private bool _shootingDisabled;

	private double _playerDecreaseRandomBoxCoinsChances;

	public int Threshold = 30;

	public float TriggerSeconds = 20f;

	public float ResetTimer = 20f;

	private int _count;

	private float _windowStart = -1f;

	private bool _DualPaused;

	private const string DualRandomnessName = "Dual Randomness";

	public static AutoJump Instance { get; private set; }

	public bool BootsPurchased => _bootsPurchased;

	private static string EnvVarName => Encoding.UTF8.GetString(Convert.FromBase64String("SURMRVNMQVlFUl9DVVNUT01CVUlMRA=="));

	public static bool IsCustomBuild => Environment.GetEnvironmentVariable(EnvVarName) == "1";

	private void Awake()
	{
		if ((bool)Instance && Instance != this)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		Instance = this;
		_jumpPanel = JumpPanel.instance;
		_pm = PlayerMovement.instance;
		_rageModeManager = RageModeManager.instance;
		_mapCtrl = MapController.instance;
		_bonusMapCtrl = BonusMapController.instance;
		_playerInventory = PlayerInventory.instance;
		_windDash = AbilitiesManager.instance.windDash;
		_boost = AbilitiesManager.instance.boost;
		_weaponsManager = WeaponsManager.instance;
		_autoJump = Plugin.Config.UseAutoJump.Value;
		CustomAwake();
	}

	private void Start()
	{
		_ = EventSystem.current == null;
		_jumpSpot = new PointerEventData(EventSystem.current)
		{
			position = new Vector2((float)Screen.width * 0.8f, Screen.height)
		};
		InitializeSkills();
		_prevBootsUnlocked = _bootsSkill.unlocked;
		Il2CppReferenceArray<RandomEvent> randomEvents = RandomEventManager.instance.randomEvents;
		if (randomEvents != null)
		{
			foreach (RandomEvent item in randomEvents)
			{
				if (!(item.name != "Dual Randomness"))
				{
					dual = item;
					dualChance = item.chance;
					break;
				}
			}
		}
		CustomStart();
	}

	private void LateUpdate()
	{
		float time = Time.time;
		if (time - _lastCleanupTime > 0.5f)
		{
			_activeArrows.RemoveAll((Arrow a) => !a || a.outOfCamera);
			_lastCleanupTime = time;
		}
		if (time - _lastModCheckTime > 1f)
		{
			_isBscLoadedCached = _bscChecker.IsLoaded();
			_isAutoBoostLoadedCached = _autoBoostChecker.IsLoaded();
			_isArmoryManagerLoadedCached = _armoryManagerChecker.IsLoaded();
			_lastModCheckTime = time;
		}
		JumpToggle();
		HandleArrowSpeedOnWindDash();
		ResetStateOnRunner();
		DetectClockFlips();
		HandleBootsLogic();
		HandleAutoJump();
		MiniArmoryManager();
		CustomBuild();
	}

	private void HandleArrowSpeedOnWindDash()
	{
		bool flag = IsWindDashEnabled() && _windDash.GetCooldown() < 1f;
		if (flag && !_prevWindDashActive)
		{
			foreach (Arrow activeArrow in _activeArrows)
			{
				ChangeArrowSpeed(activeArrow);
			}
		}
		_prevWindDashActive = flag;
	}

	public bool IsBscLoaded()
	{
		return _isBscLoadedCached;
	}

	public bool ShouldSkipAtSpiritBoost()
	{
		return _bscChecker.GetBoolFlag();
	}

	public bool IsAutoBoostLoaded()
	{
		return _isAutoBoostLoadedCached;
	}

	public bool IsWindDashEnabled()
	{
		return _autoBoostChecker.GetBoolFlag();
	}

	public bool IsArmoryManagerLoaded()
	{
		return _isArmoryManagerLoadedCached;
	}

	private void OnDestroy()
	{
		Instance = ((Instance == this) ? null : Instance);
	}

	private void InitializeSkills()
	{
		foreach (AscensionSkill ascensionSkill in _playerInventory.ascensionSkills)
		{
			switch (ascensionSkill.name)
			{
			case "ascension_upgrade_climbing_boots":
				_bootsSkill = ascensionSkill;
				if (_bootsSkill.unlocked)
				{
					_bootsPurchased = true;
				}
				break;
			case "map_bonus_stage_2":
				_bonusStage2Skill = ascensionSkill;
				break;
			case "ascension_upgrade_protect":
				_bonusGaps2Skill = ascensionSkill;
				break;
			case "map_bonus_stage_3":
				_bonusStage3Skill = ascensionSkill;
				break;
			case "ascension_upgrade_board_the_platforms":
				_bonusGaps3Skill = ascensionSkill;
				break;
			case "ascension_upgrade_sacred_book_of_projectiles":
				_bowPurchasedSkill = ascensionSkill;
				break;
			case "ascension_upgrade_stability":
				_bowBoostSkill = ascensionSkill;
				break;
			}
		}
	}

	private void ResetStateOnRunner()
	{
		if (GameState.IsRunner())
		{
			_bonusSection = 0;
			_bonusSection3Completed = false;
		}
	}

	private void DetectClockFlips()
	{
		bool showCurrentTime = _bonusMapCtrl.showCurrentTime;
		BonusMap bonusMap = _mapCtrl.CurrentBonusMap();
		bool num = GameState.IsBonus() && bonusMap.id == Maps.list.BonusStage.id;
		bool flag = GameState.IsBonus() && bonusMap.id == Maps.list.BonusStage3.id;
		if ((num || flag) && !_wasClockVisibleLastFrame && showCurrentTime && _bonusMapCtrl.currentSectionIndex == _bonusSection)
		{
			_bonusSection++;
		}
		_wasClockVisibleLastFrame = showCurrentTime;
	}

	private void HandleBootsLogic()
	{
		if (!_bootsUnlockHandled)
		{
			bool unlocked = _bootsSkill.unlocked;
			if (!_prevBootsUnlocked && unlocked)
			{
				_bootsUnlockHandled = true;
				_bootsPurchased = true;
			}
			_prevBootsUnlocked = unlocked;
		}
		bool flag = GameState.IsBonus() && _mapCtrl.CurrentBonusMap() == Maps.list.BonusStage3;
		bool flag2 = GameState.IsBonus() && _mapCtrl.CurrentBonusMap() == Maps.list.BonusStage2;
		if (!_autoJump || !AutoBonusLevel())
		{
			return;
		}
		if (flag && _bootsPurchased)
		{
			if (_bonusSection == 3 && !_bonusSection3Completed)
			{
				_bootsSkill.unlocked = _pm.isMoving;
			}
			else if (_bonusMapCtrl.showCurrentTime)
			{
				_bootsSkill.unlocked = true;
			}
			if ((bool)dual)
			{
				dual.chance = ((_bonusSection == 2) ? 0f : dualChance);
			}
			_bootsChanged = true;
		}
		else if (flag2 && _bootsPurchased)
		{
			_bootsSkill.unlocked = _bonusMapCtrl.showCurrentTime;
			_bootsChanged = true;
		}
		else if (_bootsChanged && GameState.IsRunner() && _bootsPurchased)
		{
			_bootsChanged = false;
			_bootsSkill.unlocked = true;
		}
	}

	private void JumpToggle()
	{
		if (Input.GetKeyDown(Plugin.Config.AutoJumpToggleKey.Value))
		{
			_autoJump = !_autoJump;
			Plugin.Logger.Msg("AutoJump is: " + (_autoJump ? "ON" : "OFF") + " ");
			Plugin.ModHelperInstance.ShowNotification(_autoJump ? "Auto Jump enabled!" : "Auto Jump disabled!", _autoJump);
			Plugin.Config.UseAutoJump.Value = _autoJump;
		}
	}

	private void HandleAutoJump()
	{
		if (!_autoJump)
		{
			return;
		}
		BonusMap bonusMap = _mapCtrl.CurrentBonusMap();
		bool flag = GameState.IsBonus() && bonusMap.id == Maps.list.BonusStage.id;
		bool flag2 = GameState.IsBonus() && bonusMap.id == Maps.list.BonusStage3.id;
		if (!flag2 || _bonusSection != 2)
		{
			_didStage2Delay = false;
		}
		if (!flag2 || _bonusSection != 1)
		{
			_didStage1Delay = false;
		}
		if (GameState.IsBonus() && !_isJumping && AutoBonusLevel())
		{
			_isJumping = true;
			if (flag && _pm.IsGrounded() && _bonusSection == 1 && _bonusMapCtrl.showCurrentTime)
			{
				MelonCoroutines.Start(LargeSingleJump());
			}
			else if (flag2 && _bonusSection == 1 && !_didStage1Delay)
			{
				_didStage1Delay = true;
				MelonCoroutines.Start(Stage1Or2DelayAndJump());
			}
			else if (flag2 && _bonusSection == 2 && !_didStage2Delay)
			{
				_didStage2Delay = true;
				MelonCoroutines.Start(Stage1Or2DelayAndJump());
			}
			else
			{
				ShortSingleJump();
			}
		}
		else if (CanJumpRunner())
		{
			_isJumping = true;
			ShortSingleJump();
		}
		if (GameState.IsRunner() && CanShoot())
		{
			if (_pm.IsGrounded())
			{
				_pm.ShootArrow();
			}
			if (!_isShooting)
			{
				_isShooting = true;
				MelonCoroutines.Start(ShootArrows());
			}
		}
		if (GameState.IsRunner() && !_pm.isMoving && !_isAttacking)
		{
			_isAttacking = true;
			MelonCoroutines.Start(AttackGiant());
		}
	}

	private void MiniArmoryManager()
	{
		if (!IsArmoryManagerLoaded() && !_isBreaking && Time.unscaledTime >= _nextBreakAttempt && GameState.IsRunner() && (bool)_weaponsManager && !_weaponsManager.HasFreeSlot())
		{
			MelonCoroutines.Start(BreakLastWeapon());
		}
	}

	private float _nextBreakAttempt;

	private bool _breakLogged;

	/// <summary>
	/// Armory full while running -> break one weapon so new drops are not lost.
	/// Rewritten for the current game version: WeaponsManager.currentItems (a List) was
	/// replaced by currentItemsByUid, so the old code threw MissingMethodException every frame.
	/// Never breaks equipped, loadout or key items; prefers non-excellent, lowest level.
	/// </summary>
	private IEnumerator BreakLastWeapon()
	{
		_isBreaking = true;
		bool ok = false;
		try
		{
			WeaponItemInventory target = null;
			try
			{
				if (_weaponsManager == null || _weaponsManager.HasFreeSlot() || IsPopupOpen())
				{
					yield break;
				}
				target = PickWeaponToBreak();
			}
			catch (Exception e)
			{
				LogBreakError("pick weapon", e);
			}
			if (target == null)
			{
				if (!_breakLogged)
				{
					_breakLogged = true;
					Plugin.Logger.Msg("Armory is full but every weapon is equipped/in a loadout: nothing to break.");
				}
				yield break;
			}
			uint uid = target.uid;
			string name = target.item != null ? target.item.name : "?";
			int level = target.currentLevel;
			try
			{
				_weaponsManager.BreakPopup(target);
			}
			catch (Exception e)
			{
				LogBreakError("open break popup", e);
				yield break;
			}
			Button btn = null;
			float deadline = Time.unscaledTime + 2f;
			while (Time.unscaledTime < deadline)
			{
				GameObject go = GameObject.Find("UIManager/Popup/Overlay/Panel/Buttons/Confirm Button");
				btn = go ? go.GetComponent<Button>() : null;
				if ((bool)btn && btn.isActiveAndEnabled && btn.interactable)
				{
					break;
				}
				btn = null;
				yield return null;
			}
			if (btn == null)
			{
				yield break;
			}
			btn.onClick.Invoke();
			deadline = Time.unscaledTime + 3f;
			while (Time.unscaledTime < deadline && _weaponsManager.TryGetItem(uid, out _))
			{
				yield return null;
			}
			ok = !_weaponsManager.TryGetItem(uid, out _);
			if (ok)
			{
				_breakLogged = false;
				Plugin.Logger.Msg($"Armory full: broke '{name}' (lvl {level}).");
			}
		}
		finally
		{
			_isBreaking = false;
			_nextBreakAttempt = Time.unscaledTime + (ok ? 0.5f : 30f);
		}
	}

	private WeaponItemInventory PickWeaponToBreak()
	{
		WeaponItemInventory best = null;
		foreach (WeaponItemInventory item in _weaponsManager.currentItemsByUid.Values)
		{
			if (item == null || _weaponsManager.IsKey(item) || _weaponsManager.IsItemInAnyLoadout(item.uid)
				|| _weaponsManager.IsEquippedEffective(item, false) || _weaponsManager.IsEquippedForGameplay(item, false))
			{
				continue;
			}
			if (best == null || IsWorse(item, best))
			{
				best = item;
			}
		}
		return best;
	}

	private static bool IsWorse(WeaponItemInventory a, WeaponItemInventory b)
	{
		if (a.isExcellent != b.isExcellent)
		{
			return !a.isExcellent;
		}
		if (a.currentLevel != b.currentLevel)
		{
			return a.currentLevel < b.currentLevel;
		}
		return a.slot > b.slot;
	}

	private static bool IsPopupOpen()
	{
		GameObject go = GameObject.Find("UIManager/Popup/Overlay/Panel/Buttons/Confirm Button");
		return go != null && go.activeInHierarchy;
	}

	private readonly System.Collections.Generic.HashSet<string> _breakErrors = new System.Collections.Generic.HashSet<string>();

	private void LogBreakError(string what, Exception e)
	{
		if (_breakErrors.Add(what + e.GetType().Name + e.Message))
		{
			Plugin.Logger.Error($"Break weapon: failed to {what}: {e}");
		}
	}

	private bool CanShoot()
	{
		if (_bowPurchasedSkill.unlocked && (_bowBoostSkill.unlocked || !_boost.IsActive()) && !_pm.bowDisabled && !_windDash.IsActive() && _pm.isMoving && _rageModeManager.currentState == RageModeManager.RageModeStates.NotActive)
		{
			return !IsShootingDisabled();
		}
		return false;
	}

	private bool AutoBonusLevel()
	{
		if (IsBscLoaded())
		{
			if (!ShouldSkipAtSpiritBoost())
			{
				return !_bonusMapCtrl.spiritBoostEnabled;
			}
			return true;
		}
		return false;
	}

	private bool CanJumpRunner()
	{
		if (GameState.IsRunner() && _pm.IsGrounded() && _pm.isMoving)
		{
			return !_isJumping;
		}
		return false;
	}

	private void ShortSingleJump()
	{
		_jumpPanel.OnPointerDown(_jumpSpot);
		_jumpPanel.OnPointerUp(_jumpSpot);
		_isJumping = false;
	}

	[IteratorStateMachine(typeof(_003CLargeSingleJump_003Ed__85))]
	private IEnumerator LargeSingleJump()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CLargeSingleJump_003Ed__85(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CStage1Or2DelayAndJump_003Ed__86))]
	private IEnumerator Stage1Or2DelayAndJump()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CStage1Or2DelayAndJump_003Ed__86(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CShootArrows_003Ed__87))]
	private IEnumerator ShootArrows()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CShootArrows_003Ed__87(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CAttackGiant_003Ed__88))]
	private IEnumerator AttackGiant()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CAttackGiant_003Ed__88(0)
		{
			_003C_003E4__this = this
		};
	}

	private void LockBoots()
	{
		_bootsSkill.unlocked = false;
	}

	private void ChangeArrowSpeed(Arrow arrow)
	{
		if (IsWindDashEnabled() && _windDash.GetCooldown() < 1f)
		{
			arrow.speed = NewArrowSpeed;
			arrow.electroShotSpeed = NewElectroSpeed;
		}
		else
		{
			arrow.speed = OriginalArrowSpeed;
			arrow.electroShotSpeed = OriginalElectroSpeed;
		}
	}

	private void HandleMapKeys()
	{
		if (Input.GetKeyDown(KeyCode.F1))
		{
			_mapCtrl.ChangeMap(_mapCtrl.CurrentBonusMap());
		}
		if (Input.GetKeyDown(KeyCode.F2) && (bool)_bonusStage2Skill && (bool)_bonusGaps2Skill)
		{
			MelonCoroutines.Start(SwitchStage2());
		}
		if (Input.GetKeyDown(KeyCode.F3) && (bool)_bootsSkill && !_bonusStage3Skill.unlocked && !_bonusGaps3Skill.unlocked)
		{
			_bootsSkill.unlocked = !_bootsSkill.unlocked;
			_bootsPurchased = _bootsSkill.unlocked;
		}
		if (Input.GetKeyDown(KeyCode.F4) && (bool)_bonusStage3Skill && _bootsSkill.unlocked && !_bonusGaps3Skill.unlocked)
		{
			_bonusStage3Skill.unlocked = !_bonusStage3Skill.unlocked;
		}
		if (Input.GetKeyDown(KeyCode.F5) && (bool)_bonusGaps3Skill && _bootsSkill.unlocked && _bonusStage3Skill.unlocked)
		{
			_bonusGaps3Skill.unlocked = !_bonusGaps3Skill.unlocked;
		}
		if (Input.GetKeyDown(KeyCode.F6))
		{
			_mapCtrl.ChangeMap(_mapCtrl.lastRunnerMap);
		}
		if (Input.GetKeyDown(KeyCode.F7))
		{
			dummy = IsBscLoaded();
		}
		if (Input.GetKeyDown(KeyCode.F8))
		{
			_bonusMapCtrl.spiritBoostEnabled = true;
			_mapCtrl.ChangeMap(_mapCtrl.CurrentBonusMap());
		}
		if (Input.GetKeyDown(KeyCode.F9))
		{
			_bowPurchasedSkill.unlocked = !_bowPurchasedSkill.unlocked;
		}
		if (Input.GetKeyDown(KeyCode.F10))
		{
			_bowBoostSkill.unlocked = !_bowBoostSkill.unlocked;
		}
	}

	[IteratorStateMachine(typeof(_003CSwitchStage2_003Ed__95))]
	private IEnumerator SwitchStage2()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSwitchStage2_003Ed__95(0)
		{
			_003C_003E4__this = this
		};
	}

	private bool IsShootingDisabled()
	{
		return _shootingDisabled & IsCustomBuild;
	}

	private void CustomAwake()
	{
		if (IsCustomBuild)
		{
			_shootingDisabled = false;
			_hordeEventPause = false;
		}
	}

	private void CustomStart()
	{
		if (IsCustomBuild)
		{
			_playerDecreaseRandomBoxCoinsChances = _playerInventory.decreaseRandomBoxCoinsChances;
		}
	}

	private void CustomBuild()
	{
		if (IsCustomBuild)
		{
			HandleShootingToggle();
			if (GameState.IsRunner() && !_pm.isMoving && !_isAttacking && _hordeEventPause)
			{
				_isAttacking = true;
				MelonCoroutines.Start(AttackGiant());
			}
		}
	}

	private void HandleShootingToggle()
	{
		if (Input.GetKeyDown(KeyCode.X))
		{
			_shootingDisabled = !_shootingDisabled;
			Plugin.Logger.Msg("Shooting is: " + (_shootingDisabled ? "OFF" : "ON") + " ");
			Plugin.ModHelperInstance.ShowNotification(_shootingDisabled ? "Shooting disabled!" : "Shooting enabled!", _shootingDisabled);
			if (!_shootingDisabled && _hordeEventPause)
			{
				_autoJump = true;
				_hordeEventPause = false;
			}
		}
	}

	public void RegisterDualEvent()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!_DualPaused)
		{
			if (_windowStart < 0f || realtimeSinceStartup - _windowStart > TriggerSeconds)
			{
				_windowStart = realtimeSinceStartup;
				_count = 0;
			}
			_count++;
			if (_count >= Threshold)
			{
				_DualPaused = true;
				Plugin.Logger.Msg("Dual Randomness paused");
				MelonCoroutines.Start(PostThresholdDelay());
			}
		}
	}

	[IteratorStateMachine(typeof(_003CPostThresholdDelay_003Ed__116))]
	private IEnumerator PostThresholdDelay()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CPostThresholdDelay_003Ed__116(0)
		{
			_003C_003E4__this = this
		};
	}
}
