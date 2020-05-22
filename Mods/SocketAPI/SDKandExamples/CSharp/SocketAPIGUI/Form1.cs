using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketAPIGUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        public class CustomCreatureData
        {
            public string CreatureId { get; set; }
            public string Alias { get; set; }
        }

        public List<CustomCreatureData> creatureList = new List<CustomCreatureData>();
        public string SendMessage(string command, string[] msgparams)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[4*1024*1024];

            // Connect to a remote device.  
            try
            {
                int port = 999;
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(localEndPoint);

                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes(command + " " + string.Join(",", msgparams));

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);

                    sender.ReceiveTimeout = 3000;
                    // Receive the response from the remote device.  
                    string data = "";
                    int bytesRec = 0;

                    while (sender.Available == 0)
                    {
                        System.Threading.Thread.Sleep(1);
                    }
                    while (sender.Available > 0) 
                    { 
                        bytesRec = sender.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    }
                    //int bytesRec = sender.Receive(bytes, 0, sender.Available, SocketFlags.None);
                    //int bytesRec = sender.Receive(bytes);
                    //var data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    Console.WriteLine("Server responded bytes: {0} {1}", bytesRec, data);
                    // Release the socket.  
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    return data;
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    return "";
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                    return "";
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    return "";
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return "";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string data = SendMessage("GetCreatureList", new string[0]);
            dynamic json = JsonConvert.DeserializeObject(data);
            creatureList.Clear();
            foreach (dynamic item in json)
            {
                creatureList.Add(new CustomCreatureData { Alias = (string)item["Alias"], CreatureId = (string)item["CreatureId"] });
            }
            lbCreatureList.ValueMember = "CreatureId";
            lbCreatureList.DisplayMember = "Alias";
            lbCreatureList.DataSource = creatureList;// new BindingSource(creatureList, null);


        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (CustomCreatureData ccd in lbCreatureList.SelectedItems)
            {
                string data = SendMessage("MoveCreature", new string[] {ccd.CreatureId, "Forward", "1", cbCarry.Checked.ToString() });
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (CustomCreatureData ccd in lbCreatureList.SelectedItems)
            {
                string data = SendMessage("MoveCreature", new string[] { ccd.CreatureId, "Right", "1", cbCarry.Checked.ToString() });
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (CustomCreatureData ccd in lbCreatureList.SelectedItems)
            {
                string data = SendMessage("MoveCreature", new string[] { ccd.CreatureId, "Left", "1", cbCarry.Checked.ToString() });
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            foreach (CustomCreatureData ccd in lbCreatureList.SelectedItems)
            {
                string data = SendMessage("MoveCreature", new string[] { ccd.CreatureId, "Backwards", "1", cbCarry.Checked.ToString() });
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            foreach (CustomCreatureData ccd in lbCreatureList.SelectedItems)
            {
                string data = SendMessage("PlayEmote", new string[] { ccd.CreatureId, cbEmotes.SelectedItem.ToString() });
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            foreach (CustomCreatureData ccd in lbCreatureList.SelectedItems)
            {
                string data = SendMessage("SetCreatureHp", new string[] { ccd.CreatureId, hpcurr.Value.ToString(), hpmax.Value.ToString() });
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            foreach (CustomCreatureData ccd in lbCreatureList.SelectedItems)
            {
                string data = SendMessage("SetCreatureStat", new string[] { ccd.CreatureId, "1", st1curr.Value.ToString(), st1max.Value.ToString() });
            }

        }

        private void button9_Click(object sender, EventArgs e)
        {
            foreach (CustomCreatureData ccd in lbCreatureList.SelectedItems)
            {
                string data = SendMessage("SetCreatureStat", new string[] { ccd.CreatureId, "2", st2curr.Value.ToString(), st2max.Value.ToString() });
            }

        }

        private void button10_Click(object sender, EventArgs e)
        {
            foreach (CustomCreatureData ccd in lbCreatureList.SelectedItems)
            {
                string data = SendMessage("SetCreatureStat", new string[] { ccd.CreatureId, "3", st3curr.Value.ToString(), st3max.Value.ToString() });
            }

        }

        private void button11_Click(object sender, EventArgs e)
        {
            foreach (CustomCreatureData ccd in lbCreatureList.SelectedItems)
            {
                string data = SendMessage("SetCreatureStat", new string[] { ccd.CreatureId, "4", st4curr.Value.ToString(), st4max.Value.ToString() });
            }

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            string data = SendMessage("SetCameraHeight", new string[] { trackBar1.Value.ToString(), "True" });
        }

        private void button15_Click(object sender, EventArgs e)
        {
            string data = SendMessage("MoveCamera", new string[] {"0", "0", "1", "False" });
        }

        private void button13_Click(object sender, EventArgs e)
        {
            string data = SendMessage("MoveCamera", new string[] { "-1", "0", "0", "False" });

        }

        private void button14_Click(object sender, EventArgs e)
        {
            string data = SendMessage("MoveCamera", new string[] { "1", "0", "0", "False" });

        }

        private void button12_Click(object sender, EventArgs e)
        {
            string data = SendMessage("MoveCamera", new string[] {"0", "0", "-1", "False" });

        }

        private void button16_Click(object sender, EventArgs e)
        {
            string data = SendMessage("RotateCamera", new string[] { "-1", "False" });

        }

        private void button17_Click(object sender, EventArgs e)
        {
            string data = SendMessage("RotateCamera", new string[] { "1", "False" });

        }

        private void button18_Click(object sender, EventArgs e)
        {
            string data = SendMessage("MoveCamera", new string[] { camMoveX.Value.ToString(), "0", camMoveZ.Value.ToString(), "True" });
        }
    }
}
