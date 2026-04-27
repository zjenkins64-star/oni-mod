# GasPressureEqualizer

Oxygen Not Included mod that adds a gas-balancing vent. Targets vanilla + Spaced Out (`VANILLA_ID,EXPANSION1_ID`), API v2.

## Build

- `net48`, builds against ONI's `Managed/` DLLs (Harmony 2, Assembly-CSharp, UnityEngine).
- Game install referenced from `.csproj`: `D:\Games\SteamLibrary\steamapps\common\OxygenNotIncluded\`
- Post-build target `CopyToONIModFolder` copies the DLL + `mod_info.yaml` into `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\GasPressureEqualizer\`. No manual install step.

## Modding model in this repo

ONI mods are Harmony patches against `Assembly-CSharp`. Pattern here:

- `*Config.cs` defines the building (`CreateBuildingDef`, `ConfigureBuildingTemplate`, `DoPostConfigureComplete`).
- A component class (`GasPressureEqualizerVent.cs`) holds runtime behavior, attached in `DoPostConfigure*`.
- `GasPressureEqualizerPatches.cs` registers the building via `[HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]` + `ModUtil.AddBuildingToPlanScreen(...)`.

No `UserMod2` class — the mod relies on the default loader's `PatchAll`. If patches ever stop firing, add a `KMod.UserMod2` subclass that calls `base.OnLoad(harmony)`.

## Reference docs

Local cache of community modding docs: [docs/oni-modding/](docs/oni-modding/) (read these before designing new features).

- [docs/oni-modding/01-introduction.md](docs/oni-modding/01-introduction.md) — setup, decompilation, deploy paths, log location
- [docs/oni-modding/02-mod-structure.md](docs/oni-modding/02-mod-structure.md) — `mod_info.yaml` / `mod.yaml` / `UserMod2`
- [docs/oni-modding/03-animations.md](docs/oni-modding/03-animations.md) — kanim format, Spriter workflow
- [docs/oni-modding/04-publishing.md](docs/oni-modding/04-publishing.md) — local install + Steam Workshop
- [docs/oni-modding/05-api-changes.md](docs/oni-modding/05-api-changes.md) — API v1 → v2 changes
- [docs/oni-modding/06-plib.md](docs/oni-modding/06-plib.md) — PLib utility library (consider for options UI / multi-building)

When game-internal behavior is unclear, decompile `Assembly-CSharp.dll` (dnSpy / dotPeek) — it's the source of truth. Game log: `%USERPROFILE%\AppData\LocalLow\Klei\Oxygen Not Included\player.log`.

External hubs: [Klei Forums — Mods and Tools](https://forums.kleientertainment.com/forums/forum/204-oxygen-not-included-mods-and-tools/), [peterhaneve/ONIMods](https://github.com/peterhaneve/ONIMods).
