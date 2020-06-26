using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Reflection;
using TMPro;
using BepInEx;
using UnityEngine.SceneManagement;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Bounce.Unmanaged;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using DataModel;
using Unity.Mathematics;
using System.Collections;
using BepInEx.Logging;

namespace ModdingTales
{
    public class PlayerControlled
    {
        public string Alias { get; set; }
    }
    public class APIResponse
    {
        public APIResponse(string message)
        {
            Message = message;
        }
        public APIResponse(string errorMessage, string message)
        {
            Message = message;
            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; set; }
        public string Message { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public struct MoveAction
    {
        public string guid;
        public CreatureBoardAsset asset;
        public Vector3 StartLocation;
        public Vector3 DestLocation;
        public CreatureKeyMoveBoardTool.Dir dir;
        public float steps;
        public float moveTime;
        public MovableHandle handle;
        public bool useHandle;
    }
    public class F3
    {
        public F3(float3 f)
        {
            this.x = f.x;
            this.y = f.y;
            this.z = f.z;
        }
        public F3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public float x;
        public float y;
        public float z;
    }

    public class Euler
    {
        public Euler(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public float x;
        public float y;
        public float z;
        public float w;
    }
    public struct SayTextData
    {
        public string CreatureId;
        public string Text;
    }

    public class CustomCreatureStat
    {
        public CustomCreatureStat(float value, float max)
        {
            this.Value = value;
            this.Max = max;
        }
        public float Value;
        public float Max;
    }

    public struct SlabData
    {
        public F3 Position;
        public string SlabText;
    }
    public static class ModdingUtils
    {
        private static BaseUnityPlugin parentPlugin;
        private static ManualLogSource parentLogger;
        private static bool serverStarted = false;
        private static Queue<BoardInfo> boardsToLoad = new Queue<BoardInfo>();
        //private static bool movingCreature = false;
        //private static Queue<MoveAction> moveQueue = new Queue<MoveAction>();
        //public delegate string Command(params string[] args);
        public static Dictionary<string, Func<string[], string>> Commands = new Dictionary<string, Func<string[], string>>();
        public static List<MoveAction> currentActions = new List<MoveAction>();
        public static Queue<SayTextData> sayTextQueue = new Queue<SayTextData>();
        public static Queue<SlabData> slabQueue = new Queue<SlabData>();
        public static string[] customStatNames = new string[4] { "", "", "", "" };
        public static string slabSizeSlab = "";
        public static bool slabSizeResponse;
        public static float3 slabSize;
        public static Copied beingCopied;

        static ModdingUtils()
        {
            Commands.Add("SelectNextPlayerControlled", SelectNextPlayerControlled);
            Commands.Add("SelectPlayerControlledByAlias", SelectPlayerControlledByAlias);
            Commands.Add("GetPlayerControlledList", GetPlayerControlledList);
            Commands.Add("GetCreatureList", GetCreatureList);
            Commands.Add("SetCreatureHp", SetCreatureHp);
            Commands.Add("SetCreatureStat", SetCreatureStat);
            Commands.Add("GetCreatureStats", GetCreatureStats);
            Commands.Add("PlayEmote", PlayEmote);
            Commands.Add("Knockdown", Knockdown);
            Commands.Add("SelectCreatureByCreatureId", SelectCreatureByCreatureId);
            Commands.Add("MoveCreature", MoveCreature);
            Commands.Add("GetCameraLocation", GetCameraLocation);
            Commands.Add("MoveCamera", MoveCamera);
            Commands.Add("SetCameraHeight", SetCameraHeight);
            Commands.Add("RotateCamera", RotateCamera);
            Commands.Add("ZoomCamera", ZoomCamera);
            Commands.Add("TiltCamera", TiltCamera);
            Commands.Add("SayText", SayText);
            Commands.Add("SetCustomStatName", SetCustomStatName);
            Commands.Add("CreateSlab", CreateSlab);
            Commands.Add("GetSlabSize", GetSlabSize);
            Commands.Add("GetCreatureAssets", GetCreatureAssets);
            Commands.Add("AddCreature", AddCreature);
            Commands.Add("KillCreature", KillCreature);
            Commands.Add("GetBoards", GetBoards);
            Commands.Add("GetCurrentBoard", GetCurrentBoard);
            Commands.Add("LoadBoard", LoadBoard);
        }
        static string ExecuteCommand(string command)
        {
            try
            {
                //UnityEngine.Debug.Log("Command: \"" + command + "\"");
                var parts = command.Split(' ');
                UnityEngine.Debug.Log(parts[0].Trim());
                //UnityEngine.Debug.Log(string.Join(" ", parts.Skip(1)).Trim().Split(','));
                return Commands[parts[0].Trim()].Invoke(string.Join(" ", parts.Skip(1)).Trim().Split(','));

            }
            catch (Exception ex)
            {
                return new APIResponse(ex.Message + ex.StackTrace, "Unknown command").ToString();
            }
        }

        public static byte[] ReceiveAll(this Socket socket)
        {
            var buffer = new List<byte>();
            int sleeps = 0;
            while (socket.Available == 0 && sleeps < 3000)
            {
                System.Threading.Thread.Sleep(1);
                sleeps++;
            }
            while (socket.Available > 0)
            {
                var currByte = new Byte[1];
                var byteCounter = socket.Receive(currByte, currByte.Length, SocketFlags.None);

                if (byteCounter.Equals(1))
                {
                    buffer.Add(currByte[0]);
                }
            }

            return buffer.ToArray();
        }
        private static void StartSocketServer()
        {
            if (serverStarted)
            {
                return;
            }
            serverStarted = true;
            Task.Factory.StartNew(() =>
            {
                int port = 999;
                UnityEngine.Debug.Log("Starting Modding Socket at 127.0.0.1 and Port: " + port);
                //byte[] buffer = new Byte[4096];
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(10);

                    while (true)
                    {
                        //UnityEngine.Debug.Log("Waiting for a connection...");
                        while (true)
                        {
                            Socket socket = listener.Accept();
                            string data = "";
                            //UnityEngine.Debug.Log("Connected");
                            byte[] buffer = ReceiveAll(socket);
                            int bytesRec = buffer.Length;
                            data += Encoding.UTF8.GetString(buffer, 0, bytesRec);

                            //Debug.Log("Command received : " + data);
                            //Debug.Log("Buffer Len:" + bytesRec.ToString());

                            byte[] cmdResult = Encoding.UTF8.GetBytes(ExecuteCommand(data));
                            //Debug.Log("Command Result:" + Encoding.UTF8.GetString(cmdResult, 0, cmdResult.Length));
                            socket.Send(cmdResult);
                            
                            socket.Shutdown(SocketShutdown.Both);
                            socket.Close();
                        }

                    }

                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e.ToString());
                }

            });
        }


        public static string SendOOBMessage(string message, AsyncCallback callback = null)
        {
            int port = 887;

            IPHostEntry ipHostInfo = Dns.GetHostEntry("d20armyknife.com");
            IPEndPoint localEndPoint = new IPEndPoint(ipHostInfo.AddressList[0], port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(localEndPoint);
            byte[] byteData = Encoding.UTF8.GetBytes(message);
            if (callback != null)
            {
                socket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(callback), socket);
                return "";
            } else
            {
                socket.Send(byteData);
                byte[] buffer = ReceiveAll(socket);
                int bytesRec = buffer.Length;
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRec);

                //Debug.Log("OOB Response: " + data);
                //Debug.Log("Buffer Len:" + bytesRec.ToString());
                return data;
            }
        }

        private static string GetPlayerControlledList(string[] input)
        {
            return GetPlayerControlledList();
        }
 
        public struct CustomBoardAssetData
        {
            public string GUID;
            public string boardAssetName;
            public string boardAssetDesc;
            public string boardAssetType;
            public string seachString;
            public string boardAssetGroup;
        }
        public struct CustomCreatureData
        {

            public string BoardAssetId;
            public string CreatureId;
            public string UniqueId;
            public F3 Position;
            public Euler Rotation;
            public string Alias;
            public string AvatarThumbnailUrl;
            public Color[] Colors;
            public CreatureStat Hp;
            public string Inventory;
            public CreatureStat Stat0;
            public CreatureStat Stat1;
            public CreatureStat Stat2;
            public CreatureStat Stat3;
            public bool TorchState;
            public bool ExplicitlyHidden;
        }

        public struct CustomBoardInfo
        {
            public string BoardId;
            public string BoardName;
            public string CampaignId;
            public string BoardDesc;
        }
        private static CustomCreatureData convertCreatureData(CreatureData cd)
        {
            // This is because NGuid does not serialize nicely
            CustomCreatureData ccd = new CustomCreatureData();
            ccd.Alias = cd.Alias;
            ccd.BoardAssetId = cd.BoardAssetId.ToString();
            ccd.CreatureId = cd.CreatureId.ToString();
            ccd.UniqueId = cd.UniqueId.ToString();
            //ccd.Position = cd.Position;
            ccd.Position = new F3(cd.Position.x, cd.Position.y, cd.Position.z);
            ccd.Rotation = new Euler(cd.Rotation.value.x, cd.Rotation.value.y, cd.Rotation.value.z, cd.Rotation.value.w);
            ccd.Alias = cd.Alias;
            ccd.AvatarThumbnailUrl = cd.AvatarThumbnailUrl;
            ccd.Colors = cd.Colors;
            ccd.Hp = cd.Hp;
            ccd.Inventory = cd.Inventory;
            ccd.Stat0 = cd.Stat0;
            ccd.Stat1 = cd.Stat1;
            ccd.Stat2 = cd.Stat2;
            ccd.Stat3 = cd.Stat3;
            ccd.TorchState = cd.TorchState;
            ccd.ExplicitlyHidden = cd.ExplicitlyHidden;
            return ccd;
        }

        
        private static string GetCurrentBoard(string[] input)
        {
            return GetCurrentBoard();
        }

        public static string GetCurrentBoard()
        {
            return JsonConvert.SerializeObject(new CustomBoardInfo
            {
                BoardId = BoardSessionManager.CurrentBoardInfo.Id.ToString(),
                BoardName = BoardSessionManager.CurrentBoardInfo.BoardName,
                BoardDesc = BoardSessionManager.CurrentBoardInfo.Description,
                CampaignId = BoardSessionManager.CurrentBoardInfo.CampaignId.ToString()
            });
        }

        private static string LoadBoard(string[] input)
        {
            return LoadBoard(input[0]);
        }

        public static string LoadBoard(string boardId)
        {
            foreach (BoardInfo bi in CampaignSessionManager.MostRecentBoardList)
            {
                if (bi.Id.ToString() == boardId)
                {
                    boardsToLoad.Enqueue(bi);
                    return new APIResponse("Board load queued successfully").ToString();
                }
            }
            return new APIResponse("Board not found").ToString();
        }

        private static string GetBoards(string[] input)
        {
            return GetBoards();
        }

        public static string GetBoards()
        {
            //Debug.Log("Current Board Name: " + BoardSessionManager.CurrentBoardInfo.BoardName);
            List<CustomBoardInfo> lbi = new List<CustomBoardInfo>();
            foreach (BoardInfo bi in CampaignSessionManager.MostRecentBoardList)
            {
                lbi.Add(new CustomBoardInfo
                {
                    BoardId = BoardSessionManager.CurrentBoardInfo.Id.ToString(),
                    BoardName = BoardSessionManager.CurrentBoardInfo.BoardName,
                    BoardDesc = BoardSessionManager.CurrentBoardInfo.Description,
                    CampaignId = BoardSessionManager.CurrentBoardInfo.CampaignId.ToString()
                });
            }
            return JsonConvert.SerializeObject(lbi);
        }

        private static string GetSlabSize(string[] input)
        {
            return GetSlabSize(input[0]).Result;
        }
        
        public static async Task<string> GetSlabSize(string slabText)
        {
            int msPassed = 0;
            try
            {
                slabSizeResponse = false;
                slabSizeSlab = slabText;

                while (slabSizeResponse == false || msPassed > 1000)
                {
                    msPassed++;
                    await Task.Delay(1);
                }
                return JsonConvert.SerializeObject(new F3(slabSize));
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message + ex.StackTrace);
                return new APIResponse(ex.Message + ex.StackTrace, "Could not get slab size").ToString();
            }
        }

        private static string GetCreatureAssets(string[] input)
        {
            return GetCreatureAssets();
        }

        public static string GetCreatureAssets()
        {
            //BoardAssetDatabase.
            //private static BoardAssetLookup _lookup = new BoardAssetLookup();
            //new NGuid(this.boardAssetFolder.files[k].GUID)
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
            List<CustomBoardAssetData> cbad = new List<CustomBoardAssetData>();

            DictionaryList<NGuid, BoardAssetData> b = (DictionaryList<NGuid, BoardAssetData>)typeof(BoardAssetDatabase).GetField("_lookup", flags).GetValue(null);
            
            foreach (BoardAssetData bad in b.Values)
            {
                if (bad.boardAssetType != "CREATURE")
                {
                    continue;
                }
                cbad.Add(new CustomBoardAssetData { 
                    GUID = bad.GUID,  
                    boardAssetDesc = bad.boardAssetDesc,
                    boardAssetGroup = bad.boardAssetGroup,
                    boardAssetName = bad.boardAssetName,
                    boardAssetType = bad.boardAssetType,
                    seachString = bad.seachString
                });
            }
            return JsonConvert.SerializeObject(cbad);
            //return new APIResponse("Slab Paste Queued").ToString();
        }

        private static string KillCreature(string[] input)
        {
            return KillCreature(input[0]);
        }

        public static string KillCreature(string creatureId)
        {
            CreatureBoardAsset creatureBoardAsset;
            if (PhotonSimpleSingletonBehaviour<CreatureManager>.Instance.TryGetAsset(new NGuid(creatureId), out creatureBoardAsset))
            {
                Creature creature = creatureBoardAsset.Creature;
                creature.BoardAsset.RequestDelete();
                return new APIResponse("Delete request successful").ToString();
            }
            else
            {
                return new APIResponse("Failed to delete").ToString();
            }
        }
        
        private static string AddCreature(string[] input)
        {
            return AddCreature(input[0], input[1], input[2], input[3], input[4], input[5], input[6], 
                input[7], input[8], input[9], input[10], input[11], input[12], input[13], input[14], 
                input[15], input[16], input[17]);
        }
        public static CreaturePreviewBoardAsset spawnCreature = null;
        public static float3 spawnCreaturePos;

        public static string AddCreature(string nguid, string x, string y, string z, string scale, string alias, string hpcurr, string hpmax, string stat1curr, string stat1max,
            string stat2curr, string stat2max, string stat3curr, string stat3max, string stat4curr, string stat4max, string torch, string hidden)
        {
            float3 pos = math.float3(float.Parse(x), float.Parse(y), float.Parse(z));
            spawnCreaturePos = pos;

            CreatureData data = new CreatureData(new NGuid(nguid), NGuid.Empty,
                    math.float3(float.Parse(x), float.Parse(y), float.Parse(z)), quaternion.identity, float.Parse(scale), alias, null, null, null,
                    new CreatureStat(float.Parse(hpcurr), float.Parse(hpmax)), new CreatureStat(float.Parse(stat1curr), float.Parse(stat1max)),
                    new CreatureStat(float.Parse(stat2curr), float.Parse(stat2max)), new CreatureStat(float.Parse(stat3curr), float.Parse(stat3max)),
                    new CreatureStat(float.Parse(stat4curr), float.Parse(stat4max)), bool.Parse(torch), default(NGuid), bool.Parse(hidden));
            spawnCreature = CreaturePreviewBoardAsset.Spawn(data, pos, quaternion.identity);
            spawnCreature.Drop(math.float3(float.Parse(x), float.Parse(y), float.Parse(z)), float.Parse(y));

            return new APIResponse("Creature Added").ToString();
        }

        private static string CreateSlab(string[] input)
        {
            return CreateSlab(input[0], input[1], input[2], input[3]);
        }

        public static string CreateSlab(string x, string y, string z, string slabText)
        {
            Debug.Log("X:" + x + " y:" + y + " z:" + z + " Slab: " + slabText);
            slabQueue.Enqueue(new SlabData { Position = new F3(float.Parse(x), float.Parse(y), float.Parse(z)), SlabText = slabText });
            return new APIResponse("Slab Paste Queued").ToString();
        }

        private static string SayText(string[] input)
        {
            return SayText(input[0], input[1]);
        }

        public static string SayText(string creatureId, string text)
        {
            sayTextQueue.Enqueue(new SayTextData { CreatureId = creatureId, Text = text });
            return new APIResponse("Say queued successful").ToString();
        }

        private static string SetCameraHeight(string[] input)
        {
            return SetCameraHeight(input[0], input[1]);
        }

        public static string SetCameraHeight(string height, string absolute)
        {
            if (bool.Parse(absolute))
            {
                CameraController.MoveToHeight(float.Parse(height), true);
            }
            else
            {
                CameraController.MoveToHeight(float.Parse(height) + CameraController.CameraHeight, true);
            }
            return new APIResponse("Camera Move successful").ToString();
        }

        private static string TiltCamera(string[] input)
        {
            return TiltCamera(input[0], input[1]);
        }

        public static string TiltCamera(string tilt, string absolute)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

            Transform t = (Transform)CameraController.Instance.GetType().GetField("_tiltTransform", flags).GetValue(CameraController.Instance);

            // TODO: Move this to the update method so it can be done with animation instead of just a sudden jolt. Same with rotation.
            var babsolute = bool.Parse(absolute);
            if (babsolute)
            {
                t.localRotation = Quaternion.Euler(float.Parse(tilt), 0f, 0f);
            }
            else
            {
                t.localRotation = Quaternion.Euler(t.localRotation.eulerAngles.x + float.Parse(tilt), 0f, 0f);
            }
            return new APIResponse("Camera Move successful").ToString();
        }

        private static string RotateCamera(string[] input)
        {
            return RotateCamera(input[0], input[1]);
        }

        public static string RotateCamera(string rotation, string absolute)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

            Transform t = (Transform)CameraController.Instance.GetType().GetField("_camRotator", flags).GetValue(CameraController.Instance);

            var babsolute = bool.Parse(absolute);
            if (babsolute)
            {
                t.localRotation = Quaternion.Euler(0f, float.Parse(rotation), 0f);
            }
            else
            {
                t.localRotation = Quaternion.Euler(0f, float.Parse(rotation) + t.localRotation.eulerAngles.y, 0f);
            }
            return new APIResponse("Camera Move successful").ToString();
        }
        
        private static string SetCustomStatName(string[] input)
        {
            return SetCustomStatName(input[0], input[1]);
        }

        public static string SetCustomStatName(string index, string newName)
        {
            Debug.Log("Index " + index + " new name: " + newName);
            customStatNames[int.Parse(index) - 1] = newName;
            return new APIResponse("Stat Name Set").ToString();
        }
        private static string ZoomCamera(string[] input)
        {
            return ZoomCamera(input[0], input[1]);
        }

        public static string ZoomCamera(string zoom, string absolute)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
            Transform t = (Transform)CameraController.Instance.GetType().GetField("_camRotator", flags).GetValue(CameraController.Instance);
            float current_zoom = (float)CameraController.Instance.GetType().GetField("_targetZoomLerpValue", flags).GetValue(CameraController.Instance);
            float minFov = 0;
            float maxFov = 1;



            float newZoom;
            var babsolute = bool.Parse(absolute);
            if (babsolute)
            {
                newZoom = Mathf.Clamp(float.Parse(zoom), minFov, maxFov);
            } else
            {
                newZoom = Mathf.Clamp(current_zoom + float.Parse(zoom), minFov, maxFov);
            }
            CameraController.Instance.GetType().GetField("_targetZoomLerpValue", flags).SetValue(CameraController.Instance, newZoom);
            return new APIResponse("Camera Move successful").ToString();
        }

        private static string GetCameraLocation(string[] input)
        {
            return GetCameraLocation();
        }

        public static string GetCameraLocation()
        {
            return JsonConvert.SerializeObject(new F3(CameraController.Position.x, CameraController.CameraHeight, CameraController.Position.z));
        }
        private static string MoveCamera(string[] input)
        {
            return MoveCamera(input[0], input[1], input[2], input[3]);
        }

        public static string MoveCamera(string x, string y, string z, string absolute)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
            Transform t = (Transform)CameraController.Instance.GetType().GetField("_camRotator", flags).GetValue(CameraController.Instance);
            float zoom = (float)CameraController.Instance.GetType().GetField("_targetZoomLerpValue", flags).GetValue(CameraController.Instance);

            var babsolute = bool.Parse(absolute);
            if (babsolute)
            {
                //CameraController.MoveToPosition(newPos, true);
                CameraController.LookAtTargetXZ(new Vector2(float.Parse(x), float.Parse(z)));
            }
            else
            {
                //CameraController.MoveToPosition(newPos + (float3)CameraController.Position, true);
                CameraController.LookAtTargetXZ(new Vector2(float.Parse(x) + CameraController.Position.x, float.Parse(z) + CameraController.Position.z));
            }
            return new APIResponse("Camera Move successful").ToString();
        }
        private static string MoveCreature(string[] input)
        {
            return MoveCreature(input[0], input[1], input[2], input[3]);
        }


        private static Vector3 GetMoveVector(CreatureKeyMoveBoardTool.Dir dir)
        {
            Vector3 newPosition = Vector3.zero;
            switch (dir)
            {
                case CreatureKeyMoveBoardTool.Dir.FORWARD:
                    newPosition = CameraController.Forward;
                    break;
                case CreatureKeyMoveBoardTool.Dir.BACKWARDS:
                    newPosition = -CameraController.Forward;
                    break;
                case CreatureKeyMoveBoardTool.Dir.LEFT:
                    newPosition = -CameraController.Right;
                    break;
                case CreatureKeyMoveBoardTool.Dir.RIGHT:
                    newPosition = CameraController.Right;
                    break;
            }
            float num = -1f;
            Vector3[] array = new Vector3[]
            {
                Vector3.forward,
                -Vector3.forward,
                Vector3.right,
                -Vector3.right
            };
            Vector3 b = Vector3.forward;
            for (int i = 0; i < array.Length; i++)
            {
                float num2 = Vector3.Dot(newPosition, array[i]);
                if (num2 > num)
                {
                    num = num2;
                    b = array[i];
                }
            }
            newPosition = b;
            return newPosition;
        }
        private static void StartMove(MoveAction ma)
        {
            PhotonSimpleSingletonBehaviour<CreatureManager>.Instance.TryGetAsset(new NGuid(ma.guid), out ma.asset);
            if (ma.useHandle)
            {
                ma.handle = MovableHandle.Spawn();
                ma.handle.Attach(ma.asset);
            }
            ma.asset.Pickup();
            ma.moveTime = 0;
            ma.StartLocation = ma.asset.transform.position;
            //Debug.Log("Start: " + ma.StartLocation);
            var movePos = GetMoveVector(ma.dir) * ma.steps;
            //Debug.Log("MoveVec: " + movePos);
            ma.DestLocation = Explorer.RoundToCreatureGrid(ma.StartLocation + movePos);
            //Debug.Log("Dest: " + ma.DestLocation);
            currentActions.Add(ma);
        }

        private static void UpdateMove()
        {
            RaycastHit[] creatureHits = new RaycastHit[10];
            for (int i = currentActions.Count() - 1; i >= 0; i--)
            {

                //Debug.Log("Updating: " + i);
                //Debug.Log(currentActions[i]);
                MoveAction ma = currentActions[i];
                ma.moveTime += (Time.deltaTime / (ma.steps * 0.6f));
                currentActions[i] = ma;

                Ray ray = new Ray(currentActions[i].asset.transform.position + new Vector3(0f, 1.5f, 0f), -Vector3.up);
                int num = Physics.SphereCastNonAlloc(ray, 0.32f, creatureHits, 2f, 2048);
                Debug.DrawRay(ray.origin, ray.direction * 10f, Color.white);
                float num2 = Explorer.GetTileHeightAtLocation(currentActions[i].asset.transform.position, 0.4f, 4f);

                var currentPos = Vector3.Lerp(currentActions[i].asset.transform.position, currentActions[i].DestLocation, currentActions[i].moveTime);

                //currentPos.y = Explorer.GetTileHeightAtLocation(currentPos, 0.4f, 4f) + 0.05f;// + 1.5f;
                currentActions[i].asset.RotateTowards(currentPos);
                currentActions[i].asset.MoveTo(currentPos);
                //Debug.Log("Drop check:" + currentPos + " dest:" + currentActions[i].DestLocation);
                if (currentPos.x == currentActions[i].DestLocation.x && currentPos.z == currentActions[i].DestLocation.z)
                {
                    //Debug.Log("Dropping");
                    currentActions[i].asset.Drop(currentPos, currentPos.y);
                    if (currentActions[i].useHandle)
                    {
                        currentActions[i].handle.Detach();
                        PhotonNetwork.Destroy(currentActions[i].handle.gameObject);
                    }
                    var creatureNGuid = new NGuid(currentActions[i].guid);
                    //CameraController.LookAtCreature(creatureNGuid);
                    currentActions.RemoveAt(i);
                }

            }
        }

        public static void UpdateSpeech()
        {
            try
            {
                var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
                while (sayTextQueue.Count > 0)
                {
                    var sayText = sayTextQueue.Dequeue();
                    CreatureBoardAsset creatureBoardAsset;
                    if (PhotonSimpleSingletonBehaviour<CreatureManager>.Instance.TryGetAsset(new NGuid(sayText.CreatureId), out creatureBoardAsset))
                    {
                        Creature creature = creatureBoardAsset.Creature;
                        creature.Speak(sayText.Text);
                    }
                }

                var tbm = SingletonBehaviour<TextBubbleManager>.Instance;
                List<TextBubble> bubbles = (List<TextBubble>)tbm.GetType().GetField("_bubblesInUse", flags).GetValue(tbm);
                foreach (var bubble in bubbles)
                {
                    TextMeshProUGUI bubbleText = (TextMeshProUGUI)bubble.GetType().GetField("_text", flags).GetValue(bubble);
                    bubbleText.GetComponent<RectTransform>().localPosition = new Vector2(-(bubbleText.preferredWidth / 2), bubbleText.GetComponent<RectTransform>().localPosition.y);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message + ex.StackTrace);
            }
        }

        public static void UpdateCustomStatNames()
        {
            TextMeshProUGUI stat;
            for (int i = 0; i < customStatNames.Length; i++)
            {

                if (customStatNames[i] != "")
                {
                    //Debug.Log("Inside statnames");
                    //Debug.Log("Stat " + (i + 1));
                    stat = GetUITextContainsString("Stat " + (i + 1));
                    if (stat)
                    {
                        //Debug.Log("Found stat " + i);
                        stat.text = customStatNames[i];
                    }
                }
            }
        }

        public static void UpdateSlab()
        {
            while (slabQueue.Count > 0)
            {
                var slabToPaste = slabQueue.Dequeue();
                Debug.Log("Slab:");
                Debug.Log(slabToPaste);
                if (BoardSessionManager.Board.PushStringToTsClipboard(slabToPaste.SlabText) == PushStringToTsClipboardResult.Success)
                {
                    Copied mostRecentCopied_LocalOnly = BoardSessionManager.Board.GetMostRecentCopied_LocalOnly();
                    if (mostRecentCopied_LocalOnly != null)
                    {
                        Debug.Log("X:" + slabToPaste.Position.x + " y:" + slabToPaste.Position.x + " z:" + slabToPaste.Position.z + " Slab: " + slabToPaste.SlabText);
                        BoardSessionManager.Board.PasteCopied(new Vector3(slabToPaste.Position.x, slabToPaste.Position.y, slabToPaste.Position.z), 0, 0UL);
                        //BoardSessionManager.Board.PasteCopied(new Vector3(slabToPaste.Position.x, slabToPaste.Position.y, slabToPaste.Position.z), 0, 0UL);
                    }
                }
            }
        }

        public static void GetSlabSize()
        {
            if (slabSizeSlab != "")
            {
                var slabToPaste = slabSizeSlab;// slabQueue.Dequeue();
                slabSizeSlab = "";

                if (BoardSessionManager.Board.PushStringToTsClipboard(slabToPaste) == PushStringToTsClipboardResult.Success)
                {

                    Copied mostRecentCopied_LocalOnly = BoardSessionManager.Board.GetMostRecentCopied_LocalOnly();
                    if (mostRecentCopied_LocalOnly != null)
                    {
                        slabSize = mostRecentCopied_LocalOnly.Bounds.size;
                        slabSizeResponse = true;
                    }
                } else
                {
                    slabSize = new float3(0, 0, 0);
                    slabSizeResponse = true;
                }
            }
        }
        private static void UpdateBoardLoad()
        {
            if (boardsToLoad.Count > 0)
            {
                BoardInfo bi = boardsToLoad.Dequeue();
                SingletonBehaviour<BoardSaverManager>.Instance.Load(bi);
            }
        }
        // This only needs to be called from update if you are using the socket API or MoveCharacter calls.
        public static void OnUpdate()
        {
            if (spawnCreature != null)
            {
                CreatureManager.AddCreature(spawnCreature.CreatureData, spawnCreaturePos, quaternion.identity);
                spawnCreature.DeleteAsset();
                spawnCreature = null;
            }
            UpdateMove();
            UpdateSpeech();
            UpdateCustomStatNames();
            UpdateSlab();
            GetSlabSize();
            UpdateBoardLoad();
        }
        private static string MoveCreature(string creatureId, string direction, string steps, string carryCreature)
        {
            bool useHandle = false;
            if (carryCreature != "")
            {
                useHandle = bool.Parse(carryCreature);
            }
            CreatureKeyMoveBoardTool.Dir dir = (CreatureKeyMoveBoardTool.Dir)Enum.Parse(typeof(CreatureKeyMoveBoardTool.Dir), direction, true);
            StartMove(new MoveAction { guid = creatureId, dir = dir, steps = float.Parse(steps), useHandle = useHandle });

            return new APIResponse("Move successful").ToString();
        }
        private static string Knockdown(string[] input)
        {
            return Knockdown(input[0]);
        }

        private static string Knockdown(string creatureId)
        {
            CreatureBoardAsset creatureBoardAsset;
            if (PhotonSimpleSingletonBehaviour<CreatureManager>.Instance.TryGetAsset(new NGuid(creatureId), out creatureBoardAsset))
            {
                Creature creature = creatureBoardAsset.Creature;
                creature.Knockdown();
                return new APIResponse("Emote successful").ToString(); ;
            }
            else
            {
                return new APIResponse("Failed to emote").ToString();
            }
        }
        private static string PlayEmote(string[] input)
        {
            return PlayEmote(input[0], input[1]);
        }

        private static string PlayEmote(string creatureId, string emote)
        {
            CreatureBoardAsset creatureBoardAsset;
            if (PhotonSimpleSingletonBehaviour<CreatureManager>.Instance.TryGetAsset(new NGuid(creatureId), out creatureBoardAsset))
            {
                Creature creature = creatureBoardAsset.Creature;
                creature.PlayEmote(emote);
                return new APIResponse("Emote successful").ToString(); ;
            }
            else
            {
                return new APIResponse("Failed to emote").ToString();
            }
        }
        private static string SetCreatureStat(string[] input)
        {
            return SetCreatureStat(input[0], input[1], input[2], input[3]);
        }
        public static string SetCreatureStat(string creatureId, string statIdx, string current, string max)
        {
            try
            {
                List<CustomCreatureData> allCreatures = new List<CustomCreatureData>();

                var board = BoardSessionManager.Board;

                board.SetCreatureStatByIndex(new NGuid(creatureId), new CreatureStat(float.Parse(current), float.Parse(max)), int.Parse(statIdx) - 1);
                SingletonBehaviour<BoardToolManager>.Instance.GetTool<CreatureMenuBoardTool>().CallUpdate();
                return new APIResponse(String.Format("Set stat{0} to {1}:{2} for {3}", statIdx, current, max, creatureId)).ToString();
            }
            catch (Exception ex)
            {
                return new APIResponse(ex.Message + ex.StackTrace, "Could not set stat").ToString();
            }
        }

        private static string GetCreatureStats(string[] input)
        {
            return GetCreatureStats(input[0]);
        }
        public static string GetCreatureStats(string creatureId)
        {
            try
            {
                CreatureData cd = BoardSessionManager.Board.GetCreatureData(new NGuid(creatureId));
                List<CustomCreatureStat> creatureStats = new List<CustomCreatureStat>();
                creatureStats.Add(new CustomCreatureStat(cd.Hp.Value, cd.Hp.Max));
                for (int i = 0; i < 5; i++)
                {
                    CreatureStat stat = cd.StatByIndex(i);
                    creatureStats.Add(new CustomCreatureStat(stat.Value, stat.Max));
                }
                return new APIResponse(JsonConvert.SerializeObject(creatureStats)).ToString();
            }
            catch (Exception ex)
            {
                return new APIResponse(ex.Message + ex.StackTrace, "Could not get hp").ToString();
            }
        }
        private static string SetCreatureHp(string[] input)
        {
            return SetCreatureHp(input[0], input[1], input[2]);
        }
        public static string SetCreatureHp(string creatureId, string currentHp, string maxHp)
        {
            try
            {
                List<CustomCreatureData> allCreatures = new List<CustomCreatureData>();

                var board = BoardSessionManager.Board;

                board.SetCreatureStatByIndex(new NGuid(creatureId), new CreatureStat(float.Parse(currentHp), float.Parse(maxHp)), -1);
                SingletonBehaviour<BoardToolManager>.Instance.GetTool<CreatureMenuBoardTool>().CallUpdate();
                return new APIResponse(String.Format("Set HP to {0}:{1} for {2}", currentHp, maxHp, creatureId)).ToString();
            }
            catch (Exception ex)
            {
                return new APIResponse(ex.Message + ex.StackTrace, "Could not set hp").ToString();
            }
        }

        private static string GetCreatureList(string[] input)
        {
            return GetCreatureList();
        }
        public static string GetCreatureList()
        {
            try
            {
                List<CustomCreatureData> allCreatures = new List<CustomCreatureData>();

                var board = BoardSessionManager.Board;
                var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

                Dictionary<NGuid, CreatureData> creatures = (Dictionary<NGuid, CreatureData>)board.GetType().GetField("_creatures", flags).GetValue(board);
                foreach (KeyValuePair<NGuid, CreatureData> entry in creatures)
                {
                    allCreatures.Add(convertCreatureData(entry.Value));
                }
                return JsonConvert.SerializeObject(allCreatures);
            }
            catch (Exception ex)
            {
                return new APIResponse(ex.Message + ex.StackTrace, "Could not get creature list").ToString();
            }
        }

        public static string GetPlayerControlledList()
        {
            try
            {
                List<CustomCreatureData> playerControlled = new List<CustomCreatureData>();
                NGuid[] creatureIds;

                if (BoardSessionManager.Board.TryGetPlayerOwnedCreatureIds(LocalPlayer.Id.Value, out creatureIds))
                {
                    for (int i=0;i < creatureIds.Length;i++)
                    {
                        playerControlled.Add(convertCreatureData(BoardSessionManager.Board.GetCreatureData(creatureIds[i])));
                    }

                    return JsonConvert.SerializeObject(playerControlled);
                }
                else
                {
                    return "[]";
                }
            }
            catch (Exception ex)
            {
                return new APIResponse(ex.Message + ex.StackTrace, "Could not get player controlled list").ToString();
            }
        }

        private static string SelectCreatureByCreatureId(string[] input)
        {
            return SelectCreatureByCreatureId(input[0]);
        }

        public static string SelectCreatureByCreatureId(string guid)
        {
            try
            {
                var creatureNGuid = new NGuid(guid);
                if (LocalClient.SelectedCreatureId == creatureNGuid)
                {
                    return new APIResponse("Selected successfully").ToString();
                }
                LocalClient.SelectedCreatureId = creatureNGuid;
                CameraController.LookAtCreature(creatureNGuid);
                return new APIResponse("Selected successfully").ToString();
            }
            catch (Exception ex)
            {
                return new APIResponse(ex.Message, "Error selecting via nguid: " + guid).ToString();
            }
        }
        private static string SelectPlayerControlledByAlias(string[] input)
        {
            return SelectPlayerControlledByAlias(input[0]);
        }

        public static string SelectPlayerControlledByAlias(string alias)
        {
            try
            {

                NGuid[] creatureIds;
                if (BoardSessionManager.Board.TryGetPlayerOwnedCreatureIds(LocalPlayer.Id.Value, out creatureIds))
                {
                    int i = 0;
                    while (i < creatureIds.Length)
                    {
                        //Debug.Log(LocalClient.SelectedCreatureId);
                        //Debug.Log(creatureIds[i]);
                        //Debug.Log(BoardSessionManager.Board.GetCreatureData(creatureIds[i]).Alias);
                        if (BoardSessionManager.Board.GetCreatureData(creatureIds[i]).Alias.ToLower() == alias.ToLower())
                        {
                            LocalClient.SelectedCreatureId = creatureIds[i];
                            CameraController.LookAtCreature(creatureIds[i]);
                            break;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    return BoardSessionManager.Board.GetCreatureData(creatureIds[i]).Alias;
                }
                else
                {
                    return "[]";
                }
            }
            catch (Exception)
            {
                return new APIResponse("Failed to find alias", "Unable to select by alias: " + alias).ToString();
            }
        }

        private static string SelectNextPlayerControlled(string[] input)
        {
            return SelectNextPlayerControlled();
        }

        public static string SelectNextPlayerControlled()
        {
            try
            {

                NGuid[] creatureIds;
                if (BoardSessionManager.Board.TryGetPlayerOwnedCreatureIds(LocalPlayer.Id.Value, out creatureIds))
                {
                    int i = 0;
                    while (i < creatureIds.Length)
                    {
                        //Debug.Log(LocalClient.SelectedCreatureId);
                        //Debug.Log(creatureIds[i]);
                        //Debug.Log(BoardSessionManager.Board.GetCreatureData(creatureIds[i]).Alias);
                        if (creatureIds[i] == LocalClient.SelectedCreatureId)
                        {
                            if (i + 1 < creatureIds.Length)
                            {
                                LocalClient.SelectedCreatureId = creatureIds[i + 1];
                                CameraController.LookAtCreature(creatureIds[i + 1]);
                                break;
                            }
                            LocalClient.SelectedCreatureId = creatureIds[0];
                            CameraController.LookAtCreature(creatureIds[0]);
                            break;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    return BoardSessionManager.Board.GetCreatureData(creatureIds[i]).Alias;
                }
                else
                {
                    return "";
                }
            } catch (Exception ex)
            {
                return new APIResponse(ex.Message, "Unable to select next.").ToString();
            }
        }


        public static Slab GetSelectedSlab()
        {
            try
            {
                var test = (SlabBuilderBoardTool)SingletonBehaviour<SlabBuilderBoardTool>.Instance;
            }
            catch { }
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
            if (SingletonBehaviour<BoardToolManager>.HasInstance && (SingletonBehaviour<BoardToolManager>.Instance.IsCurrentTool<SlabBuilderBoardTool>()))
            {
                var sbbt = (SlabBuilderBoardTool)SingletonBehaviour<SlabBuilderBoardTool>.Instance;
                Slab slab = (Slab)sbbt.GetType().GetField("_slab", flags).GetValue(sbbt);
                return slab;
            } else
            {
                return null;
            }

        }

        public static TilePreviewBoardAsset GetSelectedTileAsset()
        {
            try {
                var test = (SingleBuilderBoardTool)SingletonBehaviour<SingleBuilderBoardTool>.Instance;
            } catch {}
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
            if (SingletonBehaviour<BoardToolManager>.HasInstance && (SingletonBehaviour<BoardToolManager>.Instance.IsCurrentTool<SingleBuilderBoardTool>()))
            {
                var btm = (SingleBuilderBoardTool)SingletonBehaviour<SingleBuilderBoardTool>.Instance;
                TilePreviewBoardAsset selectedAsset = (TilePreviewBoardAsset)btm.GetType().GetField("_selectedTileBoardAsset", flags).GetValue(btm);
                return selectedAsset;
            }
            else
            {
                return null;
            }

        }

        public static TextMeshProUGUI GetUITextContainsString(string contains)
        {
            TextMeshProUGUI[] texts = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>();
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].text.Contains(contains))
                {
                    return texts[i];
                }
            }
            return null;
        }
        public static TextMeshProUGUI GetUITextByName(string name)
        {
            TextMeshProUGUI[] texts = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>();
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].name == name)
                {
                    return texts[i];
                }
            }
            return null;
        }

        public static PostProcessLayer GetPostProcessLayer()
        {
            return Camera.main.GetComponent<PostProcessLayer>();
        }

        public static void Initialize(BaseUnityPlugin parentPlugin, ManualLogSource logger, bool startSocket=false)
        {
            AppStateManager.UsingCodeInjection = true;
            ModdingUtils.parentPlugin = parentPlugin;
            ModdingUtils.parentLogger = logger;
            parentLogger.LogInfo("Inside initialize");
            SceneManager.sceneLoaded += OnSceneLoaded;
            // By default do not start the socket server. It requires the caller to also call OnUpdate in the plugin update method.
            if (startSocket)
            {
                StartSocketServer();
            }
        }

        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            try
            {

            parentLogger.LogInfo("On Scene Loaded" + scene.name);
            UnityEngine.Debug.Log("Loading Scene: " + scene.name);
            if (scene.name == "UI") {
                TextMeshProUGUI betaText = GetUITextByName("BETA");
                if (betaText)
                {
                    betaText.text = "INJECTED BUILD - unstable mods";
                }
            } else 
            {
                TextMeshProUGUI modListText = GetUITextByName("TextMeshPro Text");
                if (modListText)
                {
                    BepInPlugin bepInPlugin = (BepInPlugin)Attribute.GetCustomAttribute(ModdingUtils.parentPlugin.GetType(), typeof(BepInPlugin));
                    if (modListText.text.EndsWith("</size>"))
                    {
                        modListText.text += "\n\nMods Currently Installed:\n";
                    }
                    modListText.text += "\n" + bepInPlugin.Name + " - " + bepInPlugin.Version;
                }
            }
            }
            catch (Exception ex)
            {
                parentLogger.LogFatal(ex);
            }
        }
    }
}
