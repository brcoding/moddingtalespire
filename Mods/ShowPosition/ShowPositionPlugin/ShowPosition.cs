using System;
using System.Reflection;
using BepInEx;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShowPosition
{
    [BepInPlugin("org.d20armyknife.plugins.showposition", "Show Position Plug-In", "1.2.0.0")]
    public class ShowPositionPlugin: BaseUnityPlugin
    {

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            Logger.LogInfo("In Awake for ShowPosition Plug-in");

            UnityEngine.Debug.Log("Show Position Plug-in loaded");
            ModdingTales.ModdingUtils.Initialize(this, this.Logger);
        }

        private bool wasShowingMouse = false;

        void Update()
        {
            CreatureMoveBoardTool moveBoard = SingletonBehaviour<BoardToolManager>.Instance.GetTool<CreatureMoveBoardTool>();
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

            CreatureBoardAsset cba = (CreatureBoardAsset)typeof(CreatureMoveBoardTool).GetField("_pickupObject", flags).GetValue(moveBoard);
            if (cba != null)
            {
                // Something is picked up
                Transform indicator = (Transform)typeof(CreatureMoveBoardTool).GetField("_indicator", flags).GetValue(moveBoard);
                Vector3 pickupLocation = (Vector3)typeof(CreatureMoveBoardTool).GetField("_pickUpLocation", flags).GetValue(moveBoard);
                float distance = Vector3.Distance(pickupLocation, indicator.position);
                MouseManager.ShowMouseText(String.Format("{0:0}", distance), true);
                wasShowingMouse = true;
            } else
            {
                if (wasShowingMouse)
                {
                    MouseManager.ShowMouseText("", false);
                    wasShowingMouse = false;
                }
            }

            TextMeshProUGUI you = ModdingTales.ModdingUtils.GetUITextContainsString("YOU");
            if (you != null)
            {
                you.SetText(String.Format("\n\n<size=14><color=green>YOU</color></size>\n<size=22><color=#aaaaaa>{0:0.##}  {1:0.##}  {2:0.##}</color></size>", CameraController.Position.x, CameraController.CameraHeight, CameraController.Position.z));
            }
        }

    }
}
