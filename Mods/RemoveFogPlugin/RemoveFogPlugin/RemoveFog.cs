using System;
using BepInEx;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace RemoveFogPlugin
{
    [BepInPlugin("org.d20armyknife.plugins.removefog", "Remove Fog Plug-In", "1.1.0.0")]
    [BepInProcess("TaleSpire.exe")]
    public class DefogIt: BaseUnityPlugin
    {

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            UnityEngine.Debug.Log("Remove Fog Plug-in loaded");
            // Set the UsingCodeInjection so we don't anger the @Baggers
            AppStateManager.UsingCodeInjection = true;
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
            if (Input.GetKeyUp(KeyCode.G))
            {
                ToggleFog();
            }
        }

        private bool fogEnabled = true;
    }
}
