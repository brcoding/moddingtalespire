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
        public class CustomBoardAssetData
        {
            public string GUID { get; set; }
            public string boardAssetName { get; set; }
            public string boardAssetDesc { get; set; }
            public string boardAssetType { get; set; }
            public string seachString { get; set; }
            public string boardAssetGroup { get; set; }
        }
        public List<CustomBoardAssetData> creatureAssets = new List<CustomBoardAssetData>();
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
                    byte[] msg = Encoding.UTF8.GetBytes(command + " " + string.Join(",", msgparams));

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);
                    Console.WriteLine("Bytes sent:" + bytesSent.ToString());
                    Console.WriteLine("Command Sent: " + Encoding.UTF8.GetString(msg, 0, bytesSent));
                    sender.ReceiveTimeout = 3000;
                    // Receive the response from the remote device.  
                    string data = "";
                    int bytesRec = 0;
                    int sleeps = 0;
                    while (sender.Available == 0 && sleeps < 3000)
                    {
                        System.Threading.Thread.Sleep(1);
                        sleeps++;
                    }
                    while (sender.Available > 0) 
                    { 
                        bytesRec = sender.Receive(bytes);
                        data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
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
            Console.WriteLine(data);
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

        private void button19_Click(object sender, EventArgs e)
        {
            string data = SendMessage("ZoomCamera", new string[] { "-0.1", "False" });
        }

        private void button20_Click(object sender, EventArgs e)
        {
            string data = SendMessage("ZoomCamera", new string[] { "0.1", "False" });
        }

        private void button21_Click(object sender, EventArgs e)
        {
            foreach (CustomCreatureData ccd in lbCreatureList.SelectedItems)
            {
                string data = SendMessage("SayText", new string[] { ccd.CreatureId, tbSay.Text });
            }
        }

        private void lbStat1_Click(object sender, EventArgs e)
        {
            TextDialog td = new TextDialog("Update Name", "What do you want to change the name to?", lbStat1.Text);
            if (td.ShowDialog() == DialogResult.OK)
            {
                lbStat1.Text = td.Answer;
                SendMessage("SetCustomStatName", new string[] { "1", lbStat1.Text });
                
            }
        }

        private void lbStat2_Click(object sender, EventArgs e)
        {
            TextDialog td = new TextDialog("Update Name", "What do you want to change the name to?", lbStat2.Text);
            if (td.ShowDialog() == DialogResult.OK)
            {
                lbStat2.Text = td.Answer;
                SendMessage("SetCustomStatName", new string[] { "2", lbStat2.Text });

            }
        }

        private void lbStat3_Click(object sender, EventArgs e)
        {
            TextDialog td = new TextDialog("Update Name", "What do you want to change the name to?", lbStat3.Text);
            if (td.ShowDialog() == DialogResult.OK)
            {
                lbStat3.Text = td.Answer;
                SendMessage("SetCustomStatName", new string[] { "3", lbStat3.Text });

            }
        }

        private void lbStat4_Click(object sender, EventArgs e)
        {
            TextDialog td = new TextDialog("Update Name", "What do you want to change the name to?", lbStat4.Text);
            if (td.ShowDialog() == DialogResult.OK)
            {
                lbStat4.Text = td.Answer;
                SendMessage("SetCustomStatName", new string[] { "4", lbStat4.Text });

            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            //lbCreatureAssets
            string data = SendMessage("GetCreatureAssets", new string[0]);
            Console.WriteLine(data);
            dynamic json = JsonConvert.DeserializeObject(data);
            creatureAssets.Clear();

            foreach (dynamic item in json)
            {
                creatureAssets.Add(new CustomBoardAssetData { GUID = (string)item["GUID"], boardAssetName = (string)item["boardAssetName"] });
            }
            lbCreatureAssets.ValueMember = "GUID";
            lbCreatureAssets.DisplayMember = "boardAssetName";
            lbCreatureAssets.DataSource = creatureAssets;
        }
    }
}
