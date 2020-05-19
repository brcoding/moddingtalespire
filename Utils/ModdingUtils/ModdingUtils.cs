using UnityEngine.Rendering.PostProcessing;
using System.Reflection;
using TMPro;
using UnityEngine;
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
using Newtonsoft.Json.Serialization;
using Unity.Mathematics;
using UnityEngine.PlayerLoop;

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
        public CreatureKeyMoveBoardTool.Dir dir;
    }
    public static class ModdingUtils
    {
        private static BaseUnityPlugin parentPlugin;
        private static bool serverStarted = false;
        private static bool movingCreature = false;
        private static Queue<MoveAction> moveQueue = new Queue<MoveAction>();
        //public delegate string Command(params string[] args);
        public static Dictionary<string, Func<string[], string>> Commands = new Dictionary<string, Func<string[], string>>();

        static ModdingUtils()
        {
            Commands.Add("SelectNextPlayerControlled", SelectNextPlayerControlled);
            Commands.Add("SelectPlayerControlledByAlias", SelectPlayerControlledByAlias);
            Commands.Add("GetPlayerControlledList", GetPlayerControlledList);
            Commands.Add("GetCreatureList", GetCreatureList);
            Commands.Add("SetCreatureHp", SetCreatureHp);
            Commands.Add("SetCreatureStat", SetCreatureStat);
            Commands.Add("PlayEmote", PlayEmote);
            Commands.Add("Knockdown", Knockdown);
            Commands.Add("SelectCreatureByCreatureId", SelectCreatureByCreatureId);
            Commands.Add("MoveCreature", MoveCreature);
        }
        static string ExecuteCommand(string command)
        {
            try
            {
                UnityEngine.Debug.Log("Command: \"" + command + "\"");
                var parts = command.Split(' ');
                //UnityEngine.Debug.Log(parts[0]);
                //UnityEngine.Debug.Log(string.Join(",", Commands.Keys));
                return Commands[parts[0].Trim()].Invoke(string.Join(" ", parts.Skip(1)).Trim().Split(','));

            }
            catch
            {
                return new APIResponse("Failed to find command", "Unknown command").ToString();
            }
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
                byte[] buffer = new Byte[1024];
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(10);

                    while (true)
                    {
                        UnityEngine.Debug.Log("Waiting for a connection...");
                        while (true)
                        {
                            Socket socket = listener.Accept();
                            string data = "";
                            UnityEngine.Debug.Log("Connected");

                            int bytesRec = socket.Receive(buffer);
                            data += Encoding.ASCII.GetString(buffer, 0, bytesRec);

                            UnityEngine.Debug.Log("Command received : " + data);

                            byte[] cmdResult = Encoding.ASCII.GetBytes(ExecuteCommand(data));
                            
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

        private static string GetPlayerControlledList(string[] input)
        {
            return GetPlayerControlledList();
        }
 
        public struct CustomCreatureData
        {

            public string BoardAssetId;
            public string CreatureId;
            public string UniqueId;
            public Vector3 Position;
            public Quaternion Rotation;
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

        private static CustomCreatureData convertCreatureData(CreatureData cd)
        {
            // This is because NGuid does not serialize nicely
            CustomCreatureData ccd = new CustomCreatureData();
            ccd.Alias = cd.Alias;
            ccd.BoardAssetId = cd.BoardAssetId.ToString();
            ccd.CreatureId = cd.CreatureId.ToString();
            ccd.UniqueId = cd.UniqueId.ToString();
            ccd.Position = cd.Position;
            ccd.Rotation = cd.Rotation;
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

        private static string MoveCreature(string[] input)
        {
            return MoveCreature(input[0], input[1]);
        }

        private static void doDrop()
        {
            var ckmbt = SingletonBehaviour<BoardToolManager>.Instance.GetTool<CreatureKeyMoveBoardTool>();
        }

        public static float moveTime = 0f;
        public static MoveAction currentAction;
        public static CreatureBoardAsset SelectedAsset;
        public static Vector3 StartLocation;
        public static Vector3 DestLocation;
        public static CreatureNavigationController Nav = new CreatureNavigationController();
        public static AnimationCurve MoveCurve;
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
        private static void StartMove()
        {
            Debug.Log("Starting Move");
            SingletonBehaviour<BoardToolManager>.Instance.MovableHandle.Attach(SelectedAsset);
            SelectedAsset.Pickup();
            moveTime = 0;
            StartLocation = SelectedAsset.transform.position;
            var movePos = GetMoveVector(currentAction.dir);
            DestLocation = Explorer.RoundToCreatureGrid(StartLocation + movePos);
        }

        private static void EndMove()
        {
            Debug.Log("End Move");
            SelectedAsset.Drop();
            SingletonBehaviour<BoardToolManager>.Instance.MovableHandle.Detach();
            var creatureNGuid = new NGuid(currentAction.guid);
            CameraController.LookAtCreature(creatureNGuid);
        }
        private static void UpdateMove()
        {
            moveTime += Time.deltaTime * 4f;

            var currentPos = Vector3.Lerp(SelectedAsset.transform.position, DestLocation, moveTime);

            currentPos.y = Explorer.GetTileHeightAtLocation(currentPos, 0.4f, 6f) + 1.5f;///, float scanDistance = 2);
            SelectedAsset.RotateTowards(currentPos);
            SelectedAsset.MoveTo(currentPos);

        }
        public static void OnUpdate()
        {
            if (movingCreature)
            {
                if (moveTime > 1)
                {
                    EndMove();
                    movingCreature = false;
                } else
                {
                    UpdateMove();
                }
            }

            if (!movingCreature && moveQueue.Count() > 0)
            {
                moveTime = 0;
                currentAction = moveQueue.Dequeue();
                Debug.Log("Current Action: " + currentAction.guid + " dir: " + currentAction.dir);
                SelectCreatureByCreatureId(currentAction.guid);
                Debug.Log("Asset load:" + PhotonSimpleSingletonBehaviour<CreatureManager>.Instance.TryGetAsset(LocalClient.SelectedCreatureId, out SelectedAsset));
                StartMove();
                movingCreature = true;
            }

        }
        private static string MoveCreature(string creatureId, string direction)
        {
            CreatureKeyMoveBoardTool.Dir dir = (CreatureKeyMoveBoardTool.Dir)Enum.Parse(typeof(CreatureKeyMoveBoardTool.Dir), direction, true);
            moveQueue.Enqueue(new MoveAction { guid = creatureId, dir = dir });


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
                var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

                board.SetCreatureStatByIndex(new NGuid(creatureId), new CreatureStat(float.Parse(current), float.Parse(max)), int.Parse(statIdx));
                SingletonBehaviour<BoardToolManager>.Instance.GetTool<CreatureMenuBoardTool>().CallUpdate();
                return new APIResponse(String.Format("Set stat{0} to {1}:{2} for {3}", statIdx, current, max, creatureId)).ToString();
            }
            catch (Exception ex)
            {
                return new APIResponse(ex.Message + ex.StackTrace, "Could not set stat").ToString();
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
                var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

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
                //BoardSessionManager.PushMostRecentLocallySelectedCreature(creatureNGuid);
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
                        Debug.Log(LocalClient.SelectedCreatureId);
                        Debug.Log(creatureIds[i]);
                        Debug.Log(BoardSessionManager.Board.GetCreatureData(creatureIds[i]).Alias);
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
                        Debug.Log(LocalClient.SelectedCreatureId);
                        Debug.Log(creatureIds[i]);
                        Debug.Log(BoardSessionManager.Board.GetCreatureData(creatureIds[i]).Alias);
                        if (creatureIds[i] == LocalClient.SelectedCreatureId)
                        {
                            if (i + 1 < creatureIds.Length)
                            {
                                Debug.Log("One");
                                LocalClient.SelectedCreatureId = creatureIds[i + 1];
                                CameraController.LookAtCreature(creatureIds[i + 1]);
                                break;
                            }
                            Debug.Log("Zero");
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

        public static void Initialize(BaseUnityPlugin parentPlugin)
        {
            AppStateManager.UsingCodeInjection = true;
            ModdingUtils.parentPlugin = parentPlugin;
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartSocketServer();
        }

        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UnityEngine.Debug.Log("Loading Scene: " + scene.name);
            if (scene.name == "UI") {
                TextMeshProUGUI betaText = GetUITextByName("BETA");
                if (betaText)
                {
                    betaText.text = "INJECTED BUILD - unstable mods";
                }
            } else if (scene.name == "Login")
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
    }
}
