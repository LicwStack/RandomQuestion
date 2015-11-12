using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;


namespace Random_question
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        private Microsoft.Office.Interop.Excel.Application excelApplication = null;
        private Workbooks excelWorkBooks = null;
        private Workbook excelWorkBook = null;
        private Worksheet excelWorkSheet = null;
        private Range excelRange = null;
        private int ActiveSheetIndex = 0;
        private int responder, rangeOfExcel;
        private bool stateOfButton1, stateOfButton2,stateOfSave = true;
        string excelOpenFileName = "";

        #region 移动窗体
        /// <summary>
        /// 重写WndProc方法,实现窗体移动和禁止双击最大化
        /// </summary>
        /// <param name="m">Windows 消息</param>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x4e:
                case 0xd:
                case 0xe:
                case 0x14:
                    base.WndProc(ref m);
                    break;
                case 0x84://鼠标点任意位置后可以拖动窗体
                    this.DefWndProc(ref m);
                    if (m.Result.ToInt32() == 0x01)
                    {
                        m.Result = new IntPtr(0x02);
                    }
                    break;
                case 0xA3://禁止双击最大化
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
        #endregion

        //打开文件
        private void button1_Click(object sender, EventArgs e)
        {
            if (stateOfSave == true)
            {
                stateOfButton1 = true;
                OpenExcelFile();
            }
            else
            {
                MessageBox.Show("系统已加载文件，若想重新打开文件请先退出系统！！！");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            周次数.Text = GetWeekOfYear().ToString();
            excelApplication = null;//Excel Application Object
            excelWorkBooks = null;//Workbooks
            excelWorkBook = null;//Excel Workbook Object
            excelWorkSheet = null;//Excel Worksheet Object
            ActiveSheetIndex = 1;
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.TableName = "table1";
            dt.AcceptChanges();
        }
        
        // 打开Excel文件
        public void OpenExcelFile()
        {
            OpenFileDialog opd = new OpenFileDialog();
            if (opd.ShowDialog() == DialogResult.OK)
            excelOpenFileName = opd.FileName;
            textBox4.Text = System.IO.Path.GetFileNameWithoutExtension(excelOpenFileName);
            if (excelOpenFileName !="")
            {
                try
                {
                    excelApplication = new Microsoft.Office.Interop.Excel.ApplicationClass();
                    excelWorkBooks = excelApplication.Workbooks;
                    excelWorkBook = ((Workbook)excelWorkBooks.Open(excelOpenFileName, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value));
                    excelWorkSheet = (Worksheet)excelWorkBook.Worksheets[ActiveSheetIndex];
                    excelApplication.Visible = false;
                    rangeOfExcel = excelWorkSheet.UsedRange.Cells.Rows.Count;//获取EXCEL行数
                    stateOfSave = false;
                }
                catch (Exception e)
                {
                    closeApplication();
                    MessageBox.Show("(1)没有安装Excel；(2)或没有安装.NET 可编程性支持；\n详细信息："
                        + e.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("未选择文件！");
                closeApplication();
            }
        }

        // 读取一个Cell的值       
        public string getOneCellValue(int CellRowID, int CellColumnID)
        {
            if (CellRowID <= 0)
            {
                throw new Exception("行索引超出范围！");
            }
            string sValue = "";
            try
            {
                sValue = ((Range)excelWorkSheet.Cells[CellRowID, CellColumnID]).Text.ToString();
            }
            catch (Exception e)
            {
                closeApplication();
                throw new Exception(e.Message);
            }
            return (sValue);
        }

        // 向一个Cell写入数据
        public void setOneCellValue(int CellColumnID, string Value)
        {
            try
            {
                excelRange = (Range)excelWorkSheet.Cells[responder, CellColumnID];
                excelRange.Value2 = Value;
                excelRange = null;
            }
            catch (Exception e)
            {
                closeApplication();
                throw new Exception(e.Message);
            }
        }

        // 计算本周为该年第几周
        private static int GetWeekOfYear()
        {
            int firstWeekend = 7 - Convert.ToInt32(DateTime.Parse(DateTime.Today.Year + "-1-1").DayOfWeek);
            int currrentDay = DateTime.Today.DayOfYear;
            return Convert.ToInt32(Math.Ceiling((currrentDay - firstWeekend) / 7.0)) + 1 - 10;
        }

        // 关闭application
        public void closeApplication()
        {
            if (excelApplication != null)
            {
            excelApplication.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApplication);
            excelApplication = null;
            GC.Collect();
            }
            stateOfSave = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (stateOfButton1 == true && excelOpenFileName != "")
            {
                do
                {
                    Random ran = new Random();
                    responder = ran.Next(6, rangeOfExcel);//随机提问一位
                } while (getOneCellValue(responder, GetWeekOfYear() + 6) != "");
                textBox1.Text = getOneCellValue(responder, 4);
                textBox2.Text = getOneCellValue(responder, 5);
                textBox3.Text = getOneCellValue(responder, 3);
                numericUpDown1.Text = "0";
            }
            else
                MessageBox.Show("请打开文件后进行操作！！！");
            stateOfButton2 = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            closeApplication();
            this.Close();
        }

        //最小化到托盘
        private void button5_Click(object sender, EventArgs e)
        {
            this.Hide();
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.ShowBalloonTip(30, "提示", "再次打开请单击！", ToolTipIcon.Info);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (stateOfButton1 == true && stateOfButton2 == true && excelOpenFileName != "")
            {
                if (numericUpDown1.Value != 0)
                {
                    setOneCellValue(GetWeekOfYear() + 6, Convert.ToString(numericUpDown1.Value));
                    excelApplication.DisplayAlerts = false;
                    excelApplication.AlertBeforeOverwriting = false;
                    excelApplication.Application.Workbooks.Add(true).Save();//保存工作簿   
                    excelApplication.Save();//保存excel文件 
                }
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void button4_MouseMove(object sender, MouseEventArgs e)
        {
            button4.BackColor = Color.Red;
        }

        private void button4_MouseLeave(object sender, EventArgs e)
        {
            button4.BackColor = Color.Transparent;
        }

        private void button5_MouseLeave(object sender, EventArgs e)
        {
            button5.BackColor = Color.Transparent;
        }

        private void button5_MouseMove(object sender, MouseEventArgs e)
        {
            button5.BackColor = Color.BurlyWood;
        }
    }
}
