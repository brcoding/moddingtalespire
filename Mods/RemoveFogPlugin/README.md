# Remove Fog Plugin

This is a plugin for TaleSpire using BepInEx.


## Install

Go to the releases folder and download the latest and extract to the contents of your TaleSpire game folder.

## Usage

To toggle fog press the __```g```__ key.

## How to Compile / Modify

Open ```RemoveFogPlugin.sln``` in Visual Studio.

You will need to add references to:

```
* BepInEx.dll  (Download from the BepInEx project.)
* Bouncyrock.TaleSpire.Runtime (found in Steam\steamapps\common\TaleSpire\TaleSpire_Data\Managed)
* UnityEngine.dll
* UnityEngine.CoreModule.dll
* UnityEngine.InputLegacyModule.dll 
* Unity.Postprocessing.Runtime.dll
```

Build the project.

Browse to the newly created ```bin/Debug``` or ```bin/Release``` folders and copy the ```RemoveFogPlugin.dll``` to ```Steam\steamapps\common\TaleSpire\BepInEx\plugins```