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

namespace ModdingTales
{
    public static class ModdingUtils
    {
        private static BaseUnityPlugin parentPlugin;
        private static bool serverStarted = false;

        //public delegate string Command(params string[] args);
        public static Dictionary<string, Func<string[], string>> Commands = new Dictionary<string, Func<string[], string>>();

        static ModdingUtils()
        {
            Commands.Add("SelectNextPlayerControlled", SelectNextPlayerControlled);
            Commands.Add("SelectPlayerControlledByName", SelectPlayerControlledByName);
            Commands.Add("GetPlayerControlledList", GetPlayerControlledList);
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
                return "ERROR: Unknown command";
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
        public static string GetPlayerControlledList()
        {
            try
            {
                List<string> aliases = new List<string>();
                NGuid[] creatureIds;
                if (BoardSessionManager.Board.TryGetPlayerOwnedCreatureIds(LocalPlayer.Id.Value, out creatureIds))
                {
                    //int i = 0;
                    for (int i=0;i < creatureIds.Length;i++)
                    {
                        aliases.Add(BoardSessionManager.Board.GetCreatureData(creatureIds[i]).Alias);
                    }
                    return string.Join("|", aliases.ToArray());
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return "ERROR: Could not get player controlled list";
            }
        }

        private static string SelectPlayerControlledByName(string[] input)
        {
            return SelectPlayerControlledByName(input[0]);
        }

        public static string SelectPlayerControlledByName(string alias)
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
                    return "";
                }
            }
            catch (Exception)
            {
                return "ERROR: Unable to select by alias: " + alias;
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
            } catch (Exception)
            {
                return "ERROR: Unable to select next.";
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
