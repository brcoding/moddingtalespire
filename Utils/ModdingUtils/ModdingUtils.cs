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
    public class PropertyRenameAndIgnoreSerializerContractResolver : DefaultContractResolver
    {
        private readonly Dictionary<Type, HashSet<string>> _ignores;
        private readonly Dictionary<Type, Dictionary<string, string>> _renames;

        public PropertyRenameAndIgnoreSerializerContractResolver()
        {
            _ignores = new Dictionary<Type, HashSet<string>>();
            _renames = new Dictionary<Type, Dictionary<string, string>>();
        }

        public void IgnoreProperty(Type type, params string[] jsonPropertyNames)
        {
            if (!_ignores.ContainsKey(type))
                _ignores[type] = new HashSet<string>();

            foreach (var prop in jsonPropertyNames)
                _ignores[type].Add(prop);
        }

        public void RenameProperty(Type type, string propertyName, string newJsonPropertyName)
        {
            if (!_renames.ContainsKey(type))
                _renames[type] = new Dictionary<string, string>();

            _renames[type][propertyName] = newJsonPropertyName;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (IsIgnored(property.DeclaringType, property.PropertyName))
            {
                property.ShouldSerialize = i => false;
                property.Ignored = true;
            }

            if (IsRenamed(property.DeclaringType, property.PropertyName, out var newJsonPropertyName))
                property.PropertyName = newJsonPropertyName;

            return property;
        }

        private bool IsIgnored(Type type, string jsonPropertyName)
        {
            if (!_ignores.ContainsKey(type))
                return false;

            return _ignores[type].Contains(jsonPropertyName);
        }

        private bool IsRenamed(Type type, string jsonPropertyName, out string newJsonPropertyName)
        {
            Dictionary<string, string> renames;

            if (!_renames.TryGetValue(type, out renames) || !renames.TryGetValue(jsonPropertyName, out newJsonPropertyName))
            {
                newJsonPropertyName = null;
                return false;
            }

            return true;
        }
    }
    public static class ModdingUtils
    {
        private static BaseUnityPlugin parentPlugin;
        private static bool serverStarted = false;

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
