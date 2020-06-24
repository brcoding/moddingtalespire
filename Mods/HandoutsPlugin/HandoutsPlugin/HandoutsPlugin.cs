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
using Bounce.Unmanaged;
using DataModel;
using BepInEx.Configuration;
using Unity.Mathematics;
using System.Linq;

namespace HandoutsPlugin
{
    [BepInPlugin("org.d20armyknife.plugins.handouts", "Handouts Plug-In", "1.0.0.0")]
    public class HandoutsPlugin: BaseUnityPlugin
    {
        // Configuration
        private ConfigEntry<KeyboardShortcut> ShowHandout { get; set; }
        private ConfigEntry<KeyboardShortcut> BringPlayersToMe { get; set; }
        private AcceptableValueList<String> lineOfSiteFocusAlias { get; set; }
        private ConfigEntry<string> lineLater { get; set; }
        private ConfigEntry<bool> DistanceLOS { get; set; }
        private ConfigEntry<float> LOSDistance { get; set; }

        public HandoutsPlugin()
        {

        }

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            DistanceLOS = Config.Bind("Line of Sight", "Custom Line of Sight", false);
            LOSDistance = Config.Bind("Line of Sight", "Distance", 10.0f);
            lineOfSiteFocusAlias = new AcceptableValueList<String>(new String[] { "Not Set" });
            lineLater = Config.Bind("Line of Sight", "Focused Character", "Not Set", new ConfigDescription("Select the alias to focus on. If not set all characters are in line of sight.", lineOfSiteFocusAlias));
            //lineLater = Config.Bind("test", "test", "Test", new ConfigDescription("Description", lineOfSiteFocusAlias));

            ShowHandout = Config.Bind("Hotkeys", "Handout Shortcut", new KeyboardShortcut(KeyCode.P, KeyCode.LeftControl));
            BringPlayersToMe = Config.Bind("Hotkeys", "Bring Players to my Board Shortcut", new KeyboardShortcut(KeyCode.M, KeyCode.LeftControl));

            instance = this;
            Logger.LogInfo("In Awake for Handouts Plug-in");

            UnityEngine.Debug.Log("Handouts Plug-in loaded");
            ModdingTales.ModdingUtils.Initialize(this, Logger);
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
            var board = BoardSessionManager.Board;
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
            //Debug.Log("LOS Enabled: " + DistanceLOS.Value);
            if (board != null && DistanceLOS.Value)
            {
                Dictionary<NGuid, CreatureData> creatures = (Dictionary<NGuid, CreatureData>)board.GetType().GetField("_creatures", flags).GetValue(board);
                foreach (KeyValuePair<NGuid, CreatureData> focusCreature in creatures)
                {
                    if (focusCreature.Value.Alias == lineLater.Value)
                    {
                        //Debug.Log("Found Focus");
                        foreach (KeyValuePair<NGuid, CreatureData> entry in creatures)
                        {
                            CreatureBoardAsset creatureBoardAsset;
                            if (PhotonSimpleSingletonBehaviour<CreatureManager>.Instance.TryGetAsset(entry.Value.CreatureId, out creatureBoardAsset))
                            {

                                float distance = Vector3.Distance(entry.Value.Position, focusCreature.Value.Position);
                                //Debug.Log("Distance: " + distance + " Alias: " + entry.Value.Alias + "Pos: " + entry.Value.Position + " FCPos: " + focusCreature.Value.Position);
                                if (distance < LOSDistance.Value)
                                {
                                    creatureBoardAsset.InLineOfSight = true;
                                }
                                else
                                {
                                    creatureBoardAsset.InLineOfSight = false;
                                }
                            }
                        }
                    }
                }
            }
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

            if (board != null)
            {
                //Debug.Log("onboard");
                Dictionary<NGuid, CreatureData> creatures = (Dictionary<NGuid, CreatureData>)board.GetType().GetField("_creatures", flags).GetValue(board);
                if (creatures.Count + 1 != lineOfSiteFocusAlias.AcceptableValues.Length && creatures.Count > 0)
                {
                    List<string> charAliases = new List<string>();
                    charAliases.Add("Not Set");
                    foreach (KeyValuePair<NGuid, CreatureData> focusCreature in creatures)
                    {
                        charAliases.Add(focusCreature.Value.Alias);
                    }
                    string tmpVal = lineLater.Value;
                    Config.Remove(lineLater.Definition);
                    lineOfSiteFocusAlias = new AcceptableValueList<String>(charAliases.ToArray());
                    lineLater = Config.Bind("Line of Sight", "Focused Character", "Not Set", new ConfigDescription("Select the alias to focus on. If not set all characters are in line of sight.", lineOfSiteFocusAlias));
                    lineLater.Value = tmpVal;
                }
            }

            if (BringPlayersToMe.Value.IsUp())
            {
                PartyManager.SummonAllOnlinePlayersToThisBoard(true, CameraController.Position);
            }

            if (ShowHandout.Value.IsUp())
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
