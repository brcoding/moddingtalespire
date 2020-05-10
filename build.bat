@echo off

msbuild.exe AllPlugins.sln /t:Build /p:Configuration=Release
copy Mods\SetInjectionFlag\SetInjectionFlagPlugin\bin\Release\SetInjectionFlagPlugin.dll build\
copy Mods\RForRotate\RForRotatePlugin\bin\Release\RForRotatePlugin.dll build\
copy Mods\RemoveFogPlugin\RemoveFogPlugin\bin\Release\RemoveFogPlugin.dll build\
copy Mods\RemoveDoFPlugin\RemoveDoFPlugin\bin\Release\RemoveDoFPlugin.dll build\

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

copy build\*.dll tmp\BepInEx\plugins\
powershell Compress-Archive tmp\* build\AllPlugins-Full.zip

rmdir tmp\ /s /q

