using com.digitalwave.controls;
using com.digitalwave.iCare.gui.LIS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using weCare.Core.Utils;

namespace autoprintlis
{
    public class clsUnifyReportPrint : infPrintRecord  
    {
        private float m_fltPaperWidth;
        private float m_fltPaperHeight;
        private float m_fltPrintWidth;
        private float m_fltPrintHeight;
        private float m_fltStartX;
        private float m_fltEndY;
        private float m_fltTitleSpace;
        private float m_fltItemSpace;
        private float m_fltImgSpace;
        private float m_fltXRate = 0.6f;
        private float m_fltYRate = 0.45f;
        private string m_strPatientName = "姓名:";
        private string m_strSex = "性别:";
        private string m_strAge = "年龄:";
        private string m_strInPatientNo = "住院号:";
        private string m_strDepartment = "科室:";
        private string m_strBedNo = "床号:";
        private string m_strSampleType = "样本类型:";
        private string m_strApplyDoc = "送检医生:";
        private string m_strDiagnose = "临床诊断:";
        private string m_strSampleID = "样本号:";
        private string m_strCheckNo = "检验编号:";
        private string m_strCheckDate = "送检日期:";
        private string m_strSummary = "实验室提示:";
        private string m_strNotice = "祝您身体健康!此报告仅对检测标本负责,结果供医生参考!";
        private string m_strAnnotation = "附注:";
        private string m_strReportDate = "报告日期:";
        private string m_strCheckDoc = "检验者:";
        private string m_strConfirmEmp = "审核者:";
        private string m_strResult = "结    果";
        private string m_strReference = "参考区间";
        private string m_strResultUnit = "单位";
        private Font m_fntTitle;
        private Font m_fntSmallBold;
        private Font m_fntSmallNotBold;
        private Font m_fntSmall2NotBold;
        private Font m_fntHeadNotBold;
        private Font m_fntSmall2Bold;
        private Font m_fntsamll3NotBold;
        public DataTable m_dtbSample;
        public DataTable m_dtbResult;
        private clsCommonPrintMethod m_printMethodTool;
        private float m_fltY;
        private bool m_blnDocked = true;
        private bool m_blnPrintPIc;
        private clsPrintPerPageInfo[] m_objPrintPage;
        private int m_intCurrentPageIdx = 0;
        private int m_intTotalPage = 0;
        private bool m_blnSummaryEmptyVisible = false;
        private bool m_blnAnnotationEmptyVisible = false;
        private int BillStyle = 0;
        public static bool blnSurePrintDiagnose = false;
        private Image objImage;
        public bool IsDocked { get; set; }

        private List<string> lstAppUnitID{ get; set; }
        private EntityAppUnit CurrAppUnit{ get; set; }

        public clsUnifyReportPrint()
        {
            string filename = Application.StartupPath + "\\Picture\\茶山log.bmp";
            this.objImage = Image.FromFile(filename, false);
            try
            {
                string filename2 = Application.StartupPath + "\\LIS_GUI.dll.config";
                ConfigXmlDocument configXmlDocument = new ConfigXmlDocument();
                configXmlDocument.Load(filename2);
                string a = configXmlDocument["configuration"]["appSettings"].SelectSingleNode("add[@key=\"IsPrintPic\"]").Attributes["value"].Value.ToString();
                if (a == "1")
                {
                    this.m_blnPrintPIc = true;
                }
                else
                {
                    this.m_blnPrintPIc = false;
                }
            }
            catch (Exception ex)
            {
                ExceptionLog.OutPutException("clsUnifyReportPrint-->" + ex);
            }
        }
        private string GetAllergenRemarkInfo(string appId)
        {
            string result;
            try
            {
                if (this.lstAppUnitID == null || this.lstAppUnitID.Count == 0)
                {
                    this.CurrAppUnit = null;
                    result = "";
                }
                else
                {
                    if (this.CurrAppUnit != null && this.CurrAppUnit.appId == appId)
                    {
                        result = this.CurrAppUnit.remarkInfo;
                    }
                    else
                    {
                        this.CurrAppUnit = null;
                        lisprintBiz biz = new lisprintBiz();
                        List<string> appUnitIdByAppId = biz.GetAppUnitIdByAppId(appId);
                        if (appUnitIdByAppId != null && appUnitIdByAppId.Count > 0)
                        {
                            foreach (string current in appUnitIdByAppId)
                            {
                                if (this.lstAppUnitID.IndexOf(current) >= 0)
                                {
                                    this.CurrAppUnit = new EntityAppUnit();
                                    this.CurrAppUnit.appId = appId;
                                    string text = string.Empty + Environment.NewLine;
                                    text = text + "0:无[0.00-0.34 IU/ml]\t\t\t\t1:低[0.35-0.69 IU/ml]\t\t2:增加[0.70-3.49 IU/ml]" + Environment.NewLine;
                                    text = text + "3:显著增加[3.50-17.49 IU/ml]\t\t4:高[17.5-49.9 IU/ml]\t\t5:较高[50.0-100.0 IU/ml]" + Environment.NewLine;
                                    text += "6:极高[>100 IU/ml]";
                                    this.CurrAppUnit.remarkInfo = text;
                                    result = this.CurrAppUnit.remarkInfo;
                                    return result;
                                }
                            }
                        }
                        result = "";
                    }
                }
            }
            catch
            {
                result = "";
            }
            return result;
        }
        private void m_mthInitalPrintTool(PrintDocument p_printDoc)
        {
            Rectangle bounds = p_printDoc.DefaultPageSettings.Bounds;
            this.m_fltPaperWidth = (float)bounds.Width;
            bounds = p_printDoc.DefaultPageSettings.Bounds;
            this.m_fltPaperHeight = (float)bounds.Height;
            this.m_fltPrintWidth = this.m_fltPaperWidth * 0.9f;
            this.m_fltPrintHeight = this.m_fltPaperHeight * 0.9f;
            this.m_fltStartX = this.m_fltPaperWidth * 0.05f;
            this.m_fltEndY = this.m_fltPaperHeight - 106;
            this.m_fltTitleSpace = 5f;
            this.m_fltItemSpace = 2f;
            this.m_fltImgSpace = 10f;
            this.m_fntTitle = new Font("SimSun", 16f, FontStyle.Bold);
            this.m_fntSmallBold = new Font("SimSun", 11f, FontStyle.Bold);
            this.m_fntSmall2Bold = new Font("SimSun", 10f, FontStyle.Bold);
            this.m_fntSmallNotBold = new Font("SimSun", 10f, FontStyle.Regular);
            this.m_fntSmall2NotBold = new Font("SimSun", 9f, FontStyle.Regular);
            this.m_fntHeadNotBold = new Font("SimSun", 11f, FontStyle.Regular);
            this.m_fntsamll3NotBold = new Font("SimSun", 8f, FontStyle.Regular);
            lisprintBiz biz = new lisprintBiz();
            this.BillStyle = biz.m_intGetSysParm("4010");

        }
        private Image m_imgDrawGraphic(byte[] p_bytGraph, string p_strImageFormat)
        {
            Image image = null;
            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream(p_bytGraph);
                image = Image.FromStream(memoryStream, true);
                string text = (p_strImageFormat == null) ? null : p_strImageFormat.ToLower();
                string text2 = text;
                if (text2 != null)
                {
                    if (text2 == "lisb")
                    {
                        Bitmap bitmap = new Bitmap(image.Width, image.Height);
                        Graphics graphics = Graphics.FromImage(bitmap);
                        graphics.DrawImage(image, 0, 0, bitmap.Width, bitmap.Height);
                        image.Dispose();
                        image = bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLog.OutPutException("m_imgDrawGraphic-->"+ex);
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Close();
                }
            }
            return image;
        }
        private void m_mthPrintBseInfo()
        {
            if (this.m_dtbSample != null && this.m_dtbSample.Rows.Count >0)
            {
                float fltStartX = this.m_fltStartX;
                float p_fltX = this.m_fltPaperWidth * 0.25f;
                float p_fltX2 = this.m_fltPaperWidth * 0.4f;
                float p_fltX3 = this.m_fltPaperWidth * 0.62f;
                bool flag = com.digitalwave.iCare.gui.HIS.clsPublic.ConvertObjToDecimal(com.digitalwave.iCare.gui.HIS.clsPublic.m_strReadXML("Lis", "IsUseA4", "AnyOne")) == 1m;
                if (flag)
                {
                    this.m_fltY = 30f;
                }
                else
                {
                    this.m_fltY = 5f;
                }
                this.m_printMethodTool.m_mthPrintImage(this.objImage, fltStartX, this.m_fltY);
                this.m_fltY += (float)(this.objImage.Height - 40);
                string text = this.m_dtbSample.Rows[0]["print_title_vchr"].ToString().Trim().Substring(this.m_dtbSample.Rows[0]["print_title_vchr"].ToString().Trim().Length - 5);
                if (!text.Contains("检验报告单"))
                {
                    text = "检验报告单";
                }
                this.m_printMethodTool.m_mthPrintTitle(text, this.m_fntTitle, this.m_fltY, this.m_fltPaperWidth);
                this.m_fltY += 3f + this.m_printMethodTool.m_fltGetStringHeight(this.m_dtbSample.Rows[0]["print_title_vchr"].ToString().Trim(), this.m_fntTitle);
                this.m_printMethodTool.m_mthDrawLine(this.m_fltStartX - 5f, this.m_fltY, this.m_fltPaperWidth * 0.9f, this.m_fltY);
                if (flag)
                {
                    this.m_fltY += 12f;
                }
                else
                {
                    this.m_fltY += 3f;
                }
                this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallBold, this.m_fntHeadNotBold, this.m_strPatientName, this.m_dtbSample.Rows[0]["patient_name_vchr"].ToString().Trim(), fltStartX, this.m_fltY);
                this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallBold, this.m_fntSmallNotBold, this.m_strSex, this.m_dtbSample.Rows[0]["sex_chr"].ToString().Trim(), p_fltX, this.m_fltY);
                this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallBold, this.m_fntSmallNotBold, this.m_strAge, this.m_dtbSample.Rows[0]["age_chr"].ToString().Trim(), p_fltX2, this.m_fltY);
                string text2 = this.m_dtbSample.Rows[0]["patient_type_chr"].ToString().Trim();
                string p_strContent = null;
                string text3 = text2;
                if (text3 != null)
                {
                    if (text3 == "2")
                    {
                        this.m_strInPatientNo = "诊疗卡号:";
                        p_strContent = this.m_dtbSample.Rows[0]["patientcardid_chr"].ToString().Trim();
                    }
                    if (text3 == "3")
                    {
                        this.m_strInPatientNo = "体检号:";
                        p_strContent = this.m_dtbSample.Rows[0]["patient_inhospitalno_chr"].ToString().Trim();
                    }
                }
                else
                {
                    this.m_strInPatientNo = "住院号:";
                    p_strContent = this.m_dtbSample.Rows[0]["patient_inhospitalno_chr"].ToString().Trim();
                }
                
