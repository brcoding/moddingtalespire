# Handouts Plugin

This is a plugin for TaleSpire using BepInEx.


## Install

Go to the releases folder and download the latest and extract to the contents of your TaleSpire game folder.

## Usage

To share an image URL with other players press the __```p```__ key and paste a publicly accessible URL of a jpg or png image. It will be displayed for 10 seconds.

## How to Compile / Modify

Open ```HandoutsPlugin.sln``` in Visual Studio.

You will need to add references to:

```
* BepInEx.dll  (Download from the BepInEx project.)
* Bouncyrock.TaleSpire.Runtime (found in Steam\steamapps\common\TaleSpire\TaleSpire_Data\Managed)
* UnityEngine.dll
* UnityEngine.CoreModule.dll
* UnityEngine.InputLegacyModule.dll 
```

Build the project.

Browse to the newly created ```bin/Debug``` or ```bin/Release``` folders and copy the ```HandoutsPlugin.dll``` to ```Steam\steamapps\common\TaleSpire\BepInEx\plugins```