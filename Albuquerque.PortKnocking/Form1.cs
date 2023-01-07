using SharpPcap;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using NetFwTypeLib;
using System.Windows.Forms;

namespace Albuquerque.PortKnocking
{
    public partial class Form1 : Form
    {
        IniFile iniFile;
        string protocol1 = "tcp";
        string protocol2 = "tcp";

        Thread thread1 = null;
        Thread thread2 = null;

        public Form1()
        {
            InitializeComponent();

            iniFile = new IniFile("Settings.ini");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = new BindingSource(DeviceManager.GetDevices(), null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "key";

            if (iniFile.KeyExists("last", "interfaces"))
                comboBox1.SelectedIndex = Int32.Parse(iniFile.Read("last", "interfaces"));

            if (iniFile.KeyExists("sequence", "command1"))
                portsTxtBox1.Text = iniFile.Read("sequence", "command1");

            if (iniFile.KeyExists("sequence", "command2"))
                portsTxtBox2.Text = iniFile.Read("sequence", "command2");

            if (iniFile.KeyExists("command", "command1"))
                commandText1.Text = iniFile.Read("command", "command1");

            if (iniFile.KeyExists("command", "command2"))
                commandText12.Text = iniFile.Read("command", "command2");

            if (iniFile.KeyExists("protocol", "command1"))
            {
                string protocol = iniFile.Read("protocol", "command1");
                if (protocol.ToUpper() == "TCP")
                {
                    radioButton1.Select();
                } else
                {
                    radioButton2.Select();
                }
            }

            if (iniFile.KeyExists("protocol", "command2"))
            {
                string protocol = iniFile.Read("protocol", "command2");
                if (protocol.ToUpper() == "TCP")
                {
                    radioButton4.Select();
                }
                else
                {
                    radioButton3.Select();
                }
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(1000);
                Hide();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            protocol1 = "tcp";
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            protocol1 = "udp";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text != "Parar")
            {
                comboBox1.Enabled = false;
                groupBox1.Enabled = false;
                groupBox2.Enabled = false;
                ICaptureDevice interfaceResult = DeviceManager.GetInterfaceByIndex(comboBox1.SelectedIndex);
                IEnumerable<string> ports1 = portsTxtBox1.Text.Split(',').ToList();
                IEnumerable<string> ports2 = portsTxtBox2.Text.Split(',').ToList();
                List<int> intPorts1 = ports1.ToList().Select(int.Parse).ToList();
                List<int> intPorts2 = ports2.ToList().Select(int.Parse).ToList();
                PortKnocker portKnocker1 = new PortKnocker(intPorts1, 60);
                PortKnocker portKnocker2 = new PortKnocker(intPorts2, 60);
                Sniffer sniffer1 = new Sniffer(portKnocker1, commandText1.Text, protocol1);
                Sniffer sniffer2 = new Sniffer(portKnocker2, commandText12.Text, protocol2);
                var filter1 = Sniffer.GenerateFilterString(ports1, protocol1);
                var filter2 = Sniffer.GenerateFilterString(ports2, protocol2);
                thread1 = new Thread(() =>
                {
                    sniffer1.Sniff(interfaceResult, filter1);
                });
                thread2 = new Thread(() =>
                {
                    sniffer2.Sniff(interfaceResult, filter2);
                });
                thread1.Start();
                thread2.Start();
                button1.Text = "Parar";

                iniFile.Write("last", comboBox1.SelectedIndex.ToString(), "interfaces");
                iniFile.Write("sequence", portsTxtBox1.Text, "command1");
                iniFile.Write("command", commandText1.Text, "command1");
                iniFile.Write("protocol", protocol1, "command1");
                iniFile.Write("sequence", portsTxtBox2.Text, "command2");
                iniFile.Write("command", commandText12.Text, "command2");
                iniFile.Write("protocol", protocol2, "command2");
            } else {
                thread1.Abort();
                thread2.Abort();
                var fwConf = new FirewallConf();
                fwConf.DeleteRule("portknocking");
                groupBox1.Enabled = true;
                groupBox2.Enabled = true;
                comboBox1.Enabled = true;
                button1.Text = "Iniciar";
            }
        }

  

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            protocol2 = "tcp";
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            protocol2 = "udp";
        }
    }
}
