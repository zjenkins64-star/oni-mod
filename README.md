# Gas Pressure Equalizer

An Oxygen Not Included mod that adds a powerless gas-equalization system: pair
two **Equalizer Vents**, run **Equalizer Ducts** between them, and gas flows
from the higher-pressure room to the lower-pressure room until they balance —
no power, no Gas Pumps, no automation needed.

Compatible with vanilla and *Spaced Out*.

## Buildings

All three are unlocked by the **Gas Piping** research tech (the same node that
unlocks stock gas pipes / pumps / vents) and live under the **Ventilation** tab
in the build menu.

### Equalizer Vent

The endpoint of the system. Place one in each room you want to bridge.
Powerless. The vent senses room gas pressure and exchanges gas through the duct
network attached to it.

### Equalizer Duct

The pipe that connects vents. Drag-line placement, goes through walls, supports
the same materials as stock gas pipes. Distinct cyan paint job and a subdued
greyscale wave that pulses along the active flow direction whenever the
equalizer is moving gas.

### Equalizer Bridge

A 3-tile crossing piece for routing ducts past stock pipes, wires, or other
networks. Same role as the stock Gas Pipe Bridge but on the equalizer network.

## How it works

The equalizer doesn't use ONI's standard conduit flow simulation. Instead, the
mod tracks its own network of vents and ducts and moves gas directly between
connected vent cells via `SimMessages` — that's why it's powerless and works
bidirectionally regardless of pipe drawing direction.

- Multiple vents on one duct network all equalize toward the same average.
- Adding or removing ducts/vents is picked up live; the wave animation extends
  along new branches automatically.
- Dead-end branches (no vent at the end) don't animate.

## Compatibility

### ⚠️ PipeFlowOverlay

If you have the **Pipe Flow Overlay** mod installed, the directional arrows it
draws over our ducts will be misleading — equalizer flow is bidirectional, so
the arrows often point the "wrong way." This is purely cosmetic; the equalizer
itself works correctly. If the arrows bother you, disable Pipe Flow Overlay
just for the rooms you've ducted, or accept the visual glitch.

### Stock gas pipes

Equalizer Ducts can be placed adjacent to stock gas pipes without merging
networks visually (cross-system connection bumps are suppressed by Harmony
patches). Whether stock gas can route *through* an Equalizer Duct depends on
your build state; the equalizer's own gas movement is independent of the stock
gas conduit network.

## Installation

### From release

Drop the contents into:

`%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\GasPressureEqualizer\`

(create the `Local` folder if it doesn't exist)

Enable in the in-game mod menu, restart when prompted.

### From source

Requires Visual Studio + .NET Framework 4.7.1 SDK (Windows) or the .NET 4.8
reference assemblies (macOS) and an Oxygen Not Included install.

```
dotnet build GasPressureEqualizer/GasPressureEqualizer.csproj -c Release
```

The csproj auto-detects ONI in the common Steam install locations on Windows
(`C:\Program Files (x86)\Steam\...`, `D:\Games\SteamLibrary\...`) and macOS
(`~/Library/Application Support/Steam/...`). If yours is somewhere else, set
`ONI_INSTALL` before building:

```
# Windows (PowerShell)
$env:ONI_INSTALL = "E:\Steam\steamapps\common\OxygenNotIncluded"
dotnet build GasPressureEqualizer/GasPressureEqualizer.csproj -c Release

# macOS / Linux
ONI_INSTALL="$HOME/Games/OxygenNotIncluded" \
  dotnet build GasPressureEqualizer/GasPressureEqualizer.csproj -c Release
```

The mods folder is also auto-detected (Windows resolves `Documents\Klei\...`
through the real Documents path, so OneDrive-redirected setups Just Work; Mac
uses `~/Library/Application Support/unity.Klei.Oxygen Not Included/mods/Local`).
Override with `ONI_MODS_DIR` if needed.

The post-build target copies the DLL, `mod_info.yaml`, `mod.yaml`, and the
`anim/` folder into `<ONI_MODS_DIR>/GasPressureEqualizer/` automatically. If
the mods folder doesn't exist (e.g. on a build machine without ONI), the copy
step is skipped silently and the build still succeeds.

## Credits

Built by Andrew Jenkins. Custom kanim textures painted on top of stock ONI gas
conduit / vent / bridge assets.
