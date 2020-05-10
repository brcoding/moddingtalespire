using System;
using BepInEx;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace RemoveFogPlugin
{
    [BepInPlugin("org.d20armyknife.plugins.removefog", "Remove Fog Plug-In", "1.1.1.0")]
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
        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UnityEngine.Debug.Log("Loading Scene: " + scene.name);
            TextMeshProUGUI[] texts = FindObjectsOfType<TextMeshProUGUI>();
            for (int i = 0; i < texts.Length; i++)
            {
                if (scene.name == "UI" && texts[i].name == "BETA")
                {
                    texts[i].text = "INJECTED BUILD - unstable mods";
                }
                if (scene.name == "Login" && texts[i].name == "TextMeshPro Text")
                {
                    BepInPlugin bepInPlugin = (BepInPlugin)Attribute.GetCustomAttribute(this.GetType(), typeof(BepInPlugin));
                    if (texts[i].text.EndsWith("</size>"))
                    {
                        texts[i].text += "\n\nMods Currently Installed:\n";
                    }
                    texts[i].text += "\n" + bepInPlugin.Name + " - " + bepInPlugin.Version;
                }
            }
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
