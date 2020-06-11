# Show Position Plugin

This is a plugin for TaleSpire using BepInEx.


## Install

Go to the releases folder and download the latest and extract to the contents of your TaleSpire game folder.

## Usage

Below the "Game Master" portrait you will see "YOU". Below this you will see the X, Y, Z.

## How to Compile / Modify

Open ```ShowPositionPlugin.sln``` in Visual Studio.

You will need to add references to:

```
* BepInEx.dll  (Download from the BepInEx project.)
* Bouncyrock.TaleSpire.Runtime (found in Steam\steamapps\common\TaleSpire\TaleSpire_Data\Managed)
* UnityEngine.dll
* UnityEngine.CoreModule.dll
* UnityEngine.InputLegacyModule.dll 
```

Build the project.

Browse to the newly created ```bin/Debug``` or ```bin/Release``` folders and copy the ```ShowPositionPlugin.dll``` to ```Steam\steamapps\common\TaleSpire\BepInEx\plugins```