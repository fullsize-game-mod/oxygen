# Adjustable Space Heater

[Back to mod list](../README.en.md) · [简体中文](README.md) | English

Adds an adjustable, bidirectional temperature controller to the vanilla Space Heater in **Oxygen Not Included**: **heats below the target temperature and actively cools above it**. Adds no new buildings, requires no PLib, and does not modify the Liquid Tepidizer.

> Current version: **1.1.0**. Thermal calculation checks pass, but in-game validation has not been completed. Back up your saves before use. This English README does not add English in-game localization: the mod name and custom tooltips are currently in Chinese.

## Features

| Setting | Behavior / value |
| --- | --- |
| Target temperature | −200 to 200°C, adjustable by slider or numeric input with one decimal place |
| Default target | 70°C; each building saves its own setting |
| Bidirectional control | Active heating or cooling; idles when no eligible medium is present or all affected cells reach the target |
| Thermal power | Combined absolute heating and cooling energy is limited to 18 kDTU/s per building |
| Active power draw | Retains the vanilla 120 W consumption |
| Update interval | Every 200 ms |
| Area of effect | Reachable non-solid cells within a radius of 2 cells on the same asteroid |
| Eligible media | Gases and liquids; solids and vacuum are not modified directly |
| Base overheat temperature | 1000°C; material overheat bonuses remain in effect, and melting points are unchanged |
| Temperature units | Follows the game's Celsius / Fahrenheit / Kelvin setting; the physical range remains −200 to 200°C |

The bidirectional controller replaces the vanilla fixed self-heating, exhaust heat, and heating-only state machine so that heat generation does not counteract cooling. Existing `Operational` requirements, including power, automation, damage, and flooding, still apply. The game continues to handle natural heat conduction.

## Installation

1. Download and extract [AdjustableSpaceHeater.zip](../dist/AdjustableSpaceHeater.zip) from this repository, or copy the [dist/AdjustableSpaceHeater/](../dist/AdjustableSpaceHeater/) folder directly.
2. Place the entire `AdjustableSpaceHeater` folder in your platform's local mods directory. Create `Local` if it does not exist.
3. Launch the game, enable **“空间加热器温控”** in the Mods menu, and restart when prompted.

