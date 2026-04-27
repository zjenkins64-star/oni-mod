# Mod Structure

> Verbatim from [Cairath wiki — Mod Structure](https://github.com/Cairath/Oxygen-Not-Included-Modding/wiki/Mod-Structure). Synced 2026-04-26.

## Table of Contents
- [mod.yaml](#modyaml)
- [mod_info.yaml](#mod_infoyaml)
- [Versioning](#versioning)
- [UserMod2](#usermod2)

Aside from the code part of the mod, a few files are required for it to load and initialize correctly.

## mod.yaml
This file is not mandatory, but very highly **recommended**.
Place it in your mod's root directory: `YourModName\mod.yaml` - **one** file per mod.

As of now, three properties can be defined, and the file template looks as follows:
```yaml
title: "Your Mod Title"
description: "Something About Your Mod"
staticID: "yourSuperCoolMod"
```

* **title**: set this to your mod name in the Steam Workshop. In case of non-Steam mod installation, this is what will be displayed in game as the mod title
* **description**: currently not used in game, but perhaps will be one day!
* **staticID**: a mod's unique identifier. This needs to be globally unique, across ALL mods. Can be as simple as your mod name (with no spaces), or you can prefix it with your name. Once you publish the mod - don't change this value. This is used for mod compatibility - the game will use this ID to create and name a Harmony instance for your mod, and other mods can refer to yours by this ID.

**Note:** make sure the file is saved with UTF-8 (no signature) encoding. Visual Studio uses UTF-8 with BOM by default, which will cause an error in game.

## mod_info.yaml
This file is **mandatory**. If you are using [archived versions](#versioning), you need a file **per every version**.
Place it in your mod's root directory: `YourModName\mod.yaml`, and in the directory of each version if you have multiple.

The file template looks as follows:
```yaml
supportedContent: EXPANSION1_ID
minimumSupportedBuild: 469112
version: 1.3.42
APIVersion: 2
```

* **supportedContent**: this field defines game version for which this mod version will be loaded. It is possible to have separate mod versions for the Base Game and Spaced Out, please refer to the [versioning](#versioning) section.
  * ALL - both base game and Spaced Out
  * VANILLA_ID - only base game
  * EXPANSION1_ID - only Spaced Out
* **minimumSupportedBuild**: minimum game version for which the mod will be loaded
* **version**: an arbitrary string that will be displayed to users on the mod screen, automatically prefixed with 'v' (v1.3.42). Since this is just a text value, you can define your custom version format.
* **APIVersion**: all mods that include a .dll file (so anything aside from translations) must be set to 2 to specify you have upgraded your mod to use Harmony 2. If not set, the mod will not load.

**Note:** make sure the file is saved with UTF-8 (no signature) encoding. Visual Studio uses UTF-8 with BOM by default, which will cause an error in game.

## Versioning

In Progress. For the time being please refer to [this forum thread](https://forums.kleientertainment.com/forums/topic/126022-setting-up-mod_infoyaml/).

## UserMod2

As of [The Big Merge Update](https://forums.kleientertainment.com/forums/topic/131141-spaced-out-the-big-merge-update-469287/), a new class became available for modders: `UserMod2`.

Implementing this class is optional, and the game will default to simply calling `Harmony.PatchAll()` for you if you choose not to. However, if you wish to control patching, such as patching conditionally, or if want code to execute outside of the patching process, you will need to have an implementation.

### Implementation
Long story short, the game will scan for any classes which extend `UserMod2` (in namespace `KMod`), and, if found, instantiate it (with a few [caveats](#notes)). Your implementation class can be any name and in any namespace of your choosing, and should look something like this:

```cs
using KMod;

namespace ONIMod
{
    public class MyUserMod : UserMod2
    {
        // nothing yet, keep reading
    }
}
```

Easy enough, right? The game will discover this file and treat your mod a bit differently. If you want to customize this loading, see below.

### OnLoad Method
The most-used method on this class will be the `OnLoad` method, which is invoked when your mod DLL is loaded by the game. This is great if you want to execute code at load-time, such as code before/after patching or controlling patches conditionally. If not specified, the game will simply apply all patches for you.

Let's add a new method to our class:
```cs
public override void OnLoad(Harmony harmony)
{
    // do some stuff before patching
    Debug.Log("OnLoad: Before patches!");

    // let the game patch everything
    base.OnLoad(harmony);

    // do some stuff after patching
    Debug.Log("OnLoad: After patches!");
}
```

Don't forget `using HarmonyLib` if your editor complains about `Harmony` being unknown.

Breaking it down, it simply logs a message to the game's log, runs all patches, then logs another message. Nothing too fancy, but it gives your mod a lot of power to do things. The method is also passed a `Harmony` instance for your mod, which will use the `staticID` from your [mod.yaml](#modyaml) if you provided one.

### OnAllModsLoaded Method
Another method you can optionally implement is `OnAllModsLoaded`, which, as the name implies, is called after all mods have been loaded. This is lesser used, but still a handy tool if you're interested in what other mods the game has loaded for any reason.

It'll look something like this:
```cs
public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods)
{
    foreach (var mod in mods)
    {
        // do some stuff
        Debug.Log("found mod: " + mod.title);
    }
}
```

This is the same `Harmony` instance as in `OnLoad`, but we have a new argument: `mods`. This is a read-only list of `Mod` instances for each other mod the game has successfully loaded, allowing you to access information about the mod and do as you wish with it.

All we're doing here is logging each mod we find (including our own mod!), but you can do a lot more with it.

### Properties
There are a few properties as well you'll have access to with further information about your mod, here's a few:

```cs
public override void OnLoad(Harmony harmony)
{
    // the assembly of this UserMod
    assembly;

    // path to your mod's folder
    path;

    // the `Mod` instance for your mod
    mod;

    // info from your mod.yaml
    mod.title;
    mod.staticID;
    mod.description;

    // your mod_info.yaml
    mod.packageModInfo;
}
```

Of course, there are more, particularly on `Mod`, but these are the basics you'll probably find yourself using.

### Notes
* There can at most one implementation of `UserMod2` per DLL and it must not be `abstract` else the mod will fail to load entirely. However, it is okay if the class is non-`public`.
* The two classes `Mod` and `UserMod2` are two different classes that serve different roles, and should not be confused. `UserMod2` will exist for each DLL in a given mod, while there is only one `Mod` for each. In most cases, you'll only have one of both, but it's worth noting there can be more `UserMod2` classes for a given `Mod`, which you can find in `Mod.loaded_mod_data.userMod2Instances`.
