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
using Bounce.Singletons;

namespace RForRotate
{
    [BepInPlugin("org.d20armyknife.plugins.handouts", "Handouts Plug-In", "1.0.0.0")]
    public class HandoutsPlugin: BaseUnityPlugin
    {

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            instance = this;
            Logger.LogInfo("In Awake for Handouts Plug-in");

            UnityEngine.Debug.Log("Handouts Plug-in loaded");
            ModdingTales.ModdingUtils.Initialize(this, Logger);
        }


        void SwitchBoard()
        {
            Debug.Log("Current Board Name: " + BoardSessionManager.CurrentBoardInfo.BoardName);
            foreach (BoardInfo bi in CampaignSessionManager.MostRecentBoardList)
            {
                Debug.Log("Board Item: " + bi.BoardName + " Id: " + bi.Id);
                //if (bi.BoardName == "der")
                //{
                //    SingletonBehaviour<BoardSaverManager>.Instance.Load(bi);
                //}

            }

        }


        
        public static HandoutsPlugin instance;
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
            public string boardLoadId { get; set; }
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
            
            ModdingUtils.SendOOBMessage("{\"sessionid\": \"" + CampaignSessionManager.Info.CampaignId + "\", \"type\": \"get\"}");
            OOBResponse[] json = JsonConvert.DeserializeObject<OOBResponse[]>(ModdingUtils.SendOOBMessage("{\"sessionid\": \"" + CampaignSessionManager.Info.CampaignId + "\", \"type\": \"get\"}"));
            foreach (OOBResponse item in json)
            {
                Debug.Log("Board Load ID: " + item.boardLoadId);
                if (!seenMessages.Contains(item.messageid))
                {
                    seenMessages.Add(item.messageid);
                    if (item.boardLoadId != null)
                    {
                        Debug.Log("Got OOB Board load.");
                        Debug.Log(ModdingUtils.LoadBoard(item.boardLoadId));
                        continue;
                    }
                    SystemMessage.DisplayInfoText(item.handoutUrl, 2.5f);
                    StartCoroutine(DownloadImage(item.handoutUrl));
                }
            }
        }

        void Update()
        {
            ModdingUtils.OnUpdate();
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
            if (Input.GetKeyUp(KeyCode.M))
            {
                ModdingUtils.SendOOBMessage("{\"sessionid\": \"" + CampaignSessionManager.Info.CampaignId + "\", \"boardLoadId\": \"" + BoardSessionManager.CurrentBoardInfo.Id + "\", \"type\": \"put\"}");
                //SwitchBoard();
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

            }
        }

    }
}
