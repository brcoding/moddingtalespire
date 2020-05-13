using BepInEx;
using ModdingTales;
using System;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
