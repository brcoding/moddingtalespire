# Set Injection Flag Plugin

This is a plugin for TaleSpire using BepInEx.


## Install

Go to the releases folder and download the latest and extract to the contents of your TaleSpire game folder.

## Usage

Just install, this will automatically update TaleSpire so the devs know you are doing modding work.

## How to Compile / Modify

Open ```SetInjectionFlagPlugin.sln``` in Visual Studio.

You will need to add references to:

```
* BepInEx.dll  (Download from the BepInEx project.)
* Bouncyrock.TaleSpire.Runtime (found in Steam\steamapps\common\TaleSpire\TaleSpire_Data\Managed)
* UnityEngine.dll
* UnityEngine.CoreModule.dll
* UnityEngine.InputLegacyModule.dll 
* UnityEngine.UI
* Unity.TextMeshPro
```

Build the project.

Browse to the newly created ```bin/Debug``` or ```bin/Release``` folders and copy the ```SetInjectionFlagPlugin.dll``` to ```Steam\steamapps\common\TaleSpire\BepInEx\plugins```