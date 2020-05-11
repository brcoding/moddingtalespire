using BepInEx;
using ModdingTales;
using System;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtilities
{
    [BepInPlugin("org.generic.plugins.setinjectionflag", "Set Injection Flag Plugin", "1.1.0.0")]
    [BepInProcess("TaleSpire.exe")]
    class SetInjectionFlag : BaseUnityPlugin
    {
        void Awake()
        {
            UnityEngine.Debug.Log("SetInjectionFlag Plug-in loaded");
            ModdingUtils.Initialize(this);
            //StartServer();
            StartSocketServer();
        }

        static void StartSocketServer()
        {
            Task.Factory.StartNew(() =>
            {
                UnityEngine.Debug.Log("Port: ");
                byte[] buffer = new Byte[1024];
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 999);
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(10);

                    while (true)
                    {
                        UnityEngine.Debug.Log("Waiting for a connection...");
                        Socket socket = listener.Accept();
                        string data = "";
                        UnityEngine.Debug.Log("Connected");
                        while (true)
                        {
                            int bytesRec = socket.Receive(buffer);
                            data += Encoding.ASCII.GetString(buffer, 0, bytesRec);
                            if (data.IndexOf("\n") > -1)
                            {
                                break;
                            }
                        }

                        UnityEngine.Debug.Log("Text received : " + data);
                        byte[] msg = Encoding.ASCII.GetBytes(data);

                        socket.Send(msg);
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }

                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e.ToString());
                }

            });
        }
        static void StartServer()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        var server = new NamedPipeServerStream("ModdingTales");
                        server.WaitForConnection();
                        StreamReader reader = new StreamReader(server);
                        StreamWriter writer = new StreamWriter(server);
                        while (true)
                        {
                            //UnityEngine.Debug.Log("namedpipe trying to read");

                            var line = reader.ReadLine();
                            if (line.Trim() != "")
                            {
                                UnityEngine.Debug.Log("namedpipe: " + line);
                            }
                            //writer.WriteLine(String.Join("", line.Reverse()));
                            //writer.WriteLine("This is the server");
                            //UnityEngine.Debug.Log("Wrote Line");
                            //writer.Flush();
                            //UnityEngine.Debug.Log("Flushed");

                        }
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.Log("Caught Exception");
                    UnityEngine.Debug.Log(ex);
                    //break;                    // When client disconnects
                }
        });
        }
    }
}
