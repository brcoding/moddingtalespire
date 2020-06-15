using BepInEx;
using ModdingTales;

namespace SocketAPI
{
    [BepInPlugin("org.generic.plugins.socketapi", "Socket API Plugin", "1.2.1.0")]
    class SocketAPIPlugin : BaseUnityPlugin
    {
        void Awake()
        {
            UnityEngine.Debug.Log("SocketAPI Plug-in loaded");
            ModdingUtils.Initialize(this, true);
        }

        void Update()
        {
            ModdingUtils.OnUpdate();
        }
    }
}
