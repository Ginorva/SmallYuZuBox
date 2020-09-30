using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmallYuzuBox
{
    public partial class Form1 : BaseForm
    {
        private bool isMouse = false; // 鼠标是否按下
        // 原点位置
        private int originX = 0;
        private int originY = 0;
        // 鼠标按下位置
        private int mouseX = 0;
        private int mouseY = 0;
        Boolean textboxHasText = false;//判断输入框是否有文本
        public Form1()
        {
            InitializeComponent();             
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.BorderStyle = BorderStyle.None;            
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255,192,128);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;         
            Font font = new Font(dataGridView1.ColumnHeadersDefaultCellStyle.Font, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = font;            
            dataGridView1.BackgroundColor = Color.FromArgb(255, 192, 128);
            dataGridView1.AllowUserToResizeRows = false;                  
            string basepath = "db";
            string path = basepath + @"\base.sqlite";
            SQLiteConnection cn = new SQLiteConnection("data source=" + path);
            cn.Open();
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = cn;
            if (!Directory.Exists(basepath))
            {
                Directory.CreateDirectory(basepath);                                           
                cmd.CommandText = "create table if not exists user_own_code(code varchar(20),bought numeric(9,2))";
                cmd.ExecuteNonQuery();               
            }                                           
            cn.Close();
            Refresh_Data();
        }
        
        public void Refresh_Data() {
            dataGridView1.Rows.Clear();
            string basepath = "db";
            string path = basepath + @"\base.sqlite";
            SQLiteConnection cn = new SQLiteConnection("data source=" + path);
            cn.Open();
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = cn;
            cmd.CommandText = "select * from user_own_code";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
            DataTable db = new DataTable();
            adapter.Fill(db);
            WebClient client = new WebClient();
            string request = string.Empty;
            foreach (DataRow r in db.Rows)
            {
                request += r[0] + ",";
            }
            string url = "http://hq.sinajs.cn/list=" + request;
            String sr = client.DownloadString(url);
            sr = sr.Replace("\n", "");
            String[] srS = sr.Split(';');
            for (int i = 0; i < srS.Length; i++)
            {
                if (srS[i] != string.Empty)
                {
                    String[] srs = srS[i].Split(',');
                    int index = dataGridView1.Rows.Add();
                    String code = srs[0].Substring(11, srs[0].IndexOf('=') - 11);
                    dataGridView1.Rows[index].Cells[0].Value = code;
                    dataGridView1.Rows[index].Cells[2].Value = db.Rows[i][1];
                    if (code.ToLower().StartsWith("sh") || code.ToLower().StartsWith("sz"))
                    {
                        String[] srss = srs[0].Split('"');
                        dataGridView1.Rows[index].Cells[1].Value = srss[1];
                        dataGridView1.Rows[index].Cells[3].Value = srs[3];
                        double rate = Math.Round(((Double.Parse(srs[3]) - Double.Parse(db.Rows[i][1].ToString())) / Double.Parse(db.Rows[i][1].ToString())) * 100, 2, MidpointRounding.AwayFromZero);
                        dataGridView1.Rows[index].Cells[4].Value = rate > 0 ? "+ " + Math.Abs(rate) + "%" : "- " + Math.Abs(rate) + "%";
                        dataGridView1.Rows[index].Cells[4].Style.ForeColor = rate > 0 ? Color.Red : Color.Green;
                    }
                    else if (code.ToLower().StartsWith("us"))
                    {
                        dataGridView1.Rows[index].Cells[1].Value = srs[0];
                        dataGridView1.Rows[index].Cells[3].Value = srs[1];
                        double rate = Math.Round(((Double.Parse(srs[1]) - Double.Parse(db.Rows[i][1].ToString())) / Double.Parse(db.Rows[i][1].ToString())) * 100, 2, MidpointRounding.AwayFromZero);
                        dataGridView1.Rows[index].Cells[4].Value = rate > 0 ? "+ " + Math.Abs(rate) + "%" : "- " + Math.Abs(rate) + "%";
                        dataGridView1.Rows[index].Cells[4].Style.ForeColor = rate > 0 ? Color.Red : Color.Green;
                    }
                    else if (code.ToLower().StartsWith("hk"))
                    {
                        dataGridView1.Rows[index].Cells[1].Value = srs[1];
                        dataGridView1.Rows[index].Cells[3].Value = srs[6];
                        double rate = Math.Round(((Double.Parse(srs[6]) - Double.Parse(db.Rows[i][1].ToString())) / Double.Parse(db.Rows[i][1].ToString())) * 100, 2, MidpointRounding.AwayFromZero);
                        dataGridView1.Rows[index].Cells[4].Value = rate > 0 ? "+ " + Math.Abs(rate) + "%" : "- " + Math.Abs(rate) + "%";
                        dataGridView1.Rows[index].Cells[4].Style.ForeColor = rate > 0 ? Color.Red : Color.Green;
                    }
                    else { 

                    }
                }
            }
            cn.Close();
        }
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouse)
            {
                // 移动距离
                int moveX = (e.X + this.Location.X) - mouseX;
                int moveY = (e.Y + this.Location.Y) - mouseY;
                int targetX = originX + moveX;
                int targetY = originY + moveY;
                this.Location = new Point(targetX, targetY);
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            { // 判断鼠标按键
                isMouse = true;
                // 屏幕坐标位置
                originX = this.Location.X;
                originY = this.Location.Y;
                // 鼠标按下位置
                mouseX = originX + e.X;
                mouseY = originY + e.Y;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isMouse)
            {
                isMouse = false;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
 

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textboxHasText == false)
                textBox1.Text = "";

            textBox1.ForeColor = Color.Black;
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                textBox1.Text = "股票码-买入价-操作";
                textBox1.ForeColor = Color.LightGray;
                textboxHasText = false;
            }
            else
                textboxHasText = true;

        }

        private void timer1_Tick(object sender, EventArgs e)
        {                 
            Refresh_Data();           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string basepath = "db";
            string path = basepath + @"\base.sqlite";
            SQLiteConnection cn = new SQLiteConnection("data source=" + path);
            try
            {
                if (textBox1.Text == "股票码-买入价-操作")
                {
                    MessageBox.Show("请输入内容");
                    return;
                }
                if (textBox1.Text.EndsWith("D"))
                {
                    cn.Open();
                    SQLiteCommand cmd = new SQLiteCommand();
                    cmd.Connection = cn;
                    String[] inputs = textBox1.Text.Split('-');
                    cmd.CommandText = "delete from user_own_code where code='" + inputs[0] + "'";
                    cmd.ExecuteNonQuery();
                    cn.Close();
                    Refresh_Data();
                    MessageBox.Show("删除成功");
                }
                else if (textBox1.Text.EndsWith("U"))
                {
                    cn.Open();
                    SQLiteCommand cmd = new SQLiteCommand();
                    cmd.Connection = cn;
                    String[] inputs = textBox1.Text.Split('-');
                    cmd.CommandText = "update user_own_code set bought=" + inputs[1] + " where code='" + inputs[0] + "'";
                    cmd.ExecuteNonQuery();
                    cn.Close();
                    Refresh_Data();
                    MessageBox.Show("修改成功");
                }
                else if (textBox1.Text.EndsWith("A"))
                {
                    cn.Open();
                    SQLiteCommand cmd = new SQLiteCommand();
                    cmd.Connection = cn;
                    String[] inputs = textBox1.Text.Split('-');
                    cmd.CommandText = "insert into user_own_code values('" + inputs[0] + "'," + inputs[1] + ")";
                    cmd.ExecuteNonQuery();
                    cn.Close();
                    Refresh_Data();
                    MessageBox.Show("添加成功");
                }
                else {
                    MessageBox.Show("请输入有效指令");
                }
            }
            catch {
                MessageBox.Show("请输入正确的指令");
            }
        }
        public override void ChangeGridView(int width, int height)
        {
            dataGridView1.Width = width;
            dataGridView1.Height = height; 
        }
    }
}
