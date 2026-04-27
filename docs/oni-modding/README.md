# ONI Modding Reference

Local cache of community ONI modding docs (no official Klei docs exist). Pages 01–05 are **verbatim mirrors** of the [Cairath wiki](https://github.com/Cairath/Oxygen-Not-Included-Modding/wiki) (cloned via git, not summarized). Page 06 is a synthesis of the [PLib](https://github.com/peterhaneve/ONIMods) README. Synced 2026-04-26.

To resync: `git clone --depth=1 https://github.com/Cairath/Oxygen-Not-Included-Modding.wiki.git $env:TEMP\oni-wiki`

## Pages

- [01-introduction.md](01-introduction.md) — prereqs, tools, VS setup, decompilation, first mod
- [02-mod-structure.md](02-mod-structure.md) — `mod_info.yaml`, `mod.yaml`, `UserMod2` lifecycle
- [03-animations.md](03-animations.md) — kanim format, Spriter workflow, asset layout
- [04-publishing.md](04-publishing.md) — local install + Steam Workshop upload
- [05-api-changes.md](05-api-changes.md) — modding API changelog
- [06-plib.md](06-plib.md) — Peter Han's PLib utility library

## When stuck, decompile

The real source of truth is `Assembly-CSharp.dll` in `<ONI>\OxygenNotIncluded_Data\Managed\`. Decompile with **dnSpy** or **dotPeek**. Look up the game class you want to extend or patch and read its actual code — every modding pattern boils down to "call the same APIs the game already uses."

## Active community

- [Klei Forums — Mods and Tools](https://forums.kleientertainment.com/forums/forum/204-oxygen-not-included-mods-and-tools/)
- ONI Modding Discord (linked from Klei forums)
