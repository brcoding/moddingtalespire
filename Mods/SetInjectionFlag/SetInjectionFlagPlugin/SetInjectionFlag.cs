using BepInEx;
using ModdingTales;

namespace PluginUtilities
{
    [BepInPlugin("org.generic.plugins.setinjectionflag", "Set Injection Flag Plugin", "1.2.0.0")]
    class SetInjectionFlag : BaseUnityPlugin
    {
        void Awake()
        {
            Logger.LogInfo("In Awake for SetInjectionFlag Plug-in");

            UnityEngine.Debug.Log("SetInjectionFlag Plug-in loaded");
            ModdingUtils.Initialize(this, this.Logger);
        }
    }
}
