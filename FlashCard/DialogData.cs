using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace FlashCard
{
    public partial class DialogData : Form
    {
	    private string _linkFile;
        public DialogData()
        {
            InitializeComponent();
        }
       
        public void SetLink(string link)
        {
            _linkFile = link;
        }
        private string GetLink()
        {
            return _linkFile;
        }

        private void DialogData_Load(object sender, EventArgs e)
        {

	        var path = GetLink();
	        var subName = path.Split('\\');
            lblSubFolder.Text = subName[subName.Length - 2];
            var dt = new DataTable();
            dt.Columns.Add("#");
            dt.Columns["#"].AutoIncrement = true;
            //Set the Starting or Seed value.
            dt.Columns["#"].AutoIncrementSeed = 1;
            //Set the Increment value.
            dt.Columns["#"].AutoIncrementStep = 1;
            dt.Columns["#"].ReadOnly = true;
            dt.Columns.Add("言葉");
            dt.Columns.Add("例");
            using (var sr = File.OpenText(path))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
					var sSp = s.Split('|');
                    dt.NewRow();
                    dt.Rows.Add(Convert.ToInt32(sSp[0]), sSp[1], sSp[2]);
                }
                sr.Close();
            }
            dataGridView1.DataSource = dt;
	        // ReSharper disable once PossibleNullReferenceException
            dataGridView1.Columns["#"].Width = 30;
            dataGridView1.Columns["#"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //Console.WriteLine(e.ColumnIndex);

        }
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
	        if (dataGridView1 != null && dataGridView1.Rows[e.RowIndex].Cells[0].Value == null) return;
	        if (dataGridView1 == null) return;
	        var value0 = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
	        var value1 = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
	        var value2 = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
	        var path = GetLink();
	        var file = new StreamReader(path);
	        string line;
	        var sts = true;
	        while ((line = file.ReadLine()) != null)
	        {
		        if (!line.Contains(value0)) continue;
		        file.Close();
		        var str = File.ReadAllText(path);
		        str = str.Replace(line, value0 + "|" + value1 + "|" + value2);
		        File.WriteAllText(path, str);
		        sts = false;
		        break;
	        }
	        if (!sts) return;
	        file.Close();
	        var lines = File.ReadAllLines(path).Length;
	        using (var sw = File.AppendText(path))
	        {
		        sw.WriteLine((lines + 1) + "|" + value1 + "|" + value2);
		        dataGridView1.Rows[e.RowIndex].Cells[0].Value = lines + 1;
		        sw.Close();
	        }
        }

        private void DialogData_FormClosing(object sender, FormClosingEventArgs e)
        {
            var path = GetLink();
            var subName = path.Split('\\');
            Form1.ChangeValue(subName[subName.Length - 2]);
            Form1.timer1.Stop();
            Form1.timer1.Start();
        }
    }
}
