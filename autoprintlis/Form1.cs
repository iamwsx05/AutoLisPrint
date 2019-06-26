using com.digitalwave.iCare.gui.LIS;
using com.digitalwave.iCare.ValueObject;
using com.digitalwave.Utility;
using Common.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using weCare.Core.Entity;
using weCare.Core.Utils;

namespace autoprintlis
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        public Form1()
        {
            InitializeComponent();
        }
        private string m_strReportGroupID = string.Empty;
        public clsPrintValuePara m_objPrintInfo = null;

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.Query();
            //DateTime dtmNow = DateTime.Now;
            //this.dteDateStart.DateTime = new DateTime(dtmNow.Year, dtmNow.Month, 1);
            //this.dteDateEnd.DateTime = dtmNow;
            webKitBrowser1.Navigate("www.baidu.com");
        }

        void Query()
        {
            clsDomainController_ApplicationManage clsDomainController_ApplicationManage = new clsDomainController_ApplicationManage();
            clsDomainController_ApplicationManage clsDCApp = clsDomainController_ApplicationManage;

            string deptStr = string.Empty;
            string dteStart = string.Empty;
            string dteEnd = string.Empty;
            string ipNo = string.Empty;
            List<entityLisInfo> data = new List<entityLisInfo>();

            try
            {
                lisprintBiz biz = new lisprintBiz();
                string beginDate = this.dteDateStart.Text.Trim();
                string endDate = this.dteDateEnd.Text.Trim();

                if (string.IsNullOrEmpty(this.txtCardNo.Text))
                {
                    DialogBox.Msg("请输入卡号或住院号。");
                    return;
                }

                //this.gcData.DataSource = biz.QueryAreaReport(beginDate, endDate, this.txtCardNo.Text);
            }
            catch (Exception objEx)
            {
                ExceptionLog.OutPutException("Query-->"+objEx);
            }
        }

        internal void Print()
        {
            bool flag = false;
            string text = string.Empty;
            string text2 = string.Empty;
            List<string> list = new List<string>();
            text = "000000";
            text2 = "000000000004952076";
            clsPrintReport clsPrintReport = new clsPrintReport();
            if (list.IndexOf(text + text2) < 0)
            {
                list.Add(text + text2);
                clsPrintReport.m_mthGetPrintContentFromDB(text, text2, true);
                //clsPrintReport.m_mthPrint();
                flag = true;
            }
            if (!flag)
            {
                MessageBox.Show("请选择需要打印的的检验报告记录.");
            }
        }
     
        private void btnPrint_Click(object sender, EventArgs e)
        {
            Print();
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            this.Query();
        }

    }
}
