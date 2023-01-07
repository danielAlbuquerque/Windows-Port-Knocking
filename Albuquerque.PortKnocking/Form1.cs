using SharpPcap;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using NetFwTypeLib;
using System.Windows.Forms;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Albuquerque.PortKnocking
{
    public partial class Form1 : Form
    {
        IniFile iniFile;
        string protocol = "tcp";

        Thread thread1 = null;
        Thread thread2 = null;

        public Form1()
        {
            InitializeComponent();

            if (!IsAdministrator)
            {
                MessageBox.Show("Execute o programa como administrador");
                Close();
            }

            iniFile = new IniFile("Settings.ini");
        }

        public static bool IsAdministrator => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = new BindingSource(DeviceManager.GetDevices(), null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "key";

            if (iniFile.KeyExists("lastInterfaceIndex", "general"))
                comboBox1.SelectedIndex = Int32.Parse(iniFile.Read("lastInterfaceIndex", "general"));

            if (iniFile.KeyExists("sequence", "command1"))
                portsTxtBox1.Text = iniFile.Read("sequence", "command1");

            if (iniFile.KeyExists("sequence", "command2"))
                portsTxtBox2.Text = iniFile.Read("sequence", "command2");

            if (iniFile.KeyExists("command", "command1"))
                commandText1.Text = iniFile.Read("command", "command1");

            if (iniFile.KeyExists("command", "command2"))
                commandText12.Text = iniFile.Read("command", "command2");

            if (iniFile.KeyExists("protocol", "general"))
            {
                string protocol = iniFile.Read("protocol", "general");
                if (protocol.ToUpper() == "TCP")
                {
                    radioButton1.Select();
                }
                else
                {
                    radioButton2.Select();
                }
            }

            SetTimeout(() =>
            {
                if (iniFile.KeyExists("running", "general"))
                {
                    bool running = bool.Parse(iniFile.Read("running", "general"));
                    if (running && IsFormValid())
                    {
                        Start();
                    }
                }
            }, 1000);
        }

        void SetTimeout(Action action, int ms)
        {
            Task.Delay(ms).ContinueWith((task) =>
            {
                action();
            }, TaskScheduler.FromCurrentSynchronizationContext());
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
            protocol = "tcp";
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            protocol = "udp";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void Start()
        {
            if (!IsFormValid())
            {
                MessageBox.Show("Preencha os dados corretamente");
                return;
            }

            if (button1.Text != "Parar")
            {
                comboBox1.Enabled = false;
                groupBox1.Enabled = false;
                groupBox2.Enabled = false;
                ICaptureDevice interfaceResult = DeviceManager.GetInterfaceByIndex(comboBox1.SelectedIndex);
                IEnumerable<string> ports1 = portsTxtBox1.Text.Split(',').ToList();
                IEnumerable<string> ports2 = portsTxtBox2.Text.Split(',').ToList();
                
                PortKnocker portKnocker1 = new PortKnocker(ports1.Select(int.Parse).ToList(), 60);
                Sniffer sniffer1 = new Sniffer(portKnocker1, commandText1.Text, protocol);

                PortKnocker portKnocker2 = new PortKnocker(ports2.Select(int.Parse).ToList(), 60);
                Sniffer sniffer2 = new Sniffer(portKnocker2, commandText12.Text, protocol);

                thread1 = new Thread(() =>
                {
                    sniffer1.Sniff(interfaceResult, Sniffer.GenerateFilterString(ports1, protocol));
                });
                thread2 = new Thread(() =>
                {
                    sniffer2.Sniff(interfaceResult, Sniffer.GenerateFilterString(ports2, protocol));
                });
                thread1.Start();
                thread2.Start();
                button1.Text = "Parar";

                iniFile.Write("lastInterfaceIndex", comboBox1.SelectedIndex.ToString(), "general");
                iniFile.Write("protocol", protocol, "general");
                iniFile.Write("sequence", portsTxtBox1.Text, "command1");
                iniFile.Write("command", commandText1.Text, "command1");
                iniFile.Write("sequence", portsTxtBox2.Text, "command2");
                iniFile.Write("command", commandText12.Text, "command2");
                iniFile.Write("running", "true", "general");
            }
            else
            {
                thread1.Abort();
                thread2.Abort();
                groupBox1.Enabled = true;
                groupBox2.Enabled = true;
                comboBox1.Enabled = true;
                button1.Text = "Iniciar";
            }
        }

        private bool IsFormValid()
        {
            if (comboBox1.SelectedItem != null && !string.IsNullOrEmpty(commandText1.Text) && !string.IsNullOrEmpty(commandText12.Text) && !string.IsNullOrEmpty(portsTxtBox1.Text) && !string.IsNullOrEmpty(portsTxtBox2.Text))
                return true;
            return false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (thread1 != null && thread1.IsAlive) thread1.Abort();
            if (thread2 != null && thread2.IsAlive) thread2.Abort();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
