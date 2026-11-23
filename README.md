# Mods for Idle Slayer

Open-source [MelonLoader](https://melonwiki.xyz/) mods for [Idle Slayer](https://store.steampowered.com/app/1353300/Idle_Slayer/): new mods, plus updated versions of older community mods that broke after game updates.

Ready-to-install releases are on [Nexus Mods](https://www.nexusmods.com/idleslayer).

## Mods

| Mod | What it does | Nexus |
|---|---|---|
| [Ascending Heights Autopilot](AscendingHeightsAutopilot) | Plays Ascending Heights for you: never falls, climbs to the top and walks to the reward so the run ends by itself. | [54](https://www.nexusmods.com/idleslayer/mods/54) |
| [Break All Weapons](BreakAllWeapons) | Breaks every unequipped armory weapon in one click (F8), or automatically when the armory is almost full. Keeps equipped, loadout and key items, plus the Excellent weapons you choose. | [55](https://www.nexusmods.com/idleslayer/mods/55) |
| [Buy Max Equipment](BuyMaxEquipment) | One key (F7) buys every affordable upgrade and Max of every unlocked equipment, from the last one to the first. | [56](https://www.nexusmods.com/idleslayer/mods/56) |
| [Open All Eggs](OpenAllEggs) | Click Open on a Dragon Egg and every egg you can afford opens at once. | [57](https://www.nexusmods.com/idleslayer/mods/57) |

### Updated versions of other authors' mods

| Mod | Based on | Nexus |
|---|---|---|
| [AutoJump Plus](AutoJumpPlus) | [AutoJump](https://www.nexusmods.com/idleslayer/mods/20) by CrossTM | [58](https://www.nexusmods.com/idleslayer/mods/58) |
| [Enhanced Quests Plus](EnhancedQuestsPlus) | [Enhanced_Quests](https://www.nexusmods.com/idleslayer/mods/13) by Shokram (CrossTM's version) | [59](https://www.nexusmods.com/idleslayer/mods/59) |
| [Minion Sender Plus](MinionSenderPlus) | [Minion Sender](https://www.nexusmods.com/idleslayer/mods/11) by CrossTM | [60](https://www.nexusmods.com/idleslayer/mods/60) |
| [Perfect Chest Hunter Plus](PerfectChestHunterPlus) | [Perfect Chest Hunter](https://www.nexusmods.com/idleslayer/mods/9) by CrossTM, with the fix from [PR #11](https://github.com/Cross-TM/Mods-for-Idle-Slayer/pull/11) | [61](https://www.nexusmods.com/idleslayer/mods/61) |

Don't install a "Plus" mod together with its original.

## Installing

Easiest: install from Nexus with the [Idle Slayer Mod Manager](https://www.nexusmods.com/idleslayer/mods/33).

Manual: you need [MelonLoader 0.7](https://www.nexusmods.com/idleslayer/mods/6) and [Idle Slayer Mods Core](https://www.nexusmods.com/idleslayer/mods/3). Copy the mod's `.dll` into the Mods folder that MelonLoader loads. Each mod creates its config in `UserData/<ModName>.cfg` the first time the game starts.

## Building

Requirements: the [.NET SDK](https://dotnet.microsoft.com/download) (6.0 or newer), and the game set up with MelonLoader and Idle Slayer Mods Core (the Mod Manager does this). The projects reference the game assemblies that MelonLoader generates.

```sh
dotnet build -c Release
```

The output is `<Mod>/bin/Release/net6.0/<Mod>.dll`.

By default the projects look for those assemblies in `%LOCALAPPDATA%\IdleSlayerModManager` (see [Directory.Build.props](Directory.Build.props)). If yours are somewhere else, set `ModManagerRoot`:

```sh
dotnet build -c Release -p:ModManagerRoot="D:\IdleSlayerModManager"
# or, from WSL:
export ModManagerRoot=/mnt/c/Users/<you>/AppData/Local/IdleSlayerModManager
```

After a game update, rebuilding against the new assemblies is the quickest way to find what broke (`MissingMethodException` in the MelonLoader log usually means a method was renamed or removed).

## Credits

- **CrossTM** for AutoJump, Minion Sender and Perfect Chest Hunter, and for allowing these updated versions.
- **Shokram** for Enhanced_Quests.
- The author of [PR #11](https://github.com/Cross-TM/Mods-for-Idle-Slayer/pull/11) on CrossTM's repository, for the Perfect Chest Hunter fix.
- **Doomnik** for Idle Slayer Mods Core and the Idle Slayer Mod Manager.
- The MelonLoader team.

## License

[MIT](LICENSE) for my own code. The "Plus" mods are based on the work of the authors credited above and are shared with their permission; their original code remains theirs.
