using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace FlashCard
{
    public partial class Form1 : Form     //  : extend
    {
        private const int WmNchittest = 0x84;
        private const int Htclient = 0x1;
        private const int Htcaption = 0x2;
	    private int _counter;
        private static readonly string FileLink = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\files\";
        private readonly string _imageLink2 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\images\";
	    private static readonly Dictionary<int, string> Pairs = new Dictionary<int, string>();
        private ToolStripMenuItem _closeMenuItem;
        private static string _path = "";
        public Form1()
        {
            _counter = 1;
            InitializeComponent();
            InitializeTrayIcon();
            ShowInTaskbar = false;
            Rectangle rcScreen = Screen.PrimaryScreen.WorkingArea;
            Location = new Point(rcScreen.Right - 250, rcScreen.Bottom - 250);
            RegistryKey add = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
	        if (add != null)
	        {
		        add.SetValue("FlashCard", Application.StartupPath + @"\" + Process.GetCurrentProcess().ProcessName + ".exe");
		        if (add.GetValue("FlashCard") == null)
			        add.SetValue("FlashCard", Application.StartupPath + @"\" + Process.GetCurrentProcess().ProcessName + ".exe");
	        }
	        timer1.Start();
            
        }
        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);
            if (message.Msg == WmNchittest && (int)message.Result == Htclient)
                message.Result = (IntPtr)Htcaption;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timer1.Enabled) pictureBox1.Show();
            else pictureBox1.Hide();
            int lines = File.ReadAllLines(_path).Length;
            int[] minList = new int[lines];
            for (int i = 1; i < lines; i++) minList[i] = i;
            if (DateTime.Now.Second == 15 || DateTime.Now.Second == 30 || DateTime.Now.Second == 45 || DateTime.Now.Second == 59)
            {
                if (Pairs.Count() > 0)
                {
                    int k;
                    if (Pairs.ContainsKey(_counter)) k = _counter;
                    else
                    {
                        k = 1;
                        _counter = k;
                    }
                    string[] txtSp = Pairs[k].Split('|');
                    if (txtSp.Length > 0) ShowFlashCard(txtSp[0], txtSp[1]);
                    else ShowFlashCard("Data", "Data not exists 1!!!");
                    _counter++;
                }
                else ShowFlashCard("Data", "Data not exists 2!!!");
            }
        }
        private void ShowFlashCard(string title, string content)
        {
            notifyIcon1.Visible = true;
            notifyIcon1.Icon = SystemIcons.Information;
            notifyIcon1.BalloonTipTitle = title;
            
            notifyIcon1.BalloonTipText = (content != "") ? content : "---";
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.ShowBalloonTip(9000);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (Directory.Exists(FileLink))
            {
                string[] fileArray = Directory.GetDirectories(FileLink);
                if (fileArray.Count() > 0)
                {
                    for (int i = 0; i < fileArray.Length; i++)
                    {
                        FileInfo f = new FileInfo(fileArray[i]);
                        comboBox1.Items.Add(f.Name);
                    }
                    comboBox1.SelectedIndex = 0;
                    ChangeValue(comboBox1.Text.Trim());
                }
                else ShowFlashCard("Data", "Data not exists!!!");
            }
            else Directory.CreateDirectory(FileLink);
        }
        public static void ChangeValue(string subFolder)
        {
            _path = FileLink + subFolder + @"\data.txt";
            if (!File.Exists(_path)) File.Create(_path).Close();
            else
            {
                Pairs.Clear();
                using (StreamReader sr = File.OpenText(_path))
                {
                    string s;
                    while ((s = sr.ReadLine()) != null)
                    {
                        string[] sSp = s.Split('|');
                        Pairs.Add(Convert.ToInt32(sSp[0]), sSp[1] + "|" + sSp[2]);
                    }
                    sr.Close();
                }
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangeValue(comboBox1.Text.Trim());
            timer1.Stop();
            timer1.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(@"アプリケーションを終了しますか？", @"閉じる", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Application.Exit();
                notifyIcon1.Visible = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            Hide();
        }
        private void InitializeTrayIcon()
        {
            notifyIcon1.Visible = true;
            notifyIcon1.Icon = new Icon(_imageLink2 + "flash-card.ico");
            _closeMenuItem = new ToolStripMenuItem();
            contextMenuStrip1.SuspendLayout();

            contextMenuStrip1.Items.AddRange(new ToolStripItem[] {
            _closeMenuItem});
            contextMenuStrip1.Name = "TrayIconContextMenu";
            contextMenuStrip1.Size = new Size(153, 70);

            _closeMenuItem.Name = "CloseMenuItem";
            _closeMenuItem.Size = new Size(152, 22);
            _closeMenuItem.Text = @"トレイアイコンプログラムを閉じる";
            _closeMenuItem.Click += CloseMenuItem_Click;
            contextMenuStrip1.ResumeLayout(false);
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
        }
        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(@"アプリケーションを終了しますか？", @"閉じる", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Application.Exit();
                notifyIcon1.Visible = false;
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                notifyIcon1.Visible = true;
                Hide();
            }
            else if (FormWindowState.Normal == WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }

        private void btnData_Click(object sender, EventArgs e)
        {
	        using (var dialogData = new DialogData())
	        {
		        dialogData.SetLink(_path);
		        var rcScreen = Screen.PrimaryScreen.WorkingArea;
		        dialogData.Location = new Point(rcScreen.Right - dialogData.Width - Width - 30, rcScreen.Bottom - dialogData.Height - 20);
		        dialogData.ShowDialog();
	        }
        }
    }
}