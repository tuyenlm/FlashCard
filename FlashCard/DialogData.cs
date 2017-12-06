using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlashCard
{
    public partial class DialogData : Form
    {
        private Form1 mainForm = null;
        private string linkFile;
        public DialogData()
        {
            InitializeComponent();
        }
        public DialogData(Form callingForm)
        {
            mainForm = callingForm as Form1;
            InitializeComponent();
        }
        public void setLink(string link)
        {
            this.linkFile = link;
        }
        private string getLink()
        {
            return linkFile;
        }

        private void DialogData_Load(object sender, EventArgs e)
        {

            string path = getLink();
            string[] subName = path.Split('\\');
            lblSubFolder.Text = subName[subName.Length - 2];
            DataTable dt = new DataTable();
            dt.Columns.Add("#");
            dt.Columns["#"].AutoIncrement = true;
            //Set the Starting or Seed value.
            dt.Columns["#"].AutoIncrementSeed = 1;
            //Set the Increment value.
            dt.Columns["#"].AutoIncrementStep = 1;
            dt.Columns["#"].ReadOnly = true;
            dt.Columns.Add("言葉");
            dt.Columns.Add("例");
            using (StreamReader sr = File.OpenText(path))
            {
                string s;
                int i = 1;
                while ((s = sr.ReadLine()) != null)
                {
                    string[] sSp = s.Split('|');
                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(Convert.ToInt32(sSp[0]), sSp[1], sSp[2]);
                    i++;
                }
                sr.Close();
            }
            dataGridView1.DataSource = dt;
            dataGridView1.Columns["#"].Width = 30;
            dataGridView1.Columns["#"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //Console.WriteLine(e.ColumnIndex);

        }
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].Cells[0].Value != null)
            {
                string value0 = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                string value1 = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                string value2 = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                string path = getLink();
                StreamReader file = new StreamReader(path);
                string line;
                bool sts = true;
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains(value0))
                    {
                        file.Close();
                        string str = File.ReadAllText(path);
                        str = str.Replace(line.ToString(), value0 + "|" + value1 + "|" + value2);
                        File.WriteAllText(path, str);
                        sts = false;
                        break;
                    }
                }
                if (sts)
                {
                    file.Close();
                    int lines = File.ReadAllLines(path).Length;
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine((lines + 1) + "|" + value1 + "|" + value2);
                        dataGridView1.Rows[e.RowIndex].Cells[0].Value = lines + 1;
                        sw.Close();
                    }
                }
            }
        }

        private void DialogData_FormClosing(object sender, FormClosingEventArgs e)
        {
            Console.WriteLine("Closing");

            string path = getLink();
            string[] subName = path.Split('\\');
            Form1.changeValue(subName[subName.Length - 2]);
            Form1.timer1.Stop();
            Form1.timer1.Start();
        }
    }
}
