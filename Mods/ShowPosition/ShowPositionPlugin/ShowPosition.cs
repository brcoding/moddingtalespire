using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShowPosition
{
    [BepInPlugin("org.d20armyknife.plugins.showposition", "Show Position Plug-In", "1.3.0.0")]
    public class ShowPositionPlugin: BaseUnityPlugin
    {
        private ConfigEntry<bool> RulerEnabled { get; set; }
        private ConfigEntry<bool> ShowPosition { get; set; }
        private ConfigEntry<float> RulerFontSize { get; set; }

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            Logger.LogInfo("In Awake for ShowPosition Plug-in");

            UnityEngine.Debug.Log("Show Position Plug-in loaded");
            RulerEnabled = Config.Bind("Show Position", "Ruler enabled", true);
            RulerFontSize = Config.Bind("Show Position", "Ruler Font Size", 24f);
            ShowPosition = Config.Bind("Show Position", "Show Position enabled", true);

            ModdingTales.ModdingUtils.Initialize(this, this.Logger);
        }

        private bool wasShowingMouse = false;

        void Update()
        {
            if (CameraController.HasInstance)
            {
                if (RulerEnabled.Value)
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

                        TextMeshProUGUI cursorText = (TextMeshProUGUI)typeof(MouseManager).GetField("_cursorText", flags).GetValue(MouseManager.Instance);// this._cursorText
                        cursorText.fontSize = RulerFontSize.Value * GUIManager.GetUIScaleValue();
                        wasShowingMouse = true;
                    }
                    else
                    {
                        if (wasShowingMouse)
                        {
                            MouseManager.ShowMouseText("", false);
                            wasShowingMouse = false;
                        }
                    }
                }

                if (ShowPosition.Value)
                {
                    TextMeshProUGUI you = ModdingTales.ModdingUtils.GetUITextContainsString("YOU");
                    if (you != null)
                    {
                        you.SetText(String.Format("\n\n<size=14><color=green>YOU</color></size>\n<size=22><color=#aaaaaa>{0:0.##}  {1:0.##}  {2:0.##}</color></size>", CameraController.Position.x, CameraController.CameraHeight, CameraController.Position.z));
                    }
                }
            }
        }

    }
}