| Platform | Local mods directory |
| --- | --- |
| Windows | `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\` |
| macOS | `~/Library/Application Support/unity.Klei.Oxygen Not Included/mods/Local/` |
| Linux | `~/.config/unity3d/Klei/Oxygen Not Included/mods/Local/` |

If Windows redirects Documents through OneDrive or another service, use the actual Documents location. The installed layout must be:

```text
mods/Local/AdjustableSpaceHeater/
├── AdjustableSpaceHeater.dll
├── mod.yaml
└── mod_info.yaml
```

Do not place the ZIP itself in `Local` or nest the mod inside an extra folder with the same name. Avoid enabling other mods that alter the Space Heater's temperature control at the same time.

## Usage

1. Build or select a vanilla Space Heater and satisfy its operating requirements, including power.
2. Set a target temperature using the sidebar slider or numeric input.
3. For example, setting −200°C in a 20°C environment actively removes heat; changing the target to 200°C actively adds heat.
4. Each building can have a different target, stored with the save. Disabling this mod stops custom target temperatures from taking effect.

### Operating limits

- **Not instant temperature control:** heat exchange is limited by the building's thermal power and the medium's mass and specific heat capacity. The per-cell calculation does not overshoot the target in a single update; natural conduction and phase changes can still affect in-game temperatures.
- **Cooling deletes heat:** removed heat is not transferred to the building or another location. This is not an energy-conserving heat pump model.
- **No direct solid temperature control:** liquids produced by condensation remain eligible, but frozen solids are no longer adjusted directly by the controller.
- **Operating conditions still apply:** loss of power, disabled automation, damage, or flooding can stop operation. A continuous external heat source can also prevent the target from being reached.
- **Overheat threshold is not a melting point:** the 1000°C base threshold does not prevent construction materials from melting at their own melting points.

## Compatibility and validation status

- Targets the **Unity 6 / Harmony 2.4.2** branch and `netstandard2.1`.
- `mod_info.yaml` declares `minimumSupportedBuild: 718946` and `APIVersion: 2`. Build 718946 is the lower bound associated with the Harmony version used by this project, **not a claim of successful in-game testing on that build**.
- The existing `tests/ThermalBehavior` checks pass: heating / cooling direction, power budget, the absolute energy budget for mixed hot and cold cells, heat capacity, no overshoot, target reversal, and temperature range boundaries.
- These checks run isolated thermal calculations. They **do not cover** game loading, sidebar rendering, actual phase changes, overheat damage, save persistence, or DLC combinations. In-game compatibility remains unverified.
- Game updates may change building APIs. Rebuilding against your own current game assemblies checks compile-time compatibility, but does not replace in-game acceptance checks.

## Building from source

Install [.NET SDK](https://dotnet.microsoft.com/download) 8 or later and use the current game assemblies from your own Oxygen Not Included installation. Run all commands below from the repository root.

`GameManagedPath` must point to the `Managed` directory containing:

```text
Assembly-CSharp.dll
Assembly-CSharp-firstpass.dll
UnityEngine.dll
UnityEngine.CoreModule.dll
```

On Windows / Linux, this is usually `OxygenNotIncluded_Data/Managed` inside the game installation. On macOS, locate `Managed` inside the game's `.app` bundle. Replace the example path with your actual directory:

```sh
dotnet build AdjustableSpaceHeater/AdjustableSpaceHeater.csproj -c Release "-p:GameManagedPath=/absolute/path/to/Managed"
```

Alternatively, set the `ONI_MANAGED_PATH` environment variable. The first build needs internet access to restore the `Lib.Harmony.Ref` 2.4.2 compile-time reference. The game supplies runtime dependencies.

A successful build automatically populates `dist/AdjustableSpaceHeater/` with only the mod DLL and two YAML files. Game, Unity, and Harmony DLLs are not included. The build **does not update the ZIP automatically**: before distribution, compress the entire generated folder into `dist/AdjustableSpaceHeater.zip` again.

## Running calculation checks

No game assemblies are required. The check project targets `net8.0`: use the .NET 8 SDK, or a newer SDK capable of building that target together with the .NET 8 runtime.

```sh
dotnet run --project tests/ThermalBehavior/ThermalBehavior.csproj -c Release
```

Successful output:

```text
PASS: heating/cooling direction, power budget, mixed cells, heat capacity, no overshoot, target reversal and range boundaries.
```

### In-game acceptance checklist

- Build two Space Heaters and enter −200, 0, and 200°C independently. Check the sidebar's input limits and confirm that each building has its own setting.
- Set −200°C in a 20°C environment and confirm actual cooling. Change to 200°C and confirm actual heating. The building should idle once its target is reached.
- Check that loss of power and disabled automation stop heat exchange. Check that condensed liquids can still be cooled, noting the limits caused by flooding and freezing.
- Save and reload with different targets on the two buildings. Confirm that settings persist and that existing buildings in older saves receive the temperature-control interface.
- Use a material without an overheat bonus to check the 1000°C base threshold, then check material bonuses and confirm that the Liquid Tepidizer is unaffected.

## Source structure

```text
AdjustableSpaceHeater/
├── README.md                         # Chinese documentation
├── README.en.md                      # English documentation
├── AdjustableSpaceHeater.csproj      # Build and packaging configuration
├── Mod.cs                            # Mod entry point and tooltip strings
├── Patches.cs                        # Vanilla building patches
├── HeaterTemperatureControl.cs       # Temperature UI and simulation updates
├── TemperatureSettings.cs            # Temperature, power, and range constants
├── ThermalMath.cs                    # Pure heat-exchange calculations
├── mod.yaml                          # Mod name and description
└── mod_info.yaml                     # Version and game API requirements
```

## Changelog

- **1.1.0:** Replaces threshold-only heating with true bidirectional temperature control and active cooling. Removes vanilla fixed heat generation and adds heating / cooling transitions and power-boundary checks.
- **1.0.0:** Only adjusted the temperature at which heating stops; did not provide cooling. Superseded by 1.1.0.

## References

- [Klei: .NET Standard 2.1 and Harmony 2.4.2 on the Unity 6 branch](https://forums.kleientertainment.com/forums/topic/170159-game-update-public-testing-718946/)
- [ONIMods: local mod installation directories](https://github.com/peterhaneve/ONIMods#local-downloads)
- [.NET CLI: dotnet build](https://learn.microsoft.com/dotnet/core/tools/dotnet-build) and [dotnet run](https://learn.microsoft.com/dotnet/core/tools/dotnet-run)
- [OniStubs: community game API compilation references](https://github.com/richardspence/OniStubs)
- Historical interface references: [SimMessages](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/SimMessages.cs) and [negative-energy cooling in HeatBulb](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/HeatBulb.cs). Historical source and community API references do not establish that current game internals behave identically.
