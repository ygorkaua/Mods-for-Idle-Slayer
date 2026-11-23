using IdleSlayerMods.Common.Config;
using MelonLoader;
using UnityEngine;

namespace AutoJumpMod;

internal sealed class ConfigFile : BaseConfig
{
	internal MelonPreferences_Entry<bool> UseAutoJump;

	internal MelonPreferences_Entry<KeyCode> AutoJumpToggleKey;

	public ConfigFile(string configName)
		: base(configName)
	{
	}

	protected override void SetBindings()
	{
		UseAutoJump = Bind("UseAutoJump", defaultValue: false, "Turn Auto Jump on and off");
		AutoJumpToggleKey = Bind("Auto Jump Toggle Key", "AutoJumpToggleKey", KeyCode.Z, "The key bind for toggling auto jump");
	}
}
