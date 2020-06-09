using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

using UnityEngine.SceneManagement;

namespace RemoveDoFPlugin
{
    [BepInPlugin("org.d20armyknife.plugins.removedof", "Remove Depth Of Field Plug-In", "1.2.0.0")]
    [BepInProcess("TaleSpire.exe")]
    public class DeDoFIt: BaseUnityPlugin
    {

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            UnityEngine.Debug.Log("Remove Depth of Field Plug-in loaded");
            ModdingTales.ModdingUtils.Initialize(this);
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
                    

                    //PostProcessVolume dv = (PostProcessVolume)aa.GetType().GetField("_dayVolume", flags).GetValue(aa);

                    //dv.enabled = this.dofEnabled;
                    //PostProcessVolume nv = (PostProcessVolume)aa.GetType().GetField("_nightVolume", flags).GetValue(aa);

                    //nv.enabled = this.dofEnabled;

                    dof.enabled.value = this.dofEnabled;
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
            //Shader shader;// = new Shader();
            //shader = Shader.Find("Hidden/ChromaticAberration");
            //Material material = new Material(shader);
            //if (shader != null)
            //{
            //    material.SetFloat("_ChromaticAberration", 0f);
            //}
            if (Input.GetKeyUp(KeyCode.H))
            {
                ToggleDoF();
            }
        }

        private bool dofEnabled = true;
    }
}
