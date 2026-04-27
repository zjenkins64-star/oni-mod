# PLib — Peter Han's Mod Utility Library

Source: https://github.com/peterhaneve/ONIMods (PLib subdirectory)

The de facto standard utility library for ONI mods. Saves reimplementing options UI, custom lighting, building registration, etc. Available as a [NuGet package](https://www.nuget.org/packages/PLib/).

## Why use it

- Mod options UI that matches the game's style
- Cross-mod safe Harmony patching (`PPatchManager`)
- Custom light shapes for `Light2D`
- Simplified building registration (no `GeneratedBuildings` patches)
- Action / keybinding registration with rebind support

## Modules

| Module | Purpose | Depends on |
|---|---|---|
| `PLib.Core` | Patch manager, reflection helpers, game utilities | — (required by all) |
| `PLib.UI` | Custom UI controls + side screens matching game style | Core |
| `PLib.Options` | Read/write JSON config + in-game mod settings menu | UI, Core |
| `PLib.Actions` | User-rebindable input actions | Core |
| `PLib.Lighting` | Custom light shapes / falloff for `Light2D` | Core |
| `PLib.Buildings` | Register buildings without writing `GeneratedBuildings` patches | Core |
| `PLib.Database` | Db ops, translation/localization, codex entries | Core |
| `PLib.AVC` | Automatic version checking | Core |

PLib 4.0+ is **not** backwards-compatible with earlier versions — uses Harmony 2.

## Integration

In `.csproj`, enable:
```xml
<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
```
(this project already has it).

Use **ILRepack** or **ILMerge** to merge PLib into your mod DLL so you don't need to ship it separately.

## Initialization

```csharp
public sealed class ModLoad : KMod.UserMod2
{
    public override void OnLoad(Harmony harmony)
    {
        PUtil.InitLibrary(false); // true logs your AssemblyFileVersion
        base.OnLoad(harmony);
    }
}
```

## Options example

```csharp
[JsonObject(MemberSerialization.OptIn)]
[ModInfo("https://www.github.com/yourmod")]
public class TestModSettings
{
    [Option("Wattage", "Watts before explosion")]
    [Limit(1, 50000)]
    [JsonProperty]
    public float Watts { get; set; } = 10000f;
}

// In OnLoad:
new POptions().RegisterOptions(typeof(TestModSettings));
```

## Buildings

```csharp
PBuildingManager.Register(myPBuildingInstance); // in OnLoad
```

Avoids the `[HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]` boilerplate this project currently uses.

## Actions / keybindings

```csharp
PActionManager.CreateAction(string id, LocString displayName, PKeyBinding binding);
```

## Translations

`PLocalization.Register()` — drop `.po` files in `translations/` (e.g. `zh-CN.po`).

## Cross-mod components

PLib uses **forwarded components** (`PForwardedComponent`) to coordinate across mods that ship different PLib versions. Only one instance per component version runs patches; others share state via `GetSharedData`/`SetSharedData`.

---

**Recommendation for this project**: when adding mod settings or a second building, switch to PLib rather than hand-rolling. The current Harmony approach in `GasPressureEqualizerPatches.cs` is fine for one building but PLib pays off as soon as there's a second.
