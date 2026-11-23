using System.Diagnostics;
using HarmonyLib;
using IdleSlayerMods.Common;
using MelonLoader;

namespace AutoJumpMod;

public class Plugin : MelonMod
{
	internal static ConfigFile Config;

	internal static ModHelper ModHelperInstance;

	internal static readonly MelonLogger.Instance Logger = Melon<Plugin>.Logger;

	[Conditional("DEBUG")]
	public static void DLog(string message)
	{
		Logger.Msg(message);
	}

	public override void OnInitializeMelon()
	{
		Config = new ConfigFile("AutoJumpMod");
		Logger.Msg("Plugin AutoJumpMod is loaded!");
		ModHelper.ModHelperMounted += SetModHelperInstance;
		new HarmonyLib.Harmony("Auto Jump").PatchAll();
	}

	private static void SetModHelperInstance(ModHelper instance)
	{
		ModHelperInstance = instance;
	}

	public override void OnSceneWasLoaded(int buildIndex, string sceneName)
	{
		if (!(sceneName != "Game"))
		{
			ModUtils.RegisterComponent<AutoJump>();
		}
	}
}
