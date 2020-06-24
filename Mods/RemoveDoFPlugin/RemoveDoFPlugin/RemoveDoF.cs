using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

using UnityEngine.SceneManagement;

namespace RemoveDoFPlugin
{
    [BepInPlugin("org.d20armyknife.plugins.removedof", "Remove Depth Of Field Plug-In", "1.3.0.0")]
    public class DeDoFIt: BaseUnityPlugin
    {
        private ConfigEntry<KeyboardShortcut> ToggleDOF { get; set; }
        private ConfigEntry<KeyboardShortcut> ToggleLayer { get; set; }

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            Logger.LogInfo("In Awake for DepthofField Plug-in");
            UnityEngine.Debug.Log("Remove Depth of Field Plug-in loaded");

            ToggleDOF = Config.Bind("Hotkeys", "Depth of Field Shortcut", new KeyboardShortcut(KeyCode.D, KeyCode.LeftControl));
            ToggleLayer = Config.Bind("Hotkeys", "Disable All PostProcessLayers Shortcut", new KeyboardShortcut(KeyCode.H, KeyCode.LeftControl));

            ModdingTales.ModdingUtils.Initialize(this, this.Logger);
        }

        private void ToggleAll()
        {
            this.allEnabled = !this.allEnabled;

            // Disable all post processing effects
            var postProcessLayer = Camera.main.GetComponent<PostProcessLayer>();
            postProcessLayer.enabled = this.allEnabled;
            SystemMessage.DisplayInfoText("Post processing " + getEnabledText(this.allEnabled) + ".");
        }
        private string getEnabledText(bool enabled)
        {
            if (enabled)
            {
                return "enabled";
            } else
            {
                return "disabled";
            }
        }
        private void ToggleDoF()
        {
            this.dofEnabled = !this.dofEnabled;

            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
            try
            {
                if (SingletonBehaviour<AtmosphereManager>.HasInstance)
                {
                    var am = SingletonBehaviour<AtmosphereManager>.Instance;
                    var aa = (AtmosphereApplier)am.GetType().GetField("_applier", flags).GetValue(am);
                    var dof = (DepthOfField)aa.GetType().GetField("_depthOfField", flags).GetValue(aa);
                    var postProcessLayer = Camera.main.GetComponent<PostProcessLayer>();

                    dof.enabled.value = this.dofEnabled;
                    SystemMessage.DisplayInfoText("Depth of field " + getEnabledText(this.dofEnabled) + ".");
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.Log("Crash in Depth of Field Plugin");
                UnityEngine.Debug.Log(ex.Message);
                UnityEngine.Debug.Log(ex.StackTrace);
                UnityEngine.Debug.Log(ex.InnerException);
                UnityEngine.Debug.Log(ex.Source);
            }
        }
        
        void Update()
        {
            if (ToggleLayer.Value.IsUp())
            {
                ToggleAll();
            }
            else if (ToggleDOF.Value.IsUp())
            {
                ToggleDoF();
            }
        }

        private bool allEnabled = true;
        private bool dofEnabled = true;
    }
}
