using System;
using System.Reflection;
using BepInEx;
using ModdingTales;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;

namespace RForRotate
{
    [BepInPlugin("org.d20armyknife.plugins.rforrotate", "Press R to Rotate Plug-In", "1.1.2.0")]
    public class RForRotatePlugin: BaseUnityPlugin
    {

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            instance = this;
            Logger.LogInfo("In Awake for R For Rotate Plug-in");

            UnityEngine.Debug.Log("R For Rotate Plug-in loaded");
            ModdingTales.ModdingUtils.Initialize(this, Logger);
        }
        
        private void RotateSelected(double amount)
        {
            try
            {
                // We have to first grab the CameraController._targetZoomLerpValue to disable the movement when our rotate is happening. This is the only way to prevent zooming while we are rotating
                var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
                if (SingletonBehaviour<BoardToolManager>.HasInstance && (SingletonBehaviour<BoardToolManager>.Instance.IsCurrentTool<SingleBuilderBoardTool>()))
                {
                    var btm = (SingleBuilderBoardTool)SingletonBehaviour<SingleBuilderBoardTool>.Instance;
                    float angle = (float)btm.GetType().GetField("_angle", flags).GetValue(btm);
                    angle = (float)(((double)angle + amount) % 360.0);
                    
                    // After adjusting don't forget to push it back to TaleSpire so they can keep track of it.
                    btm.GetType().GetField("_angle", flags).SetValue(btm, angle);
                    var selectedAsset = (TilePreviewBoardAsset)btm.GetType().GetField("_selectedTileBoardAsset", flags).GetValue(btm);
                    selectedAsset.Rotate(angle);

                    // Next we need to counter the heightPlaneOffset that is set in the CallUpdate when left shift is pressed
                    float heightPlaneOffset = (float)btm.GetType().GetField("heightPlaneOffset", flags).GetValue(btm);
                    heightPlaneOffset -= (float)((double)Input.mouseScrollDelta.y * (double)Time.deltaTime * 8.0);
                    btm.GetType().GetField("heightPlaneOffset", flags).SetValue(btm, heightPlaneOffset);
                }
                if (SingletonBehaviour<BoardToolManager>.HasInstance && (SingletonBehaviour<BoardToolManager>.Instance.IsCurrentTool<SlabBuilderBoardTool>()))
                {
                    var sbbt = (SlabBuilderBoardTool)SingletonBehaviour<SlabBuilderBoardTool>.Instance;
                    Slab slab = (Slab)sbbt.GetType().GetField("_slab", flags).GetValue(sbbt);
                    slab.Rotate90();
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.Log("Crash in r for rotate Plugin");
                UnityEngine.Debug.Log(ex.Message);
                UnityEngine.Debug.Log(ex.StackTrace);
                UnityEngine.Debug.Log(ex.InnerException);
                UnityEngine.Debug.Log(ex.Source);
            }
        }
        public static RForRotatePlugin instance;
        private DateTime lastCheck = DateTime.Now;
        private DateTime lastHandout = DateTime.Now;
        private List<string> seenMessages = new List<string>();
        private GameObject handout;

        //[JsonConverter(typeof(OOBResponseConverter))]
        public class OOBResponse
        {
            public string sessionid { get; set; }
            public string type { get; set; }
            public string handoutUrl { get; set; }
            public string messageid { get; set; }
            
        }

        //public Sprite sprite;

        IEnumerator DownloadImage(string MediaUrl)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
                Debug.Log(request.error);
            else
            {
                Debug.Log("Downloaded!");
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                float aspectRatio = ((float)texture.width / (float)texture.height);

                Sprite sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                Vector2.one / 2);

                if (handout)
                {
                    Destroy(handout);
                }
                handout = new GameObject("Handout");
                Image image = instance.handout.AddComponent<Image>();
                image.sprite = sprite;

                lastHandout = DateTime.Now;
                instance.handout.SetActive(true);

                Canvas canvas = GUIManager.GetCanvas();
                instance.handout.transform.SetParent(canvas.transform, false);

                float worldScreenHeight = (float)(Camera.main.orthographicSize * 2.0f);
                float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

                float Scale = (float)texture.width / worldScreenWidth * 0.15f;

                instance.handout.transform.localScale = new Vector3(Scale * aspectRatio, Scale, 1);
                lastHandout = DateTime.Now;
            }
        }

        public void CheckOOB()
        {
            
            Debug.Log(ModdingUtils.SendOOBMessage("{\"sessionid\": \"" + CampaignSessionManager.Info.CampaignId + "\", \"type\": \"get\"}"));
            OOBResponse[] json = JsonConvert.DeserializeObject<OOBResponse[]>(ModdingUtils.SendOOBMessage("{\"sessionid\": \"" + CampaignSessionManager.Info.CampaignId + "\", \"type\": \"get\"}"));
            foreach (OOBResponse item in json)
            {
                if (!seenMessages.Contains(item.messageid))
                {
                    seenMessages.Add(item.messageid);
                    SystemMessage.DisplayInfoText(item.handoutUrl, 2.5f);
                    StartCoroutine(DownloadImage(item.handoutUrl));

                    
                }
            }
        }

        void Update()
        {
            var diffInSeconds = (DateTime.Now - lastCheck).TotalSeconds;
            if (diffInSeconds > 3)
            {
                lastCheck = DateTime.Now;
                CheckOOB();
            }
            var diffInHandoutSeconds = (DateTime.Now - lastHandout).TotalSeconds;
            if (handout && handout.activeSelf && diffInHandoutSeconds > 10)
            {
                handout.SetActive(false);
            }
            if (Input.GetKeyUp(KeyCode.P))
            {
                SystemMessage.AskForTextInput("Handout URL", "Enter the Handout URL (PNG or JPG Image Only)", "OK", delegate (string name)
                {
                    ModdingUtils.SendOOBMessage("{\"sessionid\": \"" + CampaignSessionManager.Info.CampaignId + "\", \"type\": \"put\", \"handoutUrl\": \"" + name + "\"}");
                }, delegate
                {
                }, "Cancel", delegate
                {
                });

                //Debug.Log("Sending OOB");
                
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                    RotateSelected(90.0);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                RotateSelected(-90.0);
            }
            if (Input.GetKeyUp(KeyCode.R))
            {
                RotateSelected(90.0);
            }
        }

    }
}
