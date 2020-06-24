using System;
using BepInEx;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace RemoveFogPlugin
{
    [BepInPlugin("org.d20armyknife.plugins.removefog", "Remove Fog Plug-In", "1.1.2.0")]
    public class DefogIt: BaseUnityPlugin
    {
        private ConfigEntry<KeyboardShortcut> FogKey { get; set; }
        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            Logger.LogInfo("In Awake for Remove Fog Plug-in");

            UnityEngine.Debug.Log("Remove Fog Plug-in loaded");
            FogKey = Config.Bind("Hotkeys", "Toggle Fog Shortcut", new KeyboardShortcut(KeyCode.F, KeyCode.LeftControl));
            ModdingTales.ModdingUtils.Initialize(this, this.Logger);
        }

        private void ToggleFog()
        {
            this.fogEnabled = !this.fogEnabled;
            RenderSettings.fog = this.fogEnabled;
            var postProcessLayer = Camera.main.GetComponent<PostProcessLayer>();
            postProcessLayer.fog.enabled = this.fogEnabled;
        }
        
        void Update()
        {
            if (FogKey.Value.IsUp())
            {
                ToggleFog();
            }
        }

        private bool fogEnabled = true;
    }
}
