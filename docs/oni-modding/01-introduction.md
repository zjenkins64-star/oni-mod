# Introduction

> Verbatim from [Cairath wiki — Introduction](https://github.com/Cairath/Oxygen-Not-Included-Modding/wiki/Introduction). Synced 2026-04-26.

In this section you will learn how to download and configure development environment required for modding, compile your first mod, and see it in game.

The guide will focus on creating a mod that modifies the game logic, i.e. not just a world generation template. World generation will be covered in another section.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Modding Ethics](#modding-ethics)
- [Downloads](#downloads)
- [Visual Studio installation](#visual-studio-installation)
- [Decompiling the game files](#decompiling-the-game-files)
- [Your first mod](#your-first-mod)

## Prerequisites
* At least basic knowledge of C# (you *will* be required to read up on more advanced concepts, depending on the depth of your mod). **We will not teach you C#, that one is on you.**
* Understanding of [Harmony](https://github.com/pardeike/Harmony) - the library we use for patching game methods. **Reading and understanding [Harmony documentation](https://harmony.pardeike.net/articles/patching.html) is absolutely mandatory before proceeding.**
* A working development environment. This can be either Windows, Linux, or MacOS (under certain conditions). **If you opt for a non-Windows platform, help may be limited.**
  * Windows: You will need Visual Studio and .NET Framework 4.7.1, which used by the game and will need to be installed either via Visual Studio or manually.
  * Linux/MacOS: `dotnetcore` must be installed and an editor capable of working with MSBuild and C# is required (such as Visual Studio Code). You will also need the reference assemblies for .NET Framework 4.7.1, which are available on [nuget here](https://www.nuget.org/packages/Microsoft.NETFramework.ReferenceAssemblies.net471/).

## Modding Ethics
First and foremost, **don't be a jerk.** We're a small and tight knit community with most modders knowing one another.

**Keep in mind that you are dealing with Intellectual Property that does not belong to you.**
You have **absolutely no rights** to distribution of the game code, assets or any files that come from Klei. Do not upload the decompiled sources to any repository, it is illegal. For more content creation guidelines, please [read these policies by Klei](https://www.klei.com/mod-player-creation-policy).

**As such, you are not allowed to charge money for access to mods**.
You'd be profiting off of Intellectual Property that isn't yours. Donations are fine - as long as they do not give access to 'premium' or 'hidden' features.

**Respect others and their work. Code being publicly available does not mean you are permitted to copy it and use as your own.**
All modders work hard on their mods and nobody likes when their stuff is used without their permission. Many display their code publicly so that others can learn from it, but will get upset if you blatantly copy big chunks of their work. Always check the license in the repository or whether the author wrote something about permissions in the repository readme file - licenses like MIT allow free use of the code, but if there is no license in the repository it means you are not licensed to use it at all, unless given explicit permission from the author.

**When in doubt - ask, but put in effort first.**
We reside in a [discord server](https://discord.gg/oxygennotincluded) (in the modding section) where we are happy to help you get started, but don't try to skip on reading the guide and rush to ask about things covered here. ~~There are no *dumb questions*~~ (okay, there are), but most of them can be solved through a simple search.

## Downloads

Below you will find links to the required tools. All tools not marked as optional are mandatory. Everything linked below is free to use.

*If you know what you're doing you're welcome to use different tools, but these tools are something we can help with.*

### Visual Studio Community 2019
***Download link:*** https://visualstudio.microsoft.com/vs/
This will be your primary tool for editing C# and browsing game code.
*Installation instructions will be presented in the next chapter of this guide.*

Non-Windows platforms will need to use an alternative, but this will not be covered in this guide.

### dotPeek
***Download link:*** https://www.jetbrains.com/decompiler/download/#section=web-installer
Used to decompile game libraries to be able to read the source code.

*Alternatives:* [dnSpy](https://github.com/0xd4d/dnSpy/releases)

### Notepad++
***Download link:*** https://notepad-plus-plus.org/downloads/
Or any other sensible text editor. Just don't use Windows Notepad. Please. We'll hate you.

### AssetStudio (optional)
***Download link:*** https://github.com/Perfare/AssetStudio/releases
A small tool for extracting Unity assets - you can extract animation files from the game. Optional tool, install only if you want to dig into unused assets, or you can always download it later.

## Visual Studio installation

Visual Studio is *huuuuuuuge*, but don't fret - you need only a few things.

In the **Workloads** tab, select *.NET desktop environment*.

In the **Individual components** tab, select *.NET Framework 4.7.1 SDK* and *.NET Framework 4.7.1 targeting pack*.

After it installs, you're good to go.

*Note:* if you are a student with a *.edu* email, you may be eligible for a free student license of [Reshaper](https://www.jetbrains.com/resharper/). It's a Visual Studio extension that makes life a bit nicer. Without a student license it's rather expensive, so don't feel pressured to get it.

## Decompiling the game files

To be able to make modifications to the code, you need to decompile it first. It's a process of that will allow you to turn the compiled game assemblies into readable code on which you will base your patches.

Two good programs used for decompilation are **dotPeek** and **dnSpy**.

### Locating the game files
* Locate the game folder in your steam library `...\SteamLibrary\steamapps\common\OxygenNotIncluded`
* Go into the `\OxygenNotIncluded_Data\Managed` directory. This folder contains all the game assemblies that you will need for your mods.

### Decompilation and exporting to a project
* Once you open dotPeek, go to `File -> Open`, navigate to the aforementioned folder and choose `Assembly-CSharp.dll`
* Once the file decompiles, you will be able to browse and navigate through the code. We **heavily recommend** exporting the sources to a project to be able to view them in Visual Studio (next step)
* Right click on the `Assembly-CSharp` project in the left sidebar then choose `Export to project`. The following options should be selected:
  * `Create .sln file`
  * Optionally `Open project in Visual Studio` if you want it to open automatically once it's done exporting. You can always open it from the folder that you save it to.

### Navigating the code in Visual Studio
Once you have your decompiled sources you're ready to browse the code.
*If you're new to VS*: Visual Studio offers many cool keybinds and shortcuts that make your life easier and navigation faster, such as finding usages of a given method or all references of a field. Take a while to play and familiarize yourself with the environment. You can also look up some shortcut cheat sheets for VS online.

## Your first mod

### Download project template (optional)
We've prepared a small mod template for Visual Studio that will allow you to create a project that already has references to external libraries and a few basic patches. You can download it [here](https://github.com/Cairath/Oxygen-Not-Included-Modding/raw/master/resources/ONI%20Mod.zip).
To add it to VS, place the downloaded .zip (don't unpack it!) in `%USERPROFILE%\Documents\Visual Studio 2019\Templates\ProjectTemplates`.

### New project
Start Visual Studio. On the main screen select `Create a new project`, then follow one of the paths below depending on whether you downloaded the template above.

* **Using project template**

  Search for `ONI Mod` in the template list. Do **not** select `Place solution and project in the same directory`.
  If the template does not appear on the list, make sure you imported the template correctly and restart VS. Click `next` to continue.

* **Without template**

  Choose `Class Library (.NET Framework)` in the template list. Click `next` then **select .NET Framework 4.7.1 in the bottom dropdown.**

Click next, then choose names for your solution and project. Solution is a collection of projects, and many of us have one solution called something along the lines of `ONI Mods` with many projects in it (one project = a single mod). If you prefer to have one solution per mod, that's fine too!

After you name your solution and project, click `Create` - and that's all for this step.

### Required game files
Once your project is created you will see a bunch of errors. It's time to import required dependencies.
* Go to the folder that you saved your solution in. (It will have `SolutionName.sln` file in it)
* Create a new folder in that directory, called `lib`
* Back in the `...\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed` folder, find the following files and copy them to the `lib` directory:
  * `Assembly-CSharp.dll`
  * `Assembly-CSharp-firstpass.dll`
  * `0Harmony.dll`
  * `UnityEngine.dll`
  * `UnityEngine.CoreModule.dll`
* Below we list other often used files that will not be needed in this mod, but you will most likely find yourself using them at some point:
  * `UnityEngine.UI.dll`
  * `Unity.TextMeshPro.dll`
  * `UnityEngine.ImageConversionModule.dll`

Once all files are in place, try to build your project (on the sidebar, right click the project name and click `Build`). It should now recognize all files and build with no issues.
If you have issues with references not being found - please restart Visual Studio or re-add the references manually.

### Game log
The game output log is located in `%USERPROFILE%\AppData\LocalLow\Klei\Oxygen Not Included\player.log`. This file will be your friend - create a shortcut somewhere accessible. Open with Notepad++. More details about how to work with the log will be published on another page.

### First look at the code
At this point you should be familiar with Harmony patches. If you aren't -- go back and read the documentation. One of the main things you will be doing with modding is writing patches to modify the original game logic. There are a few ways of doing it, and now we'll cover the most basic one.

`Db_Initialize_Patch` is an example of a patch class (file from the example project, [available here](https://github.com/Cairath/Oxygen-Not-Included-Modding/blob/master/examples/ONI%20Hello%20World%20Mod/ONIMod/Patches.cs)):
```cs
# Patches.cs

using HarmonyLib;

namespace ONIMod
{
	public class Patches
	{
		[HarmonyPatch(typeof(Db))]
		[HarmonyPatch("Initialize")]
		public class Db_Initialize_Patch
		{
			public static void Prefix()
			{
				Debug.Log("I execute before Db.Initialize!");
			}

			public static void Postfix()
			{
				Debug.Log("I execute after Db.Initialize!");
			}
		}
	}
}
```

`Db_Initialize_Patch` contains two patches (last chance to go read Harmony documentation!) that will be executed before and after `Db.Initialize()` in the original code. You can see the results of `Debug.Log()` in the aforementioned game log file.

All patches need to reside somewhere - usually it's a bigger class with patches (in this case the `Patches` class), or sometimes a few smaller ones if there's a logical need to divide them. Where you put the patch classes does not matter - but keep it tidy -- you'll have to return to the code later at some point, so don't do anything the future you will regret.

The resulting file `Patches.cs` has the `Patches` class which holds all the patches we'll use in the example mod. You do not need to do anything to register or execute them - by default the game will pick them up on its own and apply them when it starts. If you want to have more control over the patching process or need to do something before or after it, please add [UserMod2](02-mod-structure.md#usermod2) to your reading list.

### mod.yaml and mod_info.yaml
The mods use two metadata files - they are fully described in the next chapter.

You need to create:

`mod.yaml` - while the mod will work without it, do not omit it!
```yaml
title: "Your Mod Title"
description: "Something About Your Mod"
staticID: "yourSuperCoolMod"
```

`mod_info.yaml` - this file is absolutely necessary and the mod will not work without it
```yaml
supportedContent: ALL # possible options: ALL, EXPANSION1_ID, VANILLA_ID
minimumSupportedBuild: 468097 # the lowest game version that will load the mod
version: 1.0.0 # version of your mod, displayed on the mod list. for your and users' info
APIVersion: 2 # required to specify that the mod is using Harmony 2 and has been upgraded for the mergedown changes
```

For full information about these files, their properties and the mod structure please read [this chapter](02-mod-structure.md).

### Testing the mod
Once you compile the mod (remember to compile it as `Release` version!), you have to move it to the game mod directory.

You want to move **only** your mod's dll and not any of the game files that are being referenced. Also include the `mod_info.yaml` and `mod.yaml` mentioned above.

In `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods` there are three folders (if there aren't, please create them):
* `\Dev` - this is where mods in development should go. If a mod in this folder crashes, it will not be disabled automatically when the game restarts. Use this for testing.
* `\Steam` - mods to which you subscribe on Steam are automatically downloaded there. Mods will be auto-disabled upon a mod crash (of any mod).
* `\Local` - locally installed mods, otherwise behaves as `\Steam`.

On other platforms, you can find the directories here:
* Linux: `~/.config/unity3d/Klei/OxygenNotIncluded/mods`
* Mac: `~/Library/Application Support/unity.Klei.Oxygen Not Included/mods` *(you've made a bad life decision, you'll have a lot of figuring out to do)*

Each mod should have its own separate folder, so for example your `ExampleMod` path would look like this:
`...\Klei\OxygenNotIncluded\mods\Dev\ExampleMod\`
- `ExampleMod.dll`
- `mod.yaml`
- `mod_info.yaml`

Once you've confirmed the files are in the correct place, you can launch the game and click the `MODS` button. Find your mod on the list, enable it then allow the game to restart.

To verify the mod worked, you should open the game log and look for the two lines that your mod printed - `I execute before Db.Initialize!` and `I execute after Db.Initialize!`.

If you found them - congratulations, you've succeeded with your first mod.
