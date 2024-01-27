# Better Lobbies [v49]

## Overview

Enhance your multiplayer experience by having additional lobby related QoL features in game!
Note: This mod doesn't fix the "An error occured" issues.

## Features

- **Lobby search**
- **Join lobby via code**
- **Copy lobby code ingame**

## Planning:
- Attempt at fixing lobby joining issues.

## Installation

1. Ensure you have [BepInEx](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/) installed.
2. Download the latest release from [Thunderstore](https://thunderstore.io/c/lethal-company/p/Ryokune/Better_Lobbies/).
3. Extract the contents into your Lethal Company's `BepInEx/plugins` folder.

## Support and Feedback

If you encounter any issues or have suggestions for improvement, feel free to [report them on GitHub](https://github.com/VisualError/Better-Lobbies/issues). If you enjoy the mod, consider supporting the developer by [buying them a coffee on Ko-fi](https://ko-fi.com/ryokune) 


# Version 1.0.0
- Initial release.
# Version 1.0.1
- changed mod description.
# Version 1.0.2
- v47 compatibility.
- fixed challenge moons not showing up
# Version 1.0.3
- still works with v49 wow.
- fixed dev menu from [LethalDevMode](https://thunderstore.io/c/lethal-company/p/megumin/LethalDevMode/) and copy lobby code button overlapping each other.
# Version 1.0.4
- Added Crew Count ([@1A3Dev](https://github.com/1A3Dev)) [P.R: #4](https://github.com/VisualError/Better-Lobbies/pull/4)
- Fixed Debug Menu Overlapping Player List ([@1A3Dev](https://github.com/1A3Dev)) [P.R: #3](https://github.com/VisualError/Better-Lobbies/pull/3)
# Version 1.0.5
- Fixed some disconnect reasons being overwritten with "Failed to connect to lobby! Connection was not approved!" ([@1A3Dev](https://github.com/1A3Dev)) [P.R: #7](https://github.com/VisualError/Better-Lobbies/pull/7)
- Fixed errors on lobby code ui.
- Search is no longer case-sensitive.

## Contributing
### Template `Better_Lobbies/Better_Lobbies.csproj.user`
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <LETHAL_COMPANY_DIR>C:/Program Files (x86)/Steam/steamapps/common/Lethal Company</LETHAL_COMPANY_DIR>
    <TEST_PROFILE_DIR>$(APPDATA)/r2modmanPlus-local/LethalCompany/profiles/TestBetterLobbies</TEST_PROFILE_DIR>
  </PropertyGroup>

    <!-- Create your 'Test Profile' using your modman of choice before enabling this. 
    Enable by setting the Condition attribute to "true". *nix users should switch out `copy` for `cp`. -->
    <Target Name="CopyToDebugProfile" AfterTargets="PostBuildEvent" Condition="true">
		<MakeDir
                Directories="$(LETHAL_COMPANY_DIR)/BepInEx/plugins/Ryokune-BetterLobbies"
                Condition="Exists('$(LETHAL_COMPANY_DIR)') And !Exists('$(LETHAL_COMPANY_DIR)/BepInEx/plugins/Ryokune-Better_Lobbies')"
        />
		<Copy SourceFiles="$(TargetDir)\$(TargetName).pdb" DestinationFolder="$(LETHAL_COMPANY_DIR)/BepInEx/plugins/Ryokune-Better_Lobbies" />
		<Exec Command="copy &quot;$(TargetPath)&quot; &quot;$(LETHAL_COMPANY_DIR)/BepInEx/plugins/Ryokune-Better_Lobbies/&quot;" />
	</Target>
</Project>
```

## Contributors
- [@1A3Dev](https://github.com/1A3Dev)
