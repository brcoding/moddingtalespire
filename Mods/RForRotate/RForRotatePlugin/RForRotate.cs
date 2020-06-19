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
        private DateTime lastCheck = DateTime.Now;
        private List<string> seenMessages = new List<string>();
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
                Sprite sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                Vector2.one / 2);

                //GameObject NewObj = new GameObject("Handout"); //Create the GameObject
                //Image NewImage = NewObj.AddComponent<Image>(); //Add the Image Component script
                //NewImage.sprite = sprite; //Set the Sprite of the Image Component on the new GameObject
                //RectTransform canvas = GameObject.Find("Canvas").GetComponent<RectTransform>();
                //NewObj.GetComponent<RectTransform>().SetParent(canvas.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
                //NewObj.SetActive(true); //Activate the GameObject

                GameObject go = new GameObject("Handout");
                Image image = go.AddComponent<Image>();
                image.sprite = sprite;
                go.SetActive(true);
                //Transform t = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>()[0].transform.parent.parent.parent;
                //t.SetAsLastSibling();
                //go.transform.SetParent(, true);
                //Transform t = SingletonBehaviour<GUIManager>.Instance.transform;
                Canvas canvas = GUIManager.GetCanvas();
                go.transform.SetParent(canvas.transform, false);
                //go.transform.SetParent(t);
                //go.transform.SetAsLastSibling();
            }
        }

        public void CheckOOB()
        {
            Debug.Log(ModdingUtils.SendOOBMessage("{\"sessionid\": \"abc\", \"type\": \"get\"}"));
            OOBResponse[] json = JsonConvert.DeserializeObject<OOBResponse[]>(ModdingUtils.SendOOBMessage("{\"sessionid\": \"abc\", \"type\": \"get\"}"));
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
            if (Input.GetKeyUp(KeyCode.P))
            {
                //Debug.Log("Sending OOB");
                ModdingUtils.SendOOBMessage("{\"sessionid\": \"abc\", \"type\": \"put\", \"somekey\": \"somevalue\"}");
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
