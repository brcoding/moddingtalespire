using BepInEx;
using ModdingTales;

namespace SocketAPI
{
    [BepInPlugin("org.generic.plugins.socketapi", "Socket API Plugin", "1.3.0.0")]
    class SocketAPIPlugin : BaseUnityPlugin
    {
        void Awake()
        {
            Logger.LogInfo("In Awake for SocketAPI Plug-in");

            UnityEngine.Debug.Log("SocketAPI Plug-in loaded");
            ModdingUtils.Initialize(this, this.Logger, true);
        }

        void Update()
        {
            ModdingUtils.OnUpdate();
        }
    }
}