                this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallBold, this.m_fntSmallNotBold, this.m_strInPatientNo, p_strContent, p_fltX3, this.m_fltY);
                this.m_fltY += 5f + this.m_printMethodTool.m_fltGetStringHeight(this.m_strSampleID, this.m_fntSmallBold);
                this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallBold, this.m_fntSmallNotBold, this.m_strDepartment, this.m_dtbSample.Rows[0]["deptname_vchr"].ToString().Trim(), fltStartX, this.m_fltY);
                this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallBold, this.m_fntSmallNotBold, this.m_strBedNo, this.m_dtbSample.Rows[0]["bedno_chr"].ToString().Trim(), p_fltX, this.m_fltY);
                this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallBold, this.m_fntSmallNotBold, this.m_strSampleType, this.m_dtbSample.Rows[0]["sample_type_desc_vchr"].ToString().Trim(), p_fltX2, this.m_fltY);
                string text4 = this.m_dtbSample.Rows[0]["check_no_chr"].ToString().Trim();
                this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallBold, this.m_fntSmallNotBold, this.m_strCheckNo, this.m_dtbSample.Rows[0]["check_no_chr"].ToString().Trim(), p_fltX3, this.m_fltY);
                try
                {
                    if (text4.Length >=2 && text4.Substring(0, 2) == "18")
                    {
                        this.m_strReference = "MIC";
                    }
                    else
                    {
                        this.m_strReference = "参考区间";
                    }
                }
                catch (Exception ex)
                {
                    ExceptionLog.OutPutException("m_mthPrintBseInfo->"+ex);
                }
                this.m_fltY += 5f + this.m_printMethodTool.m_fltGetStringHeight(this.m_strSampleID, this.m_fntSmallBold);
                this.m_printMethodTool.m_mthDrawLine(this.m_fltStartX - 5f, this.m_fltY, this.m_fltPaperWidth * 0.9f, this.m_fltY);
                this.m_fltY += 5f;
            }
        }
        public static int intGetConfig(string strCfgName)
        {
            int result;
            try
            {
                string s = ConfigurationManager.AppSettings[strCfgName];
                int num = int.Parse(s);
                result = num;
            }
            catch (Exception ex)
            {
                result = 0;
                ExceptionLog.OutPutException("intGetConfig-->"+ex);
            }
            return result;
        }
        private float m_fltPrintSummary(float p_fltX, float p_fltY, float p_fltPrintWidth)
        {
            string summaryStr = this.m_dtbSample.Rows[0]["SUMMARY_VCHR"].ToString().Trim() + this.GetAllergenRemarkInfo(this.m_dtbSample.Rows[0]["application_id_chr"].ToString());
            float result;
            if (!this.m_blnSummaryEmptyVisible && string.IsNullOrEmpty(summaryStr))
            {
                result = p_fltY;
            }
            else
            {
                float num = p_fltY + 10f;
                this.m_printMethodTool.m_mthDrawString(this.m_strSummary, this.m_fntSmallBold, p_fltX, num);
                num += (float)this.m_fntSmallBold.Height + this.m_fltTitleSpace;
                SizeF sizeF = this.m_rectGetPrintStringRectangle(this.m_fntSmallBold, this.m_fntSmallNotBold, summaryStr, p_fltPrintWidth, this.m_fltTitleSpace, this.m_fltItemSpace);
                Rectangle rectPrint = new Rectangle((int)p_fltX, (int)num, (int)sizeF.Width, (int)sizeF.Height);
                new clsPrintRichTextContext(Color.Black, this.m_fntSmallNotBold).m_mthPrintText(summaryStr, this.m_dtbSample.Rows[0]["XML_SUMMARY_VCHR"].ToString().Trim(), this.m_fntSmallNotBold, Color.Black, rectPrint, this.m_printMethodTool.m_printEventArg.Graphics);
                num += (float)rectPrint.Height;
                result = num;
            }
            return result;
        }
        private void m_mthPrintEnd()
        {
            if (this.m_blnDocked)
            {
                if (this.m_fltY < this.m_fltEndY)
                {
                    this.m_fltY = this.m_fltEndY;
                }
            }
            float num = 0f;
            num = this.m_fltY;
            num += 10f;
            bool flag = false;
            bool flag2 = com.digitalwave.iCare.gui.HIS.clsPublic.ConvertObjToDecimal(com.digitalwave.iCare.gui.HIS.clsPublic.m_strReadXML("Lis", "IsUseA4", "AnyOne")) == 1m;
            float num2 = 0f;
            string p_str = string.Empty;
            if (flag2)
            {
                num -= 30f;
            }
            if (this.m_dtbSample.Rows[0]["SAMPLING_DATE_DAT"] != DBNull.Value)
            {
                flag = true;
                p_str = this.m_strReportDate;
                this.m_printMethodTool.m_mthDrawString(p_str, this.m_fntSmallBold, this.m_fltStartX, num);
                num2 = this.m_printMethodTool.m_fltGetStringWidth(p_str, this.m_fntSmallBold);
                DateTime dateTime = Convert.ToDateTime(this.m_dtbSample.Rows[0]["CONFIRM_DAT"]);
                p_str = dateTime.ToString("yyyy-MM-dd HH:mm");
                this.m_printMethodTool.m_mthDrawString(p_str, this.m_fntSmallBold, this.m_fltStartX + num2 + 5f, num);
                num2 += this.m_printMethodTool.m_fltGetStringWidth(p_str, this.m_fntSmallBold) + 65f;
            }
            this.m_printMethodTool.m_mthDrawString(this.m_strNotice, this.m_fntSmallNotBold, this.m_fltStartX + num2, num);
            float num3 = this.m_printMethodTool.m_fltGetStringWidth(this.m_strNotice, this.m_fntSmallNotBold);
            bool flag3 = false;
            if (this.m_dtbSample.Rows[0]["ANNOTATION_VCHR"].ToString().Trim() != "" || this.m_blnAnnotationEmptyVisible)
            {
                flag3 = true;
            }
            if (flag3)
            {
                this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallNotBold, this.m_fntSmallNotBold, this.m_strAnnotation, this.m_dtbSample.Rows[0]["ANNOTATION_VCHR"].ToString().Trim(), this.m_fltStartX + num3, num);
            }
            if (flag2)
            {
                num += this.m_printMethodTool.m_fltGetStringHeight(this.m_strAnnotation, this.m_fntSmallNotBold);
                this.m_printMethodTool.m_mthDrawLine(this.m_fltStartX - 5f, num, this.m_fltPaperWidth * 0.9f, num);
                num += 15f;
            }
            else
            {
                num += this.m_printMethodTool.m_fltGetStringHeight(this.m_strAnnotation, this.m_fntSmallNotBold) + 3f;
                this.m_printMethodTool.m_mthDrawLine(this.m_fltStartX - 5f, num, this.m_fltPaperWidth * 0.9f, num);
                num += 6f;
            }
            float fltStartX = this.m_fltStartX;
            float num4 = this.m_fltPaperWidth * 1.4f / 3f;
            float num5 = this.m_fltPaperWidth * 2.1f / 3f;
            if (flag)
            {
                clsCommonPrintMethod arg_374_0 = this.m_printMethodTool;
                Font arg_374_1 = this.m_fntSmallBold;
                Font arg_374_2 = this.m_fntSmallBold;
                string arg_374_3 = "采样时间:";
                DateTime dateTime = Convert.ToDateTime(this.m_dtbSample.Rows[0]["SAMPLING_DATE_DAT"]);
                arg_374_0.m_mthDrawTextAndContent(arg_374_1, arg_374_2, arg_374_3, dateTime.ToString("yyyy-MM-dd HH:mm"), fltStartX, num);
            }
            else
            {
                clsCommonPrintMethod arg_3C5_0 = this.m_printMethodTool;
                Font arg_3C5_1 = this.m_fntSmallBold;
                Font arg_3C5_2 = this.m_fntSmallBold;
                string arg_3C5_3 = this.m_strReportDate;
                DateTime dateTime = Convert.ToDateTime(this.m_dtbSample.Rows[0]["CONFIRM_DAT"]);
                arg_3C5_0.m_mthDrawTextAndContent(arg_3C5_1, arg_3C5_2, arg_3C5_3, dateTime.ToString("yyyy-MM-dd HH:mm"), fltStartX, num);
            }
            if (this.m_dtbSample.Columns.IndexOf("reportorSign") >= 0)
            {
                if (this.m_dtbSample.Rows[0]["reportorSign"] == DBNull.Value)
                {
                    this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallBold, this.m_fntSmallBold, this.m_strCheckDoc, this.m_dtbSample.Rows[0]["reportor"].ToString().Trim(), num4, num);
                }
                else
                {
                    MemoryStream stream = new MemoryStream((byte[])this.m_dtbSample.Rows[0]["reportorSign"]);
                    this.m_printMethodTool.DrawImage(this.m_strCheckDoc, this.m_fntSmallBold, Image.FromStream(stream), num4, num, flag2);
                }
            }
            else
            {
                this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallBold, this.m_fntSmallBold, this.m_strCheckDoc, this.m_dtbSample.Rows[0]["reportor"].ToString().Trim(), num4, num);
            }
            if (this.m_dtbSample.Columns.IndexOf("confirmerSign") >= 0)
            {
                if (this.m_dtbSample.Rows[0]["confirmerSign"] == DBNull.Value)
                {
                    this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallBold, this.m_fntSmallBold, this.m_strConfirmEmp, this.m_dtbSample.Rows[0]["confirmer"].ToString().Trim(), num5, num);
                }
                else
                {
                    MemoryStream stream = new MemoryStream((byte[])this.m_dtbSample.Rows[0]["confirmerSign"]);
                    this.m_printMethodTool.DrawImage(this.m_strConfirmEmp, this.m_fntSmallBold, Image.FromStream(stream), num5, num, flag2);
                }
            }
            else
            {
                this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallBold, this.m_fntSmallBold, this.m_strConfirmEmp, this.m_dtbSample.Rows[0]["confirmer"].ToString().Trim(), num5, num);
            }
        }
        private void m_mthPrintEnd_DGCS()
        {
            float num = 0f;
            num = this.m_fltEndY;
            num += 3f;
            num += 6f;
            float fltStartX = this.m_fltStartX;
            float num2 = this.m_fltPaperWidth * 1.4f / 3f;
            float num3 = this.m_fltPaperWidth * 2.1f / 3f;
            bool flag = false;
            bool flag2 = com.digitalwave.iCare.gui.HIS.clsPublic.ConvertObjToDecimal(com.digitalwave.iCare.gui.HIS.clsPublic.m_strReadXML("Lis", "IsUseA4", "AnyOne")) == 1m;
            if (flag2)
            {
                num -= 30f;
            }
            if (this.m_dtbSample.Rows[0]["SAMPLING_DATE_DAT"] != DBNull.Value)
            {
                flag = true;
            }
            if (flag2)
            {
                this.m_printMethodTool.m_mthDrawLine(this.m_fltStartX - 5f, num, this.m_fltPaperWidth * 0.9f, num);
                num += 12f;
            }
            else
            {
                this.m_printMethodTool.m_mthDrawLine(this.m_fltStartX - 5f, num, this.m_fltPaperWidth * 0.9f, num);
                num += 6f;
            }
            if (flag)
            {
                clsCommonPrintMethod arg_179_0 = this.m_printMethodTool;
                Font arg_179_1 = this.m_fntSmallBold;
                Font arg_179_2 = this.m_fntSmallBold;
                string arg_179_3 = "采样时间:";
                DateTime dateTime = Convert.ToDateTime(this.m_dtbSample.Rows[0]["SAMPLING_DATE_DAT"]);
                arg_179_0.m_mthDrawTextAndContent(arg_179_1, arg_179_2, arg_179_3, dateTime.ToString("yyyy-MM-dd HH:mm"), fltStartX, num);
            }
            else
            {
                clsCommonPrintMethod arg_1C9_0 = this.m_printMethodTool;
                Font arg_1C9_1 = this.m_fntSmallBold;
                Font arg_1C9_2 = this.m_fntSmallBold;
                string arg_1C9_3 = this.m_strReportDate;
                DateTime dateTime = Convert.ToDateTime(this.m_dtbSample.Rows[0]["CONFIRM_DAT"]);
                arg_1C9_0.m_mthDrawTextAndContent(arg_1C9_1, arg_1C9_2, arg_1C9_3, dateTime.ToString("yyyy-MM-dd HH:mm"), fltStartX, num);
            }
            if (this.m_dtbSample.Columns.IndexOf("reportorSign") >= 0)
            {
                if (this.m_dtbSample.Rows[0]["reportorSign"] == DBNull.Value)
                {
                    this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallBold, this.m_fntSmallBold, this.m_strCheckDoc, this.m_dtbSample.Rows[0]["reportor"].ToString().Trim(), num2, num);
                }
                else
                {
                    MemoryStream stream = new MemoryStream((byte[])this.m_dtbSample.Rows[0]["reportorSign"]);
                    this.m_printMethodTool.DrawImage(this.m_strCheckDoc, this.m_fntSmallBold, Image.FromStream(stream), num2, num, flag2);
                }
            }
            else
            {
                this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallBold, this.m_fntSmallBold, this.m_strCheckDoc, this.m_dtbSample.Rows[0]["reportor"].ToString().Trim(), num2, num);
            }
            if (this.m_dtbSample.Columns.IndexOf("confirmerSign") >= 0)
            {
                if (this.m_dtbSample.Rows[0]["confirmerSign"] == DBNull.Value)
                {
                    this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallBold, this.m_fntSmallBold, this.m_strConfirmEmp, this.m_dtbSample.Rows[0]["confirmer"].ToString().Trim(), num3, num);
                }
                else
                {
                    MemoryStream stream = new MemoryStream((byte[])this.m_dtbSample.Rows[0]["confirmerSign"]);
                    this.m_printMethodTool.DrawImage(this.m_strConfirmEmp, this.m_fntSmallBold, Image.FromStream(stream), num3, num, flag2);
                }
            }
            else
            {
                this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallBold, this.m_fntSmallBold, this.m_strConfirmEmp, this.m_dtbSample.Rows[0]["confirmer"].ToString().Trim(), num3, num);
            }
            num += this.m_printMethodTool.m_fltGetStringHeight(this.m_strReportDate, this.m_fntSmallBold) + 6f;
            float num4 = 0f;
            string p_str = string.Empty;
            if (flag)
            {
                p_str = this.m_strReportDate;
                this.m_printMethodTool.m_mthDrawString(p_str, this.m_fntSmallBold, this.m_fltStartX, num);
                num4 = this.m_printMethodTool.m_fltGetStringWidth(p_str, this.m_fntSmallBold);
                DateTime dateTime = Convert.ToDateTime(this.m_dtbSample.Rows[0]["CONFIRM_DAT"]);
                p_str = dateTime.ToString("yyyy-MM-dd HH:mm");
                this.m_printMethodTool.m_mthDrawString(p_str, this.m_fntSmallBold, this.m_fltStartX + num4 + 5f, num);
                num4 += this.m_printMethodTool.m_fltGetStringWidth(p_str, this.m_fntSmallBold) + 65f;
            }
            this.m_printMethodTool.m_printEventArg.Graphics.DrawString(this.m_strNotice, new Font("SimSun", 11f, FontStyle.Regular), Brushes.Red, this.m_fltStartX + num4, num);
            float num5 = this.m_printMethodTool.m_fltGetStringWidth(this.m_strNotice, new Font("SimSun", 11f, FontStyle.Regular));
            bool flag3 = false;
            if (this.m_dtbSample.Rows[0]["annotation_vchr"].ToString().Trim() != "" || this.m_blnAnnotationEmptyVisible)
            {
                flag3 = true;
            }
            if (flag3)
            {
                this.m_printMethodTool.m_mthDrawTextAndContent(this.m_fntSmallNotBold, this.m_fntSmallNotBold, this.m_strAnnotation, this.m_dtbSample.Rows[0]["annotation_vchr"].ToString().Trim(), this.m_fltStartX + num5, num);
            }
        }
        private void m_mthPrintDetail()
        {
            string p_strContent = this.m_dtbSample.Rows[0]["SUMMARY_VCHR"].ToString().Trim() + this.GetAllergenRemarkInfo(this.m_dtbSample.Rows[0]["application_id_chr"].ToString());
            SizeF sizeF = this.m_rectGetPrintStringRectangle(this.m_fntSmallBold, this.m_fntSmallNotBold, p_strContent, this.m_fltPrintWidth, this.m_fltTitleSpace, this.m_fltItemSpace);
            if (this.m_objPrintPage == null)
            {
                this.m_objPrintPage = this.m_objConstructPrintPageInfo(this.m_dtbResult, this.m_fltStartX, this.m_fltY, this.m_fltPrintWidth, this.m_fltPaperHeight - this.m_fltEndY - this.m_fltY, this.m_fltPaperHeight - 123f - (this.m_fltPaperHeight - this.m_fltEndY));
                this.m_intTotalPage = this.m_objPrintPage.Length;
            }
            if (this.m_intCurrentPageIdx == this.m_objPrintPage.Length - 1)
            {
                this.m_printMethodTool.m_printEventArg.HasMorePages = false;
            }
            else
            {
                this.m_printMethodTool.m_printEventArg.HasMorePages = true;
            }
            if (this.m_objPrintPage[this.m_intCurrentPageIdx].m_objSampleArr != null)
            {
                float num = this.m_fltPrintGroupData(this.m_objPrintPage[this.m_intCurrentPageIdx].m_objSampleArr);
                if (num != -1f)
                {
                    this.m_fltY = num;
                }
            }
            if (this.m_blnPrintPIc)
            {
                if (this.m_objPrintPage[this.m_intCurrentPageIdx].m_imgArr != null)
                {
                    float num = this.m_fltPrintImageArr(this.m_objPrintPage[this.m_intCurrentPageIdx].m_imgArr);
                    if (num != -1f)
                    {
                        this.m_fltY = num;
                    }
                }
            }
            if (!this.m_printMethodTool.m_printEventArg.HasMorePages)
            {
                this.m_fltY = this.m_fltPrintSummary(this.m_fltStartX, this.m_fltY, this.m_fltPrintWidth);
            }
        }
        private void m_mthPrintDetail_DGCS()
        {
            string p_strContent = this.m_dtbSample.Rows[0]["summary_vchr"].ToString().Trim() + this.GetAllergenRemarkInfo(this.m_dtbSample.Rows[0]["application_id_chr"].ToString());
            SizeF sizeF = this.m_rectGetPrintStringRectangle(this.m_fntSmallBold, this.m_fntSmallNotBold, p_strContent, this.m_fltPrintWidth, this.m_fltTitleSpace, this.m_fltItemSpace);
            if (this.m_objPrintPage == null)
            {
                this.m_objPrintPage = this.m_objConstructPrintPageInfo(this.m_dtbResult, this.m_fltStartX, this.m_fltY, this.m_fltPrintWidth, this.m_fltEndY - this.m_fltY, this.m_fltEndY - this.m_fltY);
                this.m_intTotalPage = this.m_objPrintPage.Length;
            }
            if (this.m_intCurrentPageIdx == this.m_objPrintPage.Length - 1)
            {
                this.m_printMethodTool.m_printEventArg.HasMorePages = false;
            }
            else
            {
                this.m_printMethodTool.m_printEventArg.HasMorePages = true;
            }
            if (this.m_objPrintPage[this.m_intCurrentPageIdx].m_objSampleArr != null)
            {
                float num = this.m_fltPrintGroupData_DGCS(this.m_objPrintPage[this.m_intCurrentPageIdx].m_objSampleArr);
                if (num != -1f)
                {
                    this.m_fltY = num;
                }
            }
            if (this.m_blnPrintPIc)
            {
                if (this.m_objPrintPage[this.m_intCurrentPageIdx].m_imgArr != null)
                {
                    float num = this.m_fltPrintImageArr(this.m_objPrintPage[this.m_intCurrentPageIdx].m_imgArr);
                    if (num != -1f)
                    {
                        this.m_fltY = num;
                    }
                }
            }
            if (!this.m_printMethodTool.m_printEventArg.HasMorePages)
            {
                this.m_fltY = this.m_fltPrintSummary(this.m_fltStartX, this.m_fltY, this.m_fltPrintWidth);
            }
        }
        private float m_fltPrintGroupData(clsSampleResultInfo[] p_objArr)
        {
            float num = 0f;
            float result;
            if (p_objArr == null)
            {
                result = -1f;
            }
            else
            {
                bool flag = false;
                if (p_objArr[p_objArr.Length - 1].m_fltX > this.m_fltStartX)
                {
                    flag = true;
                }
                float[] array = null;
                float num2;
                if (flag)
                {
                    array = new float[]
					{
						this.m_fltPrintWidth * 0.04f, 
						this.m_fltPrintWidth * 0.25f, 
						this.m_fltPrintWidth * 0.35f
					};
                    num2 = (array[1] - array[0]) * 0.9f;
                }
                else
                {
                    array = new float[]
					{
						this.m_fltPrintWidth * 0.04f, 
						this.m_fltPrintWidth * 0.3f, 
						this.m_fltPrintWidth * 0.45f
					};
                    num2 = (array[1] - array[0]) * 0.5f;
                }
                float num3 = this.m_fltGetPrintElementHeight(this.m_fntSmallBold, this.m_fltTitleSpace);
                float num4 = this.m_fltGetPrintElementHeight(this.m_fntSmall2NotBold, this.m_fltTitleSpace);
                for (int i = 0; i < p_objArr.Length; i++)
                {
                    float num5 = p_objArr[i].m_fltY;
                    float fltX = p_objArr[i].m_fltX;
                    float p_fltX = fltX + array[0];
                    float num6 = fltX + array[1];
                    float p_fltX2 = fltX + array[2];
                    this.m_printMethodTool.m_mthDrawString("代号", this.m_fntSmallBold, fltX, num5);
                    this.m_printMethodTool.m_mthDrawString(p_objArr[i].m_strPrintTitle, this.m_fntSmallBold, p_fltX, num5);
                    this.m_printMethodTool.m_mthDrawString(this.m_strResult, this.m_fntSmallBold, num6, num5);
                    this.m_printMethodTool.m_mthDrawString(this.m_strReference, this.m_fntSmallBold, p_fltX2, num5);
                    num5 += num3;
                    for (int j = 0; j < p_objArr[i].m_intCount; j++)
                    {
                        if (p_objArr[i].m_intStartIdx + j < p_objArr[i].m_dtvResult.Count)
                        {
                            string str = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["result_vchr"].ToString().Trim();
                            string text = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["abnormal_flag_chr"].ToString().Trim();
                            string str2 = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["unit_vchr"].ToString().Trim();
                            string p_str = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["refrange_vchr"].ToString() + " " + str2;
                            string p_str2 = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["rptno_chr"].ToString().Trim();
                            string p_str3 = p_objArr[i].m_dtvResult[p_objArr[i].m_intPageIdx + j]["check_item_english_name_vchr"].ToString().Trim();
                            this.m_printMethodTool.m_mthDrawString(p_str3, this.m_fntSmall2NotBold, fltX, num5);
                            this.m_printMethodTool.m_mthDrawString(p_str2, this.m_fntSmall2NotBold, p_fltX, num5);
                            if (text != null)
                            {
                                Font font = new Font("SimSun", 9f, FontStyle.Bold);
                                string p_str4 = str + " ↑";
                                float num7 = this.m_printMethodTool.m_fltGetStringWidth(p_str4, font);
                                if (text == "H")
                                {
                                    p_str4 = str + " ↑";
                                    float num8 = num6 + num2 - num7;
                                    this.m_printMethodTool.m_mthDrawString(p_str4, font, num6, num5);
                                }
                                else
                                {
                                    if (text == "L")
                                    {
                                        p_str4 = str + " ↓";
                                        float num8 = num6 + num2 - num7;
                                        this.m_printMethodTool.m_mthDrawString(p_str4, font, num6, num5);
                                    }
                                    else
                                    {
                                        p_str4 = str + "  ";
                                        float num8 = num6 + num2 - num7;
                                        this.m_printMethodTool.m_mthDrawString(p_str4, this.m_fntSmall2NotBold, num6, num5);
                                    }
                                }
                            }
                            this.m_printMethodTool.m_mthDrawString(p_str, this.m_fntSmall2NotBold, p_fltX2, num5);
                            num5 += (float)this.m_fntSmall2NotBold.Height + this.m_fltItemSpace;
                            if (num < num5)
                            {
                                num = num5;
                            }
                        }
                    }
                }
                result = num;
            }
            return result;
        }
        private float m_fltPrintGroupData_DGCS(clsSampleResultInfo[] p_objArr)
        {
            float num = 0f;
            float result;
            if (p_objArr == null)
            {
                result = -1f;
            }
            else
            {
                bool flag = false;
                if (p_objArr[p_objArr.Length - 1].m_fltX > this.m_fltStartX)
                {
                    flag = true;
                }
                float[] array = null;
                float num2;
                if (flag)
                {
                    array = new float[]
					{
						this.m_fltPrintWidth * 0.22f, 
						this.m_fltPrintWidth * 0.3f, 
						this.m_fltPrintWidth * 0.375f
					};
                    num2 = (array[1] - array[0]) * 0.9f;
                }
                else
                {
                    array = new float[]
					{
						this.m_fltPrintWidth * 0.3f, 
						this.m_fltPrintWidth * 0.5f, 
						this.m_fltPrintWidth * 0.62f
					};
                    num2 = (array[1] - array[0]) * 0.9f;
                }
                float num3 = this.m_fltGetPrintElementHeight(this.m_fntSmallBold, this.m_fltTitleSpace);
                float num4 = this.m_fltGetPrintElementHeight(this.m_fntSmall2NotBold, this.m_fltTitleSpace);
                Font font = new Font("SimSun", 11f, FontStyle.Regular);
                Font p_fnt = new Font("SimSun", 11f, FontStyle.Regular);
                for (int i = 0; i < p_objArr.Length; i++)
                {
                    float num5 = p_objArr[i].m_fltY;
                    float fltX = p_objArr[i].m_fltX;
                    float num6 = fltX + array[0];
                    float p_fltX = fltX + array[1];
                    float p_fltX2 = fltX + array[2];
                    this.m_printMethodTool.m_mthDrawString(p_objArr[i].m_strPrintTitle, this.m_fntSmallBold, fltX, num5);
                    if (flag)
                    {
                        this.m_strResult = "结果";
                        this.m_printMethodTool.m_mthDrawString(this.m_strResult, this.m_fntSmallBold, num6 + 6f, num5);
                    }
                    else
                    {
                        this.m_strResult = "结     果";
                        this.m_printMethodTool.m_mthDrawString(this.m_strResult, this.m_fntSmallBold, num6 + 60f, num5);
                    }
                    this.m_printMethodTool.m_mthDrawString(this.m_strResultUnit, this.m_fntSmallBold, p_fltX, num5);
                    this.m_printMethodTool.m_mthDrawString(this.m_strReference, this.m_fntSmallBold, p_fltX2, num5);
                    num5 += num3;
                    for (int j = 0; j < p_objArr[i].m_intCount; j++)
                    {
                        if (p_objArr[i].m_intStartIdx + j < p_objArr[i].m_dtvResult.Count)
                        {
                            string text = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["result_vchr"].ToString().Trim();
                            string text2 = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["abnormal_flag_chr"].ToString().Trim();
                            string text3 = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["unit_vchr"].ToString().Trim();
                            string text4 = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["refrange_vchr"].ToString().Trim();
                            string p_str = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["rptno_chr"].ToString().Trim();
                            int num7 = Convert.ToInt32(font.Size);
                            Font font2 = font;
                            for (int k = num7; k > 0; k--)
                            {
                                font2 = new Font(font.Name, (float)k, FontStyle.Regular);
                                float num8 = this.m_printMethodTool.m_fltGetStringWidth(p_str, font2);
                                if (num8 + this.m_fltTitleSpace <= array[0])
                                {
                                    break;
                                }
                            }
                            this.m_printMethodTool.m_mthDrawString(p_str, font2, fltX, num5);
                            if (text2 != null)
                            {
                                Font font3 = new Font("SimSun", 11f, FontStyle.Bold);
                                string p_str2 = text + " ↑";
                                float num9 = this.m_printMethodTool.m_fltGetStringWidth(p_str2, font3);
                                float num10;
                                if (num2 - num9 > 0f)
                                {
                                    num10 = num6 + num2 - num9;
                                }
                                else
                                {
                                    num10 = num6;
                                }
                                if (text2 == "H")
                                {
                                    p_str2 = text + " ↑";
                                    float p_fltX3 = num10;
                                    this.m_printMethodTool.m_mthDrawString(p_str2, font3, p_fltX3, num5);
                                }
                                else
                                {
                                    if (text2 == "L")
                                    {
                                        if (text.Contains(">") || text.Contains("<"))
                                        {
                                            p_str2 = text + " ↑";
                                        }
                                        else
                                        {
                                            p_str2 = text + " ↓";
                                        }
                                        float p_fltX3 = num10;
                                        this.m_printMethodTool.m_mthDrawString(p_str2, font3, p_fltX3, num5);
                                    }
                                    else
                                    {
                                        p_str2 = text + "  ";
                                        float p_fltX3 = num10;
                                        this.m_printMethodTool.m_mthDrawString(p_str2, p_fnt, p_fltX3, num5);
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(text3))
                            {
                                this.m_printMethodTool.m_mthDrawString(text3, this.m_fntSmall2NotBold, p_fltX, num5);
                            }
                            if (!string.IsNullOrEmpty(text4))
                            {
                                this.m_printMethodTool.m_mthDrawString(text4, this.m_fntSmall2NotBold, p_fltX2, num5);
                            }
                            num5 += (float)this.m_fntSmall2NotBold.Height + this.m_fltItemSpace;
                            if (num < num5)
                            {
                                num = num5;
                            }
                        }
                    }
                }
                result = num;
            }
            return result;
        }
        private float m_fltPrintImageArr(clsPrintImage[] p_objArr)
        {
            float num = 0f;
            float result;
            if (p_objArr == null)
            {
                result = -1f;
            }
            else
            {
                for (int i = 0; i < p_objArr.Length; i++)
                {
                    this.m_printMethodTool.m_printEventArg.Graphics.DrawImage(p_objArr[i].m_img, p_objArr[i].m_fltX, p_objArr[i].m_fltY, p_objArr[i].m_fltWidth, p_objArr[i].m_fltHeight);
                    if (num < p_objArr[i].m_fltY + p_objArr[i].m_fltHeight)
                    {
                        num = p_objArr[i].m_fltY + p_objArr[i].m_fltHeight;
                    }
                }
                result = num;
            }
            return result;
        }
        private SizeF m_rectGetPrintStringRectangle(Font p_fntTitle, Font p_fntContent, string p_strContent, float p_fltWidth, float p_fltTitleSpace, float p_fltItemSpace)
        {
            SizeF result;
            if ((p_strContent == "" || p_strContent == null) && !this.m_blnSummaryEmptyVisible)
            {
                result = new SizeF(0f, 0f);
            }
            else
            {
                float num = (float)p_fntTitle.Height;
                float num2 = (float)p_fntContent.Height;
                float height = 0f;
                if (p_strContent != null && p_strContent != "")
                {
                    height = this.m_printMethodTool.m_printEventArg.Graphics.MeasureString(p_strContent, p_fntContent).Height;
                }
                else
                {
                    height = num + p_fltTitleSpace + num2;
                }
                SizeF sizeF = new SizeF(p_fltWidth, height);
                result = sizeF;
            }
            return result;
        }
        private clsPrintPerPageInfo[] m_objConstructPrintPageInfo(DataTable p_dtbResult, float p_fltX, float p_fltY, float p_fltWidth, float p_fltHeight, float p_fltMaxHeight)
        {
            DataView dataView = this.m_dtvFilterRows(p_dtbResult, "IS_GRAPH_RESULT_NUM = 0");
            DataView dataView2 = this.m_dtvFilterRows(p_dtbResult, "IS_GRAPH_RESULT_NUM = 1");
            dataView.Sort = "REPORT_PRINT_SEQ_INT ASC,GROUPID_CHR ASC,SAMPLE_PRINT_SEQ_INT ASC";
            dataView2.Sort = "REPORT_PRINT_SEQ_INT ASC,GROUPID_CHR ASC,SAMPLE_PRINT_SEQ_INT ASC";
            clsSampleResultInfo[] array = this.m_objConstructSampleResultArr(dataView);
            clsPrintImage[] array2 = this.m_objConstructPrintImage(dataView2);
            float num = 0f;
            if (this.m_blnPrintPIc)
            {
                if (array2 != null && array2.Length > 0)
                {
                    num = array2[0].m_fltHeight + 5f;
                }
            }
            int num2 = 0;
            ArrayList arrayList = new ArrayList();
            float num3 = 0f;
            float num4 = 0f;
            float num5 = this.m_fltGetPrintElementHeight(this.m_fntSmallBold, this.m_fltTitleSpace);
            float num6 = this.m_fltGetPrintElementHeight(this.m_fntSmall2NotBold, this.m_fltItemSpace);
            int num7 = dataView.Count;
            float num8 = 0f;
            if ((float)num7 * num6 + (float)array.Length * num5 <= (p_fltHeight - num) * 2f)
            {
                num8 = p_fltHeight - num;
            }
            else
            {
                num8 = p_fltMaxHeight - num;
            }
            ArrayList arrayList2 = new ArrayList();
            bool flag = false;
            for (int i = 0; i < array.Length; i++)
            {
                int j = array[i].m_dtvResult.Count;
                array[i].m_fltHeight = this.m_fltGetPrintGroupHeight(array[i], this.m_fntSmallBold, this.m_fntSmall2NotBold, this.m_fltTitleSpace, this.m_fltItemSpace);
                if (!flag && array[i].m_fltHeight < num8 - num3)
                {
                    array[i].m_fltX = p_fltX;
                    array[i].m_fltY = num3 + p_fltY;
                    array[i].m_intStartIdx = 0;
                    array[i].m_intCount = array[i].m_dtvResult.Count;
                    array[i].m_intPageIdx = num2;
                    num3 += array[i].m_fltHeight + this.m_fltTitleSpace;
                    arrayList2.Add(array[i]);
                    num7 -= array[i].m_intCount;
                }
                else
                {
                    if (num3 >= num8 / 2f && num6 * (float)num7 + this.m_fltImgSpace * (float)num7 + this.m_fltTitleSpace * (float)(array.Length - i - 1) + num5 * (float)(array.Length - i - 1) < num8)
                    {
                        flag = true;
                        array[i].m_fltX = p_fltX + p_fltWidth / 2f;
                        array[i].m_fltY = num4 + p_fltY;
                        array[i].m_intStartIdx = 0;
                        array[i].m_intCount = array[i].m_dtvResult.Count;
                        array[i].m_intPageIdx = num2;
                        num4 += array[i].m_fltHeight + this.m_fltTitleSpace;
                        arrayList2.Add(array[i]);
                        num7 -= array[i].m_intCount;
                    }
                    else
                    {
                        while (j > 0)
                        {
                            if (num5 + num6 < num8 - num3)
                            {
                                int num9 = 1;
                                while ((float)(num9 + 1) * num6 + num5 < num8 - num3)
                                {
                                    if (num9 >= j)
                                    {
                                        break;
                                    }
                                    num9++;
                                }
                                clsSampleResultInfo clsSampleResultInfo = new clsSampleResultInfo(array[i].m_dtvResult);
                                clsSampleResultInfo.m_strPrintTitle = array[i].m_strPrintTitle;
                                clsSampleResultInfo.m_fltX = p_fltX;
                                clsSampleResultInfo.m_fltY = num3 + p_fltY;
                                clsSampleResultInfo.m_intStartIdx = array[i].m_dtvResult.Count - j;
                                clsSampleResultInfo.m_intCount = num9;
                                clsSampleResultInfo.m_intPageIdx = num2;
                                num3 += (float)num9 * num6 + num5 + this.m_fltTitleSpace;
                                arrayList2.Add(clsSampleResultInfo);
                                j -= num9;
                                num7 -= num9;
                            }
                            else
                            {
                                if (num5 + num6 * (float)j < num8 - num4)
                                {
                                    clsSampleResultInfo clsSampleResultInfo = new clsSampleResultInfo(array[i].m_dtvResult);
                                    clsSampleResultInfo.m_strPrintTitle = array[i].m_strPrintTitle;
                                    clsSampleResultInfo.m_fltX = p_fltX + p_fltWidth / 2f;
                                    clsSampleResultInfo.m_fltY = num4 + p_fltY;
                                    clsSampleResultInfo.m_intStartIdx = array[i].m_dtvResult.Count - j;
                                    clsSampleResultInfo.m_intCount = j;
                                    clsSampleResultInfo.m_intPageIdx = num2;
                                    num4 += (float)j * num6 + num5 + this.m_fltTitleSpace;
                                    arrayList2.Add(clsSampleResultInfo);
                                    j -= j;
                                    num7 -= j;
                                }
                                else
                                {
                                    if (num5 + num6 < num8 - num4)
                                    {
                                        int num9 = 1;
                                        while ((float)(num9 + 1) * num6 + num5 < num8 - num4)
                                        {
                                            num9++;
                                        }
                                        clsSampleResultInfo clsSampleResultInfo = new clsSampleResultInfo(array[i].m_dtvResult);
                                        clsSampleResultInfo.m_strPrintTitle = array[i].m_strPrintTitle;
                                        clsSampleResultInfo.m_fltX = p_fltX + p_fltWidth / 2f;
                                        clsSampleResultInfo.m_fltY = num4 + p_fltY;
                                        clsSampleResultInfo.m_intStartIdx = array[i].m_dtvResult.Count - j;
                                        clsSampleResultInfo.m_intCount = num9;
                                        clsSampleResultInfo.m_intPageIdx = num2;
                                        num4 += (float)num9 * num6 + num5 + this.m_fltTitleSpace;
                                        arrayList2.Add(clsSampleResultInfo);
                                        j -= num9;
                                        num7 -= num9;
                                    }
                                    else
                                    {
                                        num3 = 0f;
                                        num4 = 0f;
                                        flag = false;
                                        num2++;
                                        arrayList.Add(arrayList2);
                                        arrayList2 = new ArrayList();
                                        if ((float)num7 * num6 + (float)array.Length * num5 <= p_fltHeight * 2f)
                                        {
                                            num8 = p_fltHeight;
                                        }
                                        else
                                        {
                                            num8 = p_fltMaxHeight;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (arrayList2.Count > 0)
            {
                arrayList.Add(arrayList2);
            }
            float num10 = Math.Max(num3, num4);
            ArrayList arrayList3 = null;
            ArrayList arrayList4 = null;
            if (this.m_blnPrintPIc)
            {
                if (array2 != null && array2.Length > 0)
                {
                    arrayList3 = new ArrayList();
                    arrayList4 = new ArrayList();
                    float num11 = 0f;
                    for (int i = 0; i < array2.Length; i++)
                    {
                        if (array2[i].m_fltHeight >= p_fltMaxHeight || array2[i].m_fltWidth >= p_fltWidth)
                        {
                            break;
                        }
                        bool flag2 = false;
                        while (!flag2)
                        {
                            if (p_fltMaxHeight - num10 > array2[i].m_fltHeight)
                            {
                                if (p_fltWidth - num11 > array2[i].m_fltWidth)
                                {
                                    array2[i].m_fltX = num11 + p_fltX;
                                    array2[i].m_fltY = num10 + p_fltY;
                                    array2[i].m_intPageIdx = num2;
                                    arrayList4.Add(array2[i]);
                                    num11 += array2[i].m_fltWidth + this.m_fltImgSpace + 20f;
                                    flag2 = true;
                                }
                                else
                                {
                                    if (i > 0)
                                    {
                                        num10 += array2[i].m_fltHeight + this.m_fltImgSpace;
                                        num11 = 0f;
                                    }
                                }
                            }
                            else
                            {
                                num11 = 0f;
                                num10 = 0f;
                                if (arrayList4.Count > 0)
                                {
                                    arrayList3.Add(arrayList4);
                                    arrayList4 = new ArrayList();
                                }
                                num2++;
                            }
                        }
                    }
                    if (arrayList4.Count > 0)
                    {
                        arrayList3.Add(arrayList4);
                    }
                }
            }
            string p_strContent = this.m_dtbSample.Rows[0]["summary_vchr"].ToString().Trim() + this.GetAllergenRemarkInfo(this.m_dtbSample.Rows[0]["application_id_chr"].ToString());
            SizeF sizeF = this.m_rectGetPrintStringRectangle(this.m_fntSmallBold, this.m_fntSmallNotBold, p_strContent, this.m_fltPrintWidth, this.m_fltTitleSpace, this.m_fltItemSpace);
            if (sizeF.Height > 0f && sizeF.Height > p_fltMaxHeight - num10)
            {
                num2++;
            }
            clsPrintPerPageInfo[] array3 = new clsPrintPerPageInfo[num2 + 1];
            int num12 = -1;
            if (arrayList3 != null)
            {
                num12 = ((clsPrintImage[])((ArrayList)arrayList3[0]).ToArray(typeof(clsPrintImage)))[0].m_intPageIdx;
            }
            for (int i = 0; i < array3.Length; i++)
            {
                array3[i] = new clsPrintPerPageInfo();
                if (i <= arrayList.Count - 1)
                {
                    array3[i].m_objSampleArr = (clsSampleResultInfo[])((ArrayList)arrayList[i]).ToArray(typeof(clsSampleResultInfo));
                }
                if (arrayList3 != null)
                {
                    if (num12 <= i && i <= num12 + arrayList3.Count - 1)
                    {
                        array3[i].m_imgArr = (clsPrintImage[])((ArrayList)arrayList3[i - num12]).ToArray(typeof(clsPrintImage));
                    }
                }
            }
            return array3;
        }
        private float m_fltGetPrintGroupHeight(clsSampleResultInfo p_objData, Font p_fntTitle, Font p_fntItem, float p_fltTitleSpace, float p_fltItemSpace)
        {
            float num = 0f;
            return num + ((float)p_fntTitle.Height + p_fltTitleSpace + (float)p_objData.m_intCount * ((float)p_fntItem.Height + p_fltItemSpace));
        }
        private float m_fltGetPrintElementHeight(Font p_fnt, float p_fltPrintSpace)
        {
            float num = 0f;
            return num + ((float)p_fnt.Height + p_fltPrintSpace);
        }
        private clsSampleResultInfo[] m_objConstructSampleResultArr(DataView p_dtvData)
        {
            ArrayList arrayList = new ArrayList();
            clsSampleResultInfo[] array = null;
            for (int i = 0; i < p_dtvData.Count; i++)
            {
                if (i > 0)
                {
                    if (p_dtvData[i]["groupid_chr"].ToString().Trim() != p_dtvData[i - 1]["groupid_chr"].ToString().Trim())
                    {
                        arrayList.Add(p_dtvData[i]["groupid_chr"].ToString().Trim());
                    }
                }
                else
                {
                    arrayList.Add(p_dtvData[i]["groupid_chr"].ToString().Trim());
                }
            }
            if (arrayList.Count > 0)
            {
                array = new clsSampleResultInfo[arrayList.Count];
                for (int i = 0; i < arrayList.Count; i++)
                {
                    DataView dataView = new DataView(p_dtvData.Table);
                    dataView.RowFilter = "IS_GRAPH_RESULT_NUM = 0 AND groupid_chr = " + arrayList[i].ToString().Trim();
                    array[i] = new clsSampleResultInfo(dataView);
                    array[i].m_dtvResult.Sort = "SAMPLE_PRINT_SEQ_INT ASC";
                    array[i].m_strPrintTitle = dataView[0]["print_title_vchr"].ToString().Trim();
                    array[i].m_fltHeight = this.m_fltGetPrintGroupHeight(array[i], this.m_fntSmallBold, this.m_fntSmall2NotBold, this.m_fltTitleSpace, this.m_fltItemSpace);
                    array[i].m_intCount = array[i].m_dtvResult.Count;
                }
            }
            clsSampleResultInfo[] result;
            if (array == null)
            {
                result = new clsSampleResultInfo[0];
            }
            else
            {
                result = array;
            }
            return result;
        }
        private clsPrintImage[] m_objConstructPrintImage(DataView p_dtvData)
        {
            int count = p_dtvData.Count;
            ArrayList arrayList = new ArrayList();
            for (int i = 0; i < count; i++)
            {
                if (!(p_dtvData[i]["GRAPH_IMG"] is DBNull))
                {
                    Image image = this.m_imgDrawGraphic((byte[])p_dtvData[i]["GRAPH_IMG"], p_dtvData[i]["GRAPH_FORMAT_NAME_VCHR"].ToString());
                    if (image != null)
                    {
                        clsPrintImage clsPrintImage = new clsPrintImage(image);
                        clsPrintImage.m_fltWidth = this.m_fltXRate * clsPrintImage.m_fltWidth;
                        clsPrintImage.m_fltHeight = this.m_fltYRate * clsPrintImage.m_fltHeight;
                        arrayList.Add(clsPrintImage);
                    }
                }
            }
            return (clsPrintImage[])arrayList.ToArray(typeof(clsPrintImage));
        }
        private DataView m_dtvFilterRows(DataTable p_dtbSource, string p_strFltExp)
        {
            return new DataView(p_dtbSource)
            {
                RowFilter = p_strFltExp
            };
        }
        private void m_mthPrint()
        {
            lisprintBiz biz = new lisprintBiz();
            string parmValue = biz.m_strGetSysparm("7011");
            if (!string.IsNullOrEmpty(parmValue) && parmValue.Trim() != "")
            {
                this.lstAppUnitID = new List<string>();
                this.lstAppUnitID.AddRange(parmValue.Split(';'));
            }

            this.m_mthPrintBseInfo();
            
            string text = biz.m_strGetSysparm("7006");

            if (text != null)
            {
                if (text == "")
                {
                    this.m_strNotice = "祝您身体健康!此报告仅对检测标本负责,结果供医生参考!";
                }
                if (text == "0")
                {
                    this.m_strNotice = "祝您身体健康!此报告仅对检测标本负责,结果供医生参考!";
                }
                if (text == "1")
                {
                    this.m_strNotice = string.Empty;
                }
            }
            else 
                this.m_strNotice = biz.m_strGetSysparm("7006");

            if (this.BillStyle == 0)
            {
                this.m_mthPrintEnd();
                this.m_mthPrintDetail();
            }
            else
            {
                this.m_mthPrintEnd_DGCS();
                this.m_mthPrintDetail_DGCS();
            }
            if (this.m_intTotalPage - 1 > this.m_intCurrentPageIdx)
                this.m_intCurrentPageIdx++;
        }
        public void m_mthInitPrintContent()
        {
        }
        public void m_mthInitPrintTool(object p_objArg)
        {
            this.m_mthInitalPrintTool((PrintDocument)p_objArg);
        }
        public void m_mthDisposePrintTools(object p_objArg)
        {
        }
        public void m_mthBeginPrint(object p_objPrintArg)
        {
            this.m_dtbSample = ((clsPrintValuePara)p_objPrintArg).m_dtbBaseInfo;
            this.m_dtbResult = ((clsPrintValuePara)p_objPrintArg).m_dtbResult;
        }
        public void m_mthPrintPage(object p_objPrintArg)
        {
            this.m_printMethodTool = new clsCommonPrintMethod((PrintPageEventArgs)p_objPrintArg);
            this.m_mthPrint();
        }
        public void m_mthEndPrint(object p_objPrintArg)
        {
        }

    }
}
