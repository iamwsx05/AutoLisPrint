using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using com.digitalwave.iCare.gui.LIS;
using Common.Controls;
using weCare.Core.Utils;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using MessageBoxDemo;
using System.Reflection;
using System.Xml;

namespace autoprintlis
{
    public partial class frmmainLis : frmBase
    {
        public frmmainLis()
        {
            InitializeComponent();
        }

        private string m_strReportGroupID = string.Empty;
        public clsPrintValuePara m_objPrintInfo = null;
        List<entityLisInfo> data = new List<entityLisInfo>();
        private static int waitTime = 0;

        #region
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WM_MOUSEACTIVATE = 0x0021;
        private const int MA_NOACTIVATE = 0x0003;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE;
                return cp;
            }

        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_MOUSEACTIVATE)
                m.Result = (IntPtr)MA_NOACTIVATE;
            else
                base.WndProc(ref m);
        }
        #endregion

        #region 无边框拖动效果
        [DllImport("user32.dll")]//拖动无窗体的控件
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;

        private void Start_MouseDown(object sender, MouseEventArgs e)
        {
            //拖动窗体
            ReleaseCapture();
            SendMessage(this.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }
        #endregion

        #region 事件

        private void frmmain_Load(object sender, EventArgs e)
        {
            Init();
        }
        
        #region 键盘事件
        void btn_Click(object sender, EventArgs e)
        {
            SimpleButton key = (SimpleButton)sender;
            string strkey = key.Text;

            if (strkey == "确定")
            {
                QueryAndPrint();
            }
            else if(strkey == "清空")
            {
                this.txtCard.Text = string.Empty;
                this.txtCard.Focus();
            }
            else
            {
                this.txtCard.Text += strkey;
                this.txtCard.Focus();
                this.txtCard.SelectionStart = this.txtCard.Text.Length;
            }
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            string txtContent = this.txtCard.Text;
            if(txtContent.Length > 0)
                txtContent = txtContent.Substring(0,txtContent.Length - 1);
            this.txtCard.Text = txtContent;
            this.txtCard.Focus();
            this.txtCard.SelectionStart = this.txtCard.Text.Length;
        }

        private void btn_back_MouseUp(object sender, MouseEventArgs e)
        {
            this.timer.Enabled = false;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            string txtContent = this.txtCard.Text;
            if (txtContent.Length > 0)
                txtContent = txtContent.Substring(0, txtContent.Length - 1);
            this.txtCard.Text = txtContent;
            this.txtCard.Focus();
            this.txtCard.SelectionStart = this.txtCard.Text.Length;
        }

        private void btn_back_MouseDown(object sender, MouseEventArgs e)
        {
            this.timer.Interval = 100;
            this.timer.Enabled = true;
        }

        private void frmmain_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                this.Close();
            }

            if (e.KeyChar == (char)Keys.Enter)
            {
                QueryAndPrint();
            }
        }

        #endregion

        #region 文本事件
        private void txtCard_TextChanged(object sender, EventArgs e)
        {
            if (this.txtCard.Text.Length == 10)
            {
                QueryAndPrint();
            }
        }

        private void txtCard_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                this.Close();
            }
            if(e.KeyChar == (char)Keys.Enter)
            {
                QueryAndPrint();
            }
        }
        #endregion

        #region 定时事件
        private void timerFocus_Tick(object sender, EventArgs e)
        {
            this.txtCard.Focus();
            this.txtCard.SelectionStart = this.txtCard.Text.Length;
        }

        private void timerClose_Tick(object sender, EventArgs e)
        {
            SendKeys.SendWait("{Enter}");
            timerClose.Enabled = false;
            this.txtCard.Text = string.Empty;
        }
        #endregion

        #region 方法

        private void Init()
        {
            this.btn_1.Click += new System.EventHandler(this.btn_Click);
            this.btn_2.Click += new System.EventHandler(this.btn_Click);
            this.btn_3.Click += new System.EventHandler(this.btn_Click);
            this.btn_4.Click += new System.EventHandler(this.btn_Click);
            this.btn_5.Click += new System.EventHandler(this.btn_Click);
            this.btn_6.Click += new System.EventHandler(this.btn_Click);
            this.btn_7.Click += new System.EventHandler(this.btn_Click);
            this.btn_8.Click += new System.EventHandler(this.btn_Click);
            this.btn_9.Click += new System.EventHandler(this.btn_Click);
            this.btn_0.Click += new System.EventHandler(this.btn_Click);
            this.btn_enter.Click += new System.EventHandler(this.btn_Click);
            this.btnClear.Click += new System.EventHandler(this.btn_Click);
            this.txtCard.Focus();
            this.lblName.Text = string.Empty;
           // this.lblIngNum.Text = string.Empty;
            this.lblTipNum.Text = string.Empty;
            this.lblTip2.Visible = false;
            this.lblTip1.Visible = false;
            this.lblIng1.Visible = false;
            //this.lblIng2.Visible = false;
            waitTime = Function.Int(ReadXmlConfig("WaitTime"));
            string path = Application.StartupPath + "\\guid.jpg";
            if (File.Exists(path))
                this.pictureBox2.Image = Image.FromFile(path);
            this.Activate();
        }

        private void initTimerClose()
        {
            //启动定时器
            timerClose.Enabled = true;
            timerClose.Interval = 8000;
            timerClose.Start();
        }

        private void printCompleted()
        {
            //this.lblIngNum.Text = string.Empty;
            this.lblTipNum.Text = string.Empty;
            this.lblName.Text = string.Empty;
            this.lblIng1.Visible = false;
            //this.lblIng2.Visible = false;
            this.lblTip1.Visible = false;
            this.lblTip2.Visible = false;
            this.lblComplete.Text = string.Empty;
            this.btn_0.Enabled = true;
            this.btn_1.Enabled = true;
            this.btn_2.Enabled = true;
            this.btn_3.Enabled = true;
            this.btn_4.Enabled = true;
            this.btn_5.Enabled = true;
            this.btn_6.Enabled = true;
            this.btn_7.Enabled = true;
            this.btn_8.Enabled = true;
            this.btn_9.Enabled = true;
            this.btn_back.Enabled = true;
            this.btn_enter.Enabled = true;
            this.btnClear.Enabled = true;
            this.txtCard.Enabled = true;
            this.txtCard.Text = string.Empty;
            this.txtCard.Focus();
        }

        private void printInit()
        {
            this.lblIng1.Visible = true;
            //this.lblIng2.Visible = true;
            this.lblTip1.Visible = true;
            this.lblTip2.Visible = true;
            this.lblComplete.Text = string.Empty;
            this.lblComplete.Visible = true;

            this.btn_0.Enabled = false;
            this.btn_1.Enabled = false;
            this.btn_2.Enabled = false;
            this.btn_3.Enabled = false;
            this.btn_4.Enabled = false;
            this.btn_5.Enabled = false;
            this.btn_6.Enabled = false;
            this.btn_7.Enabled = false;
            this.btn_8.Enabled = false;
            this.btn_9.Enabled = false;
            this.btn_back.Enabled = false;
            this.btn_enter.Enabled = false;
            this.btnClear.Enabled = false;
            this.txtCard.Enabled = false;
        }

        private void messageShow(int messageType, string messageStr)
        {
            if (!string.IsNullOrEmpty(messageStr))
            {
                if (messageType == 0)
                {
                    initTimerClose();
                    this.txtCard.Enabled = false;
                    if (frmShowMessage.Show(messageStr, enumMessageIcon.Information, enumMessageButton.OK) == DialogResult.Yes)
                    {
                        timerClose.Enabled = false;
                        this.txtCard.Text = string.Empty;
                        this.txtCard.Enabled = true;
                    }
                }
                else if (messageType == 1)
                {
                    this.lblName.Text = messageStr.Split(',')[0] + ",";
                    this.lblTipNum.Text = messageStr.Split(',')[1];
                }
                else if (messageType == 3)
                {
                    this.lblComplete.Text = messageStr;
                }
                else
                {
                    //this.lblIngNum.Text = messageStr;
                }
            }
        }

        #endregion

        #region QueryAndPrint
        /// <summary>
        /// 
        /// </summary>
        void QueryAndPrint()
        {
            string deptStr = string.Empty;
            string dteStart = string.Empty;
            string dteEnd = string.Empty;
            string ipNo = string.Empty;
            string patName = string.Empty;
            int printed = 0;
            int unPrinted = 0;
            DateTime dtmNow = DateTime.Now;
            
            try
            {
                uiHelper.BeginLoading(this);
                lisprintBiz biz = new lisprintBiz();
                int daySpan =  0 - Function.Int(ReadXmlConfig("Dayspan"));
                string beginDate = dtmNow.AddDays(daySpan).ToString("yyyy-MM-dd");
                string endDate = dtmNow.ToString("yyyy-MM-dd");
                String cardNo = this.txtCard.Text;
                if(cardNo.Length < 10)
                    cardNo = cardNo.PadLeft(10, '0');
                if (string.IsNullOrEmpty(this.txtCard.Text))
                {
                    messageShow(0, "请输入卡号。");
                    return;
                }
                patName = biz.getPatName(cardNo);
                data = biz.QueryReport(beginDate, endDate, cardNo,ref printed,ref unPrinted);

                if (printed == -1)
                {
                    messageShow(0, "对不起，系统有故障。\r\n如有疑问，请到检验科咨询！");
                    return;
                }
                    
                if(data == null || data.Count <= 0)
                {
                    messageShow(0, patName + ",未查询到您的报告。\r\n如有疑问，请到检验科咨询！");
                }
                else if(data != null && data.Count > 0)
                {
                    if (data.Count == printed)
                    {
                        messageShow(0, patName + ",未查询到您的报告。\r\n如有疑问，请到检验科咨询！");
                    }
                    else
                    {
                        printInit();
                        int Ii = 0;
                       
                        int page = 0;
                        foreach (entityLisInfo var in data)
                        {
                            if (string.IsNullOrEmpty(var.printeded) || var.printeded == "0")
                            {
                                page += GetPrintPage(var.rptGroupId, var.applicationId);
                            }
                        }
                        string tip1 = patName + "," + page.ToString();
                        messageShow(1, tip1);

                        foreach (entityLisInfo var in data)
                        {
                            if (string.IsNullOrEmpty(var.printeded) || var.printeded == "0")
                            {
                                messageShow(2, (++Ii).ToString());
                                this.Print(var.rptGroupId, var.applicationId);
                                Delay(waitTime);
                            }
                            //if (string.IsNullOrEmpty(var.checkContent))
                            //{
                            //    if (var.checkContent.Contains("性激素6项") && var.checkContent.Contains("绒毛膜促性腺激素定量"))
                            //    {
                            //        messageShow(2, (++Ii).ToString());
                            //        Delay(waitTime);
                            //    }
                            //}
                        }
                        printCompleted();
                        messageShow(3, "您的报告已全部打印。");
                        Delay(3);
                        this.lblComplete.Text = string.Empty;
                    }
                }
            }
            catch (Exception objEx)
            {
                ExceptionLog.OutPutException("Query-->"+objEx);
                messageShow(0, "对不起，系统有故障！如有疑问，请到检验科咨询！");
            }
            finally
            {
                uiHelper.CloseLoading(this);
            }
        }
        #endregion

        #region  print
        /// <summary>
        /// 
        /// </summary>
        internal void Print(string reportGroupID, string applicationId)
        {
            List<string> list = new List<string>();
            clsPrintReport clsPrintReport = new clsPrintReport();
            
            if (list.IndexOf(reportGroupID + applicationId) < 0)
            {
                list.Add(reportGroupID + applicationId);
                clsPrintReport.m_mthGetPrintContentFromDB(reportGroupID, applicationId, true);
                clsPrintReport.m_mthPrint();
            }
        }

        internal int GetPrintPage(string reportGroupID, string applicationId)
        {
            int page = 0;
            List<string> list = new List<string>();
            clsPrintReport clsPrintReport = new clsPrintReport();

            if (list.IndexOf(reportGroupID + applicationId) < 0)
            {
                list.Add(reportGroupID + applicationId);
                clsPrintReport.m_mthGetPrintContentFromDB(reportGroupID, applicationId, true);
                DataTable m_dtbResult = clsPrintReport.m_ObjPrintInfo.m_dtbResult;
                DataTable m_dtbSample = clsPrintReport.m_ObjPrintInfo.m_dtbBaseInfo;
                clsUnifyReportPrint printReport = new clsUnifyReportPrint();
                float p_fltX = 41.3500023F;
                float p_fltY = 127.796875F;
                float p_fltWidth = 744.3F;
                float p_fltHeight = 360.203125F;
                float p_fltMaxHeight = 360.203125F;
                printReport.m_dtbSample = m_dtbSample;
                printReport.m_mthInitalPrintTool();
                clsPrintPerPageInfo[] m_objPrintPage = printReport.m_objConstructPrintPageInfoGetPage(m_dtbResult,  p_fltX,  p_fltY,  p_fltWidth,  p_fltHeight,  p_fltMaxHeight);
                page = m_objPrintPage.Length;
            }

            return page;
        }
        #endregion

        #region 读配置文件
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbKey"></param>
        /// <returns></returns>
        public string ReadXmlConfig(string dbKey)
        {
            string result = null;
            string text = Application.StartupPath + "\\Config.xml";
            //string text = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName + "\\Debug\\OpMedStoreLed.xml";
            if (!File.Exists(text))
            {
                try
                {
                    text = AppDomain.CurrentDomain.BaseDirectory + "\\Debug\\Config.xml";
                    if (!File.Exists(text))
                    {
                        text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\bin\\Config.xml";
                    }
                }
                catch(Exception ex)
                {
                    text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\bin\\Config.xml";
                    ExceptionLog.OutPutException("ReadXmlConfig-->"+ex);
                }
            }
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(text);
            XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/configuration");
            XmlNodeList xmlNodeList2 = xmlNodeList[0].SelectNodes("Client");
            foreach (XmlNode xmlNode in xmlNodeList2)
            {
                foreach (XmlNode xmlNode2 in xmlNode.ChildNodes)
                {
                    if (xmlNode2.Name == dbKey)
                    {
                        result = xmlNode2.InnerText;
                    }
                }
            }
            return result;
        }
        #endregion

  
        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = true;
                this.WindowState = FormWindowState.Normal;
                this.Activate();
                this.notifyIcon.Visible = false;
            }
        }

        #endregion

        public static bool Delay(int delayTime)
        {
            DateTime now = DateTime.Now;
            int s;
            do
            {
                TimeSpan spand = DateTime.Now - now;
                s = spand.Seconds;
                Application.DoEvents();
            }
            while (s < delayTime);
            return true;
        }

    }
}