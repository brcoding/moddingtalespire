using BepInEx;
using ModdingTales;

namespace PluginUtilities
{
    [BepInPlugin("org.generic.plugins.setinjectionflag", "Set Injection Flag Plugin", "1.1.0.0")]
    [BepInProcess("TaleSpire.exe")]
    class SetInjectionFlag : BaseUnityPlugin
    {
        void Awake()
        {
            UnityEngine.Debug.Log("SetInjectionFlag Plug-in loaded");
            ModdingUtils.Initialize(this);
        }
    }
}
