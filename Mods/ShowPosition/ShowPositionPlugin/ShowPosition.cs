using System;
using System.Reflection;
using BepInEx;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShowPosition
{
    [BepInPlugin("org.d20armyknife.plugins.showposition", "Show Position Plug-In", "1.1.0.0")]
    public class ShowPositionPlugin: BaseUnityPlugin
    {

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            UnityEngine.Debug.Log("Show Position Plug-in loaded");
            ModdingTales.ModdingUtils.Initialize(this);
        }
        
        void Update()
        {
            TextMeshProUGUI you = ModdingTales.ModdingUtils.GetUITextContainsString("YOU");
            if (you != null)
            {
                you.SetText(String.Format("\n\n<size=14><color=green>YOU</color></size>\n<size=22><color=#aaaaaa>{0:0.##}  {1:0.##}  {2:0.##}</color></size>", CameraController.Position.x, CameraController.CameraHeight, CameraController.Position.z));
            }
        }

    }
}
