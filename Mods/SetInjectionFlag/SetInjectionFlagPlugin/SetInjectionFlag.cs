using BepInEx;
using ModdingTales;

namespace PluginUtilities
{
    [BepInPlugin("org.generic.plugins.setinjectionflag", "Set Injection Flag Plugin", "1.1.1.0")]
    class SetInjectionFlag : BaseUnityPlugin
    {
        void Awake()
        {
            UnityEngine.Debug.Log("SetInjectionFlag Plug-in loaded");
            ModdingUtils.Initialize(this);
        }
    }
}
