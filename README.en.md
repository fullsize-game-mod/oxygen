# Oxygen Not Included Mods

[简体中文](README.md) | English

This repository contains mods for **Oxygen Not Included**, with a separate source directory and installation package for each mod. Install and enable the mods you want independently.

## Mod list

| Mod | Overview | Documentation | Download |
| --- | --- | --- | --- |
| Adjustable Space Heater | Adds bidirectional temperature control from −200 to 200°C to the vanilla Space Heater: heats below the target and actively cools above it | [Usage, settings, and limits](AdjustableSpaceHeater/README.en.md) | [AdjustableSpaceHeater.zip](dist/AdjustableSpaceHeater.zip) |
| Solid Resource Generator | Adds a powered building that generates a selected solid element at 0–100 kg/s, with automation start/stop control | [Usage, resource support, and limits](SolidResourceGenerator/README.md) (Chinese) | [SolidResourceGenerator.zip](dist/SolidResourceGenerator.zip) |
| Liquid Resource Generator | Generates a selected liquid element into liquid pipes, with an adjustable 0–100 kg/s rate limited by pipe capacity and automation start/stop control | [Usage, pipe output, and limits](LiquidResourceGenerator/README.md) (Chinese) | [LiquidResourceGenerator.zip](dist/LiquidResourceGenerator.zip) |
| Food Resource Generator | Generates raw ingredients including meat, raw egg, fish and crops at 0–100 kg/s, with automation start/stop control | [Usage, ingredients, and limits](FoodResourceGenerator/README.md) (Chinese) | [FoodResourceGenerator.zip](dist/FoodResourceGenerator.zip) |

See each mod's documentation for its version, settings, usage, and validation scope. In-game acceptance testing has not been completed; back up your saves before use. In-game mod names and custom strings are currently in Chinese.

## Installation

1. Download and extract the packages you want from the table above, or copy the corresponding mod folders from [dist/](dist/).
2. Place each entire mod folder in your platform's local mods directory. Create `Local` if it does not exist.
3. Launch the game, enable the corresponding mods in the Mods menu, and restart when prompted.

| Platform | Local mods directory |
| --- | --- |
| Windows | `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\` |
| macOS | `~/Library/Application Support/unity.Klei.Oxygen Not Included/mods/Local/` |
| Linux | `~/.config/unity3d/Klei/Oxygen Not Included/mods/Local/` |

If Windows redirects Documents through OneDrive or another service, use the actual Documents location. With all four mods installed, the layout should be:

```text
mods/Local/
├── AdjustableSpaceHeater/
│   ├── AdjustableSpaceHeater.dll
│   ├── mod.yaml
│   └── mod_info.yaml
├── SolidResourceGenerator/
│   ├── SolidResourceGenerator.dll
│   ├── mod.yaml
│   └── mod_info.yaml
├── LiquidResourceGenerator/
│   ├── LiquidResourceGenerator.dll
│   ├── mod.yaml
│   └── mod_info.yaml
└── FoodResourceGenerator/
    ├── FoodResourceGenerator.dll
    ├── mod.yaml
    └── mod_info.yaml
```

Do not place the ZIP itself in `Local` or nest a mod inside an extra folder with the same name.

## Building from source

Install [.NET SDK](https://dotnet.microsoft.com/download) 8 or later and use the game assemblies from your own Oxygen Not Included installation. Run all commands below from the repository root.

`GameManagedPath` must point to the `Managed` directory containing:

```text
Assembly-CSharp.dll
Assembly-CSharp-firstpass.dll
UnityEngine.dll
UnityEngine.CoreModule.dll
```

On Windows / Linux, this is usually `OxygenNotIncluded_Data/Managed` inside the game installation. On macOS, locate `Managed` inside the game's `.app` bundle. Replace the example path with your actual directory and build the mods you need:

```sh
dotnet build AdjustableSpaceHeater/AdjustableSpaceHeater.csproj -c Release "-p:GameManagedPath=/absolute/path/to/Managed"
dotnet build SolidResourceGenerator/SolidResourceGenerator.csproj -c Release "-p:GameManagedPath=/absolute/path/to/Managed"
dotnet build LiquidResourceGenerator/LiquidResourceGenerator.csproj -c Release "-p:GameManagedPath=/absolute/path/to/Managed"
dotnet build FoodResourceGenerator/FoodResourceGenerator.csproj -c Release "-p:GameManagedPath=/absolute/path/to/Managed"
```

Alternatively, set the `ONI_MANAGED_PATH` environment variable. The first build needs internet access to restore the `Lib.Harmony.Ref` 2.4.2 compile-time reference. The game supplies runtime dependencies.

A successful build automatically updates `dist/<mod name>/` with only that mod's DLL and two YAML files. Game, Unity, and Harmony DLLs are not included. The build **does not update ZIP archives automatically**: before distribution, compress each entire generated mod folder into its corresponding installation package.

## Running isolated checks

No game assemblies are required. The check projects target `net8.0`: use the .NET 8 SDK, or a newer SDK capable of building that target together with the .NET 8 runtime.

```sh
dotnet run --project tests/ThermalBehavior/ThermalBehavior.csproj -c Release
dotnet run --project tests/SolidGeneration/SolidGeneration.csproj -c Release
dotnet run --project tests/LiquidGeneration/LiquidGeneration.csproj -c Release
dotnet run --project tests/FoodGeneration/FoodGeneration.csproj -c Release
```

- `ThermalBehavior`: Space Heater calculations, including heat-exchange direction, power budget, heat capacity, and temperature boundaries.
- `SolidGeneration`: isolated Solid Resource Generator checks, including production rate, operating conditions, settings, and invalid inputs, using test doubles at game boundaries.
- `LiquidGeneration`: isolated Liquid Resource Generator checks for production rate, pipe connection/blockage, buffer capacity, liquid temperatures, settings, and resource lists, using test doubles at game boundaries.
- `FoodGeneration`: isolated Food Resource Generator checks for item identity, mass/unit conversion, production rate, settings, unavailable ingredients, and resource lists, using test doubles at game boundaries.

These checks do not replace in-game acceptance testing. Sidebar rendering, automation networks, actual save persistence, and DLC combinations still need in-game validation. See each mod's documentation for its detailed validation scope.

## Compatibility

All four mods currently target `netstandard2.1` and reference Harmony 2.4.2. Each `mod_info.yaml` declares `minimumSupportedBuild: 718946` and `APIVersion: 2`. These settings do not establish successful in-game testing on that build. After game updates, rebuild against the current game assemblies and validate in-game behavior.

## Project structure

```text
.
├── README.md                         # Chinese repository overview
├── README.en.md                      # English repository overview
├── AdjustableSpaceHeater/            # Space Heater mod source and documentation
├── SolidResourceGenerator/           # Solid generator mod source and documentation
├── LiquidResourceGenerator/          # Liquid generator mod source and documentation
├── FoodResourceGenerator/            # Food ingredient generator source and documentation
├── tests/
│   ├── ThermalBehavior/              # Thermal calculation checks
│   ├── SolidGeneration/              # Solid production checks
│   ├── LiquidGeneration/             # Liquid production and pipe boundary checks
│   └── FoodGeneration/               # Food production and item boundary checks
└── dist/                             # Installable folders and ZIP archives for each mod
```
