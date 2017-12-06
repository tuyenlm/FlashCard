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
        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;
        private const int WM_NCACTIVATE = 0x86;
        private int counter;
        private static string fileLink = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\files\";
        private string imageLink2 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\images\";
        private static Dictionary<int, string> Pairs = new Dictionary<int, string>();
        private ToolStripMenuItem CloseMenuItem;
        private static string path = "";
        public Form1()
        {
            counter = 1;
            InitializeComponent();
            InitializeTrayIcon();
            this.ShowInTaskbar = false;
            Rectangle rcScreen = Screen.PrimaryScreen.WorkingArea;
            this.Location = new System.Drawing.Point(rcScreen.Right - 250, rcScreen.Bottom - 250);
            RegistryKey add = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            add.SetValue("FlashCard", Application.StartupPath + @"\" + Process.GetCurrentProcess().ProcessName + ".exe");
            if (add.GetValue("FlashCard") == null) add.SetValue("FlashCard", Application.StartupPath + @"\" + Process.GetCurrentProcess().ProcessName + ".exe");
            timer1.Start();
            
        }
        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);
            if (message.Msg == WM_NCHITTEST && (int)message.Result == HTCLIENT)
                message.Result = (IntPtr)HTCAPTION;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timer1.Enabled) pictureBox1.Show();
            else pictureBox1.Hide();
            int lines = File.ReadAllLines(path).Length;
            int[] minList = new int[lines];
            for (int i = 1; i < lines; i++) minList[i] = i;
            if (DateTime.Now.Second == 15 || DateTime.Now.Second == 30 || DateTime.Now.Second == 45 || DateTime.Now.Second == 59)
            {
                if (Pairs.Count() > 0)
                {
                    int k;
                    if (Pairs.ContainsKey(counter)) k = counter;
                    else
                    {
                        k = 1;
                        counter = k;
                    }
                    string[] txtSp = Pairs[k].Split('|');
                    if (txtSp.Length > 0) showFlashCard(txtSp[0], txtSp[1]);
                    else showFlashCard("Data", "Data not exists 1!!!");
                    counter++;
                }
                else showFlashCard("Data", "Data not exists 2!!!");
            }
        }
        private void showFlashCard(string title, string content)
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
            if (Directory.Exists(fileLink))
            {
                string[] fileArray = Directory.GetDirectories(fileLink);
                if (fileArray.Count() > 0)
                {
                    for (int i = 0; i < fileArray.Length; i++)
                    {
                        FileInfo f = new FileInfo(fileArray[i]);
                        comboBox1.Items.Add(f.Name);
                    }
                    comboBox1.SelectedIndex = 0;
                    changeValue(comboBox1.Text.Trim());
                }
                else showFlashCard("Data", "Data not exists!!!");
            }
            else System.IO.Directory.CreateDirectory(fileLink);
        }
        public static void changeValue(string subFolder)
        {
            path = fileLink + subFolder + @"\data.txt";
            if (!File.Exists(path)) File.Create(path).Close();
            else
            {
                Pairs.Clear();
                using (StreamReader sr = File.OpenText(path))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        string[] sSP = s.Split('|');
                        Pairs.Add(Convert.ToInt32(sSP[0]), sSP[1] + "|" + sSP[2]);
                    }
                    sr.Close();
                }
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            changeValue(comboBox1.Text.Trim());
            timer1.Stop();
            timer1.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("アプリケーションを終了しますか？", "閉じる", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Application.Exit();
                notifyIcon1.Visible = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
        }
        private void InitializeTrayIcon()
        {
            notifyIcon1.Visible = true;
            notifyIcon1.Icon = new Icon(imageLink2 + "flash-card.ico");
            CloseMenuItem = new ToolStripMenuItem();
            contextMenuStrip1.SuspendLayout();

            this.contextMenuStrip1.Items.AddRange(new ToolStripItem[] {
            this.CloseMenuItem});
            this.contextMenuStrip1.Name = "TrayIconContextMenu";
            this.contextMenuStrip1.Size = new Size(153, 70);

            this.CloseMenuItem.Name = "CloseMenuItem";
            this.CloseMenuItem.Size = new Size(152, 22);
            this.CloseMenuItem.Text = "トレイアイコンプログラムを閉じる";
            this.CloseMenuItem.Click += new EventHandler(this.CloseMenuItem_Click);
            contextMenuStrip1.ResumeLayout(false);
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
        }
        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("アプリケーションを終了しますか？", "閉じる", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
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
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }

        private void btnData_Click(object sender, EventArgs e)
        {
            DialogData DialogData = new DialogData(this);
            DialogData.setLink(path);
            Rectangle rcScreen = Screen.PrimaryScreen.WorkingArea;
            DialogData.Location = new Point(rcScreen.Right - DialogData.Width - this.Width - 30, rcScreen.Bottom - DialogData.Height - 20);
            DialogData.ShowDialog();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("tuyen");
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            MessageBox.Show("tuyen");

        }
    }
}