using com.digitalwave.iCare.gui.LIS;
using com.digitalwave.iCare.ValueObject;
using com.digitalwave.Utility;
using Sybase.DataWindow;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using weCare.Core.Utils;

namespace autoprintlis
{
    public class clsPrintReport   
    {
        public PrintDocument m_printDoc;
        public PrintPreviewDialog m_printPrev;
        public PrintDialog m_printDlg;
        private clsPrintValuePara m_objPrintInfo;
        private infPrintRecord m_objPrintTool;
        private DataStore dsReport = null;
        private string m_strReportGroupID;
        private bool m_blnPrintWithDialog = false;
        public bool m_BlnPrintWithDialog
        {
            get
            {
                return this.m_blnPrintWithDialog;
            }
            set
            {
                this.m_blnPrintWithDialog = value;
            }
        }
        public clsPrintValuePara m_ObjPrintInfo
        {
            get
            {
                return this.m_objPrintInfo;
            }
            set
            {
                this.m_objPrintInfo = value;
                if (this.m_objPrintInfo != null && this.m_objPrintInfo.m_dtbResult != null)
                {
                    int i = 0;
                    while (i < this.m_objPrintInfo.m_dtbResult.Rows.Count)
                    {
                        DataRow dataRow = this.m_objPrintInfo.m_dtbResult.Rows[i];
                        if (dataRow["result_vchr"].ToString() == "\\")
                        {
                            this.m_objPrintInfo.m_dtbResult.Rows.Remove(dataRow);
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
                string p_strParmValue = null;
                long num = this.m_lngGetCollocate(out p_strParmValue, "4030");
                this.m_mthSetPrintTool(p_strParmValue);
            }
        }

        public clsPrintReport()
		{
			this.m_mthInit();
		}

		public clsPrintReport(string p_strReportGroupID, string p_strApplID, bool p_blnConfirmed)
		{
			this.m_mthInit();
			this.m_mthGetPrintContentFromDB(p_strReportGroupID, p_strApplID, p_blnConfirmed);
			string p_strParmValue = null;
			long num = this.m_lngGetCollocate(out p_strParmValue, "4030");
			this.m_mthSetPrintTool(p_strParmValue);
		}

        private void m_mthInit()
		{
			this.m_printDoc = new PrintDocument();
			this.m_printDoc.PrintPage += new PrintPageEventHandler(this.m_printDoc_PrintPage);
			this.m_printDoc.BeginPrint += new PrintEventHandler(this.m_printDoc_BeginPrint);
			this.m_printDoc.EndPrint += new PrintEventHandler(this.m_printDoc_EndPrint);
		}

        public void m_mthGetPrintContentFromDB(string reportGroupID, string applicationId, bool blnConfirmed)
        {
            try
            {
                clsPrintValuePara clsPrintValuePara = null;
                clsReportObject clsReportObject = null;
                lisprintBiz biz = new lisprintBiz();

                long num = biz.m_lngGetReportObject(null, applicationId, out clsReportObject);

                if (clsReportObject != null && clsReportObject.bytReportObjectArr != null)
                {
                    Stream stream = new MemoryStream(clsReportObject.bytReportObjectArr);
                    IFormatter formatter = new BinaryFormatter();
                    clsPrintValuePara = (formatter.Deserialize(stream) as clsPrintValuePara);
                    stream.Close();
                }
                if (clsPrintValuePara == null)
                {
                    biz.m_lngGetReportPrintInfo(reportGroupID, applicationId, blnConfirmed, out clsPrintValuePara);
                }
                if (clsPrintValuePara != null)
                {
                    this.m_strReportGroupID = reportGroupID;
                    this.m_ObjPrintInfo = clsPrintValuePara;
                }
            }
            catch (Exception ex)
            {
                ExceptionLog.OutPutException("m_mthGetPrintContentFromDB-->"+ex);
            }
        }

        private void m_mthSetPrintTool(string p_strParmValue)
        {
            this.m_objPrintTool = clsPrintToolFactory.Create(this.m_strReportGroupID);   
        }

        private void m_printDoc_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
		{
			if (this.m_objPrintTool != null)
			{
				this.m_objPrintTool.m_mthInitPrintTool(this.m_printDoc);
				this.m_objPrintTool.m_mthBeginPrint(this.m_ObjPrintInfo);
            }
		}

		private void m_printDoc_PrintPage(object sender, PrintPageEventArgs e)
		{
			if (this.m_objPrintTool != null)
			{
				this.m_objPrintTool.m_mthPrintPage(e);
			}
		}

		private void m_printDoc_EndPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
		{

	        if (this.m_objPrintInfo != null && this.m_objPrintInfo.m_dtbBaseInfo != null)
			{
				string text = this.m_objPrintInfo.m_dtbBaseInfo.Rows[0]["application_id_chr"].ToString().Trim();
				if (!string.IsNullOrEmpty(text))
				{
					long num = this.m_mthWriteReportPrintState(text);
				}
			}

			if (this.m_objPrintTool != null)
			{
				this.m_objPrintTool.m_mthEndPrint(e);
			}
		}

		public long m_mthWriteReportPrintState(string p_strApplicaionID)
		{
            lisprintBiz biz = new lisprintBiz();
            return biz.m_lngUpdatePrinctTime(null,p_strApplicaionID);
		}

		public void m_mthPrintPreview()
		{
			try
			{
				this.m_printPrev = new PrintPreviewDialog();
				this.m_printPrev.Document = this.m_printDoc;
				this.m_printPrev.PrintPreviewControl.Zoom = 1.0;
				this.m_printPrev.WindowState = FormWindowState.Maximized;
				this.m_printPrev.ShowDialog();
			}
			catch
			{
				MessageBox.Show("打印预览失败！");
			}
		}

		public void m_mthPrint()
		{
			this.m_mthPrint(string.Empty);
		}

		public void m_mthPrint(string printerName)
		{
            PrintController printController = new StandardPrintController();
            this.m_printDoc.PrintController = printController;
			try
			{
				if (this.dsReport != null)
				{
					this.dsReport.Print(false);
					if (this.m_objPrintInfo != null)
					{
						string text = this.m_objPrintInfo.m_dtbBaseInfo.Rows[0]["application_id_chr"].ToString().Trim();
						if (!string.IsNullOrEmpty(text))
						{
							long num = this.m_mthWriteReportPrintState(text);
						}
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(printerName))
					{
						this.m_printDoc.PrinterSettings.PrinterName = printerName;
					}

				    this.m_printDoc.Print();
				}
			}
			catch(Exception e)
			{
                ExceptionLog.OutPutException("m_mthPrint-->"+e);
			}
		}

		public long m_lngGetCollocate(out string p_strFlag, string p_strSetID)
		{
			p_strFlag = null;
			IPrincipal p_objPrincipal = null;
            lisprintBiz biz = new lisprintBiz();
            return biz.m_lngGetCollocate(p_objPrincipal, out p_strFlag, p_strSetID);
		}

		public void m_mthLoadDWReport(DataStore p_dsReport, string dwObject, string p_strParmValue)
		{
			DataTable dataTable = new DataTable();
			DataTable table = new DataTable();
			dataTable = this.m_objPrintInfo.m_dtbBaseInfo;
			table = this.m_objPrintInfo.m_dtbResult;
			this.dsReport.LibraryList = Application.StartupPath + "\\pb_lis.pbl";
			this.dsReport.DataWindowObject = dwObject;
			this.dsReport.InsertRow(0);
			string text = dataTable.Rows[0]["patient_type_chr"].ToString().Trim();
			string text2 = text;
            if (text2 != null)
            {
                if (text2 == "2")
                {
                    this.dsReport.Modify("t_4_2.visible=true");
                    this.dsReport.Modify("t_4_1.visible=false");
                    this.dsReport.Modify("t_4_3.visible=false");
                    this.dsReport.Modify("st_bihno.text = '" + dataTable.Rows[0]["patientcardid_chr"].ToString().Trim() + "'");
                }
                if (text2 == "3")
                {
                    this.dsReport.Modify("t_4_3.visible=true");
                    this.dsReport.Modify("t_4_2.visible=false");
                    this.dsReport.Modify("t_4_1.visible=false");
                    this.dsReport.Modify("st_bihno.text = '" + dataTable.Rows[0]["patient_inhospitalno_chr"].ToString().Trim() + "'");
                }
            }
            else
            {
                this.dsReport.Modify("t_4_1.visible=true");
                this.dsReport.Modify("t_4_2.visible=false");
                this.dsReport.Modify("t_4_3.visible=false");
                this.dsReport.Modify("st_bihno.text = '" + dataTable.Rows[0]["patient_inhospitalno_chr"].ToString().Trim() + "'");
            }
			
			this.dsReport.Modify("p_1.filename = '" + Application.StartupPath + "\\Picture\\东莞茶山医院图标.jpg'");
			this.dsReport.Modify("st_name.text = '" + dataTable.Rows[0]["patient_name_vchr"].ToString().Trim() + "'");
			this.dsReport.Modify("st_sex.text = '" + dataTable.Rows[0]["sex_chr"].ToString().Trim() + "'");
			this.dsReport.Modify("st_age.text = '" + dataTable.Rows[0]["age_chr"].ToString().Trim() + "'");
			this.dsReport.Modify("st_checkno.text = '" + dataTable.Rows[0]["check_no_chr"].ToString().Trim() + "'");
			this.dsReport.Modify("st_dept.text = '" + dataTable.Rows[0]["deptname_vchr"].ToString().Trim() + "'");
			this.dsReport.Modify("st_bedno.text = '" + dataTable.Rows[0]["bedno_chr"].ToString().Trim() + "'");
			this.dsReport.Modify("st_sample.text = '" + dataTable.Rows[0]["sample_type_desc_vchr"].ToString().Trim() + "'");
			DataView dataView = new DataView(table);
			dataView.RowFilter = "is_graph_result_num = 0";
			dataView.Sort = "sample_print_seq_int asc";
			int num = 21;
			int num2 = 0;
			while (num2 < dataView.Count && num < 221)
			{
				DataRowView dataRowView = dataView[num2];
				this.dsReport.Modify("t_" + Convert.ToString(num++) + ".text = '" + dataRowView["device_check_item_name_vchr"].ToString().Trim() + "'");
				this.dsReport.Modify("t_" + Convert.ToString(num++) + ".text = '" + dataRowView["rptno_chr"].ToString().Trim() + "'");
				string text3 = dataRowView["result_vchr"].ToString().Trim();
				string text4 = dataRowView["min_val_dec"].ToString().Trim();
				string text5 = dataRowView["max_val_dec"].ToString().Trim();
				if (!string.IsNullOrEmpty(text4) && !string.IsNullOrEmpty(text5))
				{
					if (decimal.Parse(text3) > decimal.Parse(text5))
						text3 = text3.PadRight(6, ' ') + "↑";
					else
					{
						if (decimal.Parse(text3) < decimal.Parse(text4))
						{
							if (text3.Contains(">") || text3.Contains("<"))
								text3 = text3.PadRight(6, ' ') + "↑";
							else
								text3 = text3.PadRight(6, ' ') + "↓";
						}
					}
				}
				this.dsReport.Modify("t_" +  Convert.ToString(num++) + ".text = '" + text3 + "'");
				this.dsReport.Modify("t_" + Convert.ToString(num++) + ".text = '" + dataRowView["refrange_vchr"].ToString().Trim() +"'");
				this.dsReport.Modify("t_" + Convert.ToString(num++) + ".text = '" + dataRowView["unit_vchr"].ToString().Trim() + "'");
				num2++;
			}
			dataView.RowFilter = "is_graph_result_num = 1";
			dataView.Sort = "sample_print_seq_int asc";
			MemoryStream memoryStream = null;
			for (int i = 0; i < dataView.Count; i++)
			{
				DataRowView dataRowView = dataView[i];
				if (!(dataRowView["graph_img"] is DBNull))
				{
					try
					{
						memoryStream = new MemoryStream(dataRowView["graph_img"] as byte[]);
						Image image = Image.FromStream(memoryStream, true);
						image.Save("D:\\\\code\\\\0001.jpg", ImageFormat.Jpeg);
						image.Dispose();
						this.dsReport.Modify("p_img.filename = 'D:\\code\\0001.jpg'");
						break;
					}
					catch (Exception ex)
					{
                        ExceptionLog.OutPutException("m_mthLoadDWReport-->"+ex);
					}
					finally
					{
						if (memoryStream != null)
						{
							memoryStream.Close();
						}
					}
				}
			}
			this.dsReport.Modify("t_confirmdate.text = '" + DateTime.Parse(dataTable.Rows[0]["confirm_dat"].ToString().Trim()).ToString("yyyy-MM-dd") + "'");
			this.dsReport.Modify("t_reportor.text = '" + dataTable.Rows[0]["reportor"].ToString().Trim() + "'");
			this.dsReport.Modify("t_confirmer.text = '" + dataTable.Rows[0]["confirmer"].ToString().Trim() + "'");
		}
    }
}

