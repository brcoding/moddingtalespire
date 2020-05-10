@echo off

set refpath="G:\\Program Files (x86)\\Steam\\steamapps\\common\\TaleSpire\\TaleSpire_Data\\Managed;E:\\Projects\\Talespirehax\\ModdingTales\\moddingtalespire\\bepinexbin\\BepInEx\\core"

del build\* /q

msbuild.exe AllPlugins.sln /t:Build /p:Configuration=Release -property:ReferencePath="G:\\Program Files (x86)\\Steam\\steamapps\\common\\TaleSpire\\TaleSpire_Data\\Managed"
copy Mods\SetInjectionFlag\SetInjectionFlagPlugin\bin\Release\SetInjectionFlagPlugin.dll build\
copy Mods\RForRotate\RForRotatePlugin\bin\Release\RForRotatePlugin.dll build\
copy Mods\RemoveFogPlugin\RemoveFogPlugin\bin\Release\RemoveFogPlugin.dll build\
copy Mods\RemoveDoFPlugin\RemoveDoFPlugin\bin\Release\RemoveDoFPlugin.dll build\


REM Clone the external projects with mods
IF EXIST TmpTaleSpireModding (
    rmdir TmpTaleSpireModding
) ELSE (
    git clone https://github.com/Mercer01/TalespireModding.git TmpTaleSpireModding
)


REM COMPILE EXTERNAL MODS

msbuild.exe TmpTaleSpireModding\TalespireModding.sln /t:Build /p:Configuration=Release -property:ReferencePath=%refpath%
copy TmpTaleSpireModding\Mods\CameraToolsPlugin\CameraToolsPlugin\bin\Release\CameraToolsPlugin.dll build\
copy TmpTaleSpireModding\Mods\ToggleCharacterNames\bin\Release\ToggleCharacterNames.dll build\
copy TmpTaleSpireModding\Mods\SwitchCharacters\bin\Release\SwitchCharacters.dll build\

rmdir TmpTaleSpireModding\ /s /q

REM PACKAGE UP THE MODS

mkdir tmp
xcopy bepinexbin\* tmp\ /s /e /y
copy build\SetInjectionFlagPlugin.dll tmp\BepInEx\plugins\
powershell Compress-Archive tmp\* build\SetInjectionFlag-Full.zip
del tmp\BepInEx\plugins\SetInjectionFlagPlugin.dll

copy build\RForRotatePlugin.dll tmp\BepInEx\plugins\
powershell Compress-Archive tmp\* build\RForRotate-Full.zip
del tmp\BepInEx\plugins\RForRotatePlugin.dll

copy build\RemoveFogPlugin.dll tmp\BepInEx\plugins\
powershell Compress-Archive tmp\* build\RemoveFogPlugin-Full.zip
del tmp\BepInEx\plugins\RemoveFogPlugin.dll

copy build\RemoveDoFPlugin.dll tmp\BepInEx\plugins\
powershell Compress-Archive tmp\* build\RemoveDoFPlugin-Full.zip
del tmp\BepInEx\plugins\RemoveDoFPlugin.dll

copy build\ToggleCharacterNames.dll tmp\BepInEx\plugins\
powershell Compress-Archive tmp\* build\ToggleCharacterNames-Full.zip
del tmp\BepInEx\plugins\ToggleCharacterNames.dll

copy build\CameraToolsPlugin.dll tmp\BepInEx\plugins\
powershell Compress-Archive tmp\* build\CameraToolsPlugin-Full.zip
del tmp\BepInEx\plugins\CameraToolsPlugin.dll

copy build\SwitchCharacters.dll tmp\BepInEx\plugins\
powershell Compress-Archive tmp\* build\SwitchCharacters-Full.zip
del tmp\BepInEx\plugins\SwitchCharacters.dll

copy build\*.dll tmp\BepInEx\plugins\
powershell Compress-Archive tmp\* build\AllPlugins-Full.zip

rmdir tmp\ /s /q


