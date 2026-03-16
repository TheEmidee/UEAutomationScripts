# UEAutomationScripts

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE) 

Collection of automation scripts to use in an Unreal Engine environment

## Installation 🛠️

- Add to your unreal engine project repository as a submodule: `git submodule add git@github.com:TheEmidee/UEAutomationScripts Scripts/Automation/`

## How to use

- Pass the automation folder to `AutomationTool.exe` when you run it using the `-ScriptDir` argument. Ex: `E:\Dev\UE\Engine\Binaries\DotNET\AutomationTool\AutomationTool.exe -ScriptDir=C:\YourGameFolder\Scripts\Automation XXX`

- The [Buildgraph](Automation/Buildgraph) tasks can be used in a buildgraph file.

Ex:

```xml
<Agent Name="Naming Convention Validation $(EditorPlatform)" Type="$(HostAgentType)">
    <Node Name="Naming Convention Validation $(EditorPlatform)" Requires="$(PlatformEditorPlatformCompileNodeName)">
        <Commandlet Name="NamingConventionValidation" Project="$(ProjectFile)" Arguments="$(CommandletCommonArguments) -log=NamingConventionValidation.log $(Test_DataValidationArguments)" />
        <GenerateJenkinsCompatibleWarningsFile LogFile="$(LogsDirectory)/NamingConventionValidation.log" Category="NamingConvention" OutputFile="$(Test_WarningsDirectory)/NamingConventionValidation.txt" Append="False" If="$(IsBuildMachine)" />
    </Node>
</Agent>
```