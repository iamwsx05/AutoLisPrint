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

        private void m_mthInitalPrintTool(PrintDocument p_printDoc)
        {
            //获取纸张的宽和高
            m_fltPaperWidth = p_printDoc.DefaultPageSettings.Bounds.Width;
            m_fltPaperHeight = p_printDoc.DefaultPageSettings.Bounds.Height;

            //设置打印区域的宽和高
            m_fltPrintWidth = m_fltPaperWidth * 0.9f;
            m_fltPrintHeight = m_fltPaperHeight * 0.9f;
            m_fltStartX = m_fltPaperWidth * 0.05f;
            m_fltEndY = m_fltPaperHeight - 106;    //baojian.mo -2007.9.3 modify

            //设置报告单组打印间隔
            m_fltTitleSpace = 5;
            m_fltItemSpace = 2;
            m_fltImgSpace = 10;

            //设置打印字体
            m_fntTitle = new Font("SimSun", 16, FontStyle.Bold);
            m_fntSmallBold = new Font("SimSun", 11, FontStyle.Bold);
            m_fntSmall2Bold = new Font("SimSun", 10, FontStyle.Bold);

            m_fntSmallNotBold = new Font("SimSun", 10f, FontStyle.Regular);
            m_fntSmall2NotBold = new Font("SimSun", 9f, FontStyle.Regular);
            m_fntHeadNotBold = new Font("SimSun", 11f, FontStyle.Regular);
            m_fntsamll3NotBold = new Font("SimSun", 8f, FontStyle.Regular);

            //get parm value  mobaojian.mo  -2007.09.03 Modify    
            lisprintBiz biz = new lisprintBiz();
            BillStyle = biz.m_intGetSysParm("4010");
        }


        private Image m_imgDrawGraphic(byte[] p_bytGraph, string p_strImageFormat)
        {
            Image img = null;
            System.IO.MemoryStream ms = null;
            try
            {
                ms = new System.IO.MemoryStream(p_bytGraph);
                img = Image.FromStream(ms, true);
                string strFormat = (p_strImageFormat == null) ? null : p_strImageFormat.ToLower();
                switch (strFormat)
                {
                    case "lisb":
                        System.Drawing.Bitmap bm = new Bitmap(img.Width, img.Height);
                        Graphics g = Graphics.FromImage(bm);
                        g.DrawImage(img, 0, 0, bm.Width, bm.Height);
                        img.Dispose();
                        img = bm;
                        break;
                    default:
                        break;
                }
            }
            catch
            {
            }
            finally
            {
                if (ms != null)
                    ms.Close();
            }
            return img;
        }
        private void m_mthPrintBseInfo()
        {
            if (this.m_dtbSample != null && this.m_dtbSample.Rows.Count > 0)
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
                    if (text4.Length >= 2 && text4.Substring(0, 2) == "18")
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
                    ExceptionLog.OutPutException("m_mthPrintBseInfo->" + ex);
                }
                this.m_fltY += 5f + this.m_printMethodTool.m_fltGetStringHeight(this.m_strSampleID, this.m_fntSmallBold);
                this.m_printMethodTool.m_mthDrawLine(this.m_fltStartX - 5f, this.m_fltY, this.m_fltPaperWidth * 0.9f, this.m_fltY);
                this.m_fltY += 5f;
            }

            //if (m_dtbSample == null)
            //    return;


            //float fltColumn1 = m_fltStartX;
            //float fltColumn2 = m_fltPaperWidth * 0.25f;
            //float fltColumn3 = m_fltPaperWidth * 0.40f;
            //float fltColumn4 = m_fltPaperWidth * 0.62f;

            //bool isUseA4 = (com.digitalwave.iCare.gui.HIS.clsPublic.ConvertObjToDecimal(com.digitalwave.iCare.gui.HIS.clsPublic.m_strReadXML("Lis", "IsUseA4", "AnyOne")) == 1 ? true : false);
            //if (isUseA4)
            //{
            //    m_fltY = 30;
            //}
            //else
            //{
            //    m_fltY = 5;
            //}

            ////图标
            //m_printMethodTool.m_mthPrintImage(objImage, fltColumn1, m_fltY);

            ////string m_strTitleImg = m_dtbSample.Rows[0]["print_title_vchr"].ToString().Trim().Remove
            ////    (m_dtbSample.Rows[0]["print_title_vchr"].ToString().Trim().Length - 5);

            //string m_strTitleImg = "东 莞 市 茶 山 医 院";
            //string m_strTitleImgEng = "ChaShan Hospital of DongGuang";

            ////医院名称
            //// m_printMethodTool.m_mthDrawString(m_strTitleImg, m_fntSmallBold, fltColumn1 + objImage.Width, m_fltY + 16);

            ////英文
            //// m_printMethodTool.m_mthDrawString(m_strTitleImgEng, m_fntsamll3NotBold, fltColumn1 + objImage.Width, m_fltY + 30);

            //m_fltY += objImage.Height - 40;

            //string m_strTitle = m_dtbSample.Rows[0]["print_title_vchr"].ToString().Trim().Substring
            //    (m_dtbSample.Rows[0]["print_title_vchr"].ToString().Trim().Length - 5);

            //if (!m_strTitle.Contains("检验报告单"))
            //{
            //    m_strTitle = "检验报告单";
            //}

            ////DrawTitle
            //m_printMethodTool.m_mthPrintTitle(m_strTitle, m_fntTitle, m_fltY, m_fltPaperWidth);

            ////Locate Y
            //m_fltY += 3 + m_printMethodTool.m_fltGetStringHeight(m_dtbSample.Rows[0]["print_title_vchr"].ToString().Trim(), m_fntTitle);
            //m_printMethodTool.m_mthDrawLine(m_fltStartX - 5, m_fltY, m_fltPaperWidth * 0.9f, m_fltY);
            //if (isUseA4)
            //{
            //    //Locate Y
            //    m_fltY += 12;
            //}
            //else
            //{
            //    //Locate Y
            //    m_fltY += 3;
            //}

            ////姓名
            //m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntHeadNotBold, m_strPatientName,
            //    m_dtbSample.Rows[0]["patient_name_vchr"].ToString().Trim(), fltColumn1, m_fltY);


            ////性别
            //m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold,
            //    m_strSex, m_dtbSample.Rows[0]["sex_chr"].ToString().Trim(), fltColumn2, m_fltY);

            ////年龄
            //m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold,
            //    m_strAge, m_dtbSample.Rows[0]["age_chr"].ToString().Trim(), fltColumn3, m_fltY);

            ////住院号、门诊卡号、体检号
            //string strPatientType = m_dtbSample.Rows[0]["patient_type_chr"].ToString().Trim();
            //string strPrintContent = null;
            //switch (strPatientType)
            //{
            //    case "2":
            //        m_strInPatientNo = "诊疗卡号:";
            //        strPrintContent = m_dtbSample.Rows[0]["patientcardid_chr"].ToString().Trim();
            //        break;

            //    case "3":
            //        m_strInPatientNo = "体检号:";
            //        strPrintContent = m_dtbSample.Rows[0]["patient_inhospitalno_chr"].ToString().Trim();
            //        break;

            //    default:
            //        m_strInPatientNo = "住院号:";
            //        strPrintContent = m_dtbSample.Rows[0]["patient_inhospitalno_chr"].ToString().Trim();
            //        break;
            //}


            //m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold, m_strInPatientNo,
            //    strPrintContent, fltColumn4, m_fltY);

            ////Locate Y
            //m_fltY += 5 + m_printMethodTool.m_fltGetStringHeight(m_strSampleID, m_fntSmallBold);


            ////科  室
            //m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold, m_strDepartment,
            //    m_dtbSample.Rows[0]["deptname_vchr"].ToString().Trim(), fltColumn1, m_fltY);


            ////床  号
            //m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold, m_strBedNo,
            //    m_dtbSample.Rows[0]["bedno_chr"].ToString().Trim(), fltColumn2, m_fltY);

            ////样本类型
            //m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold, m_strSampleType,
            //    m_dtbSample.Rows[0]["sample_type_desc_vchr"].ToString().Trim(), fltColumn3, m_fltY);

            ////检验编号
            //string temp_No = m_dtbSample.Rows[0]["check_no_chr"].ToString().Trim();
            //m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold, m_strCheckNo,
            //    m_dtbSample.Rows[0]["check_no_chr"].ToString().Trim(), fltColumn4, m_fltY);
            //try
            //{
            //    if (temp_No.Substring(0, 2) == "18")
            //    {
            //        m_strReference = "MIC";
            //    }
            //    else
            //    {
            //        m_strReference = "参考区间";
            //    }
            //}
            //catch
            //{

            //}

            ////Locate Y
            //m_fltY += 5 + m_printMethodTool.m_fltGetStringHeight(m_strSampleID, m_fntSmallBold);


            //m_printMethodTool.m_mthDrawLine(m_fltStartX - 5, m_fltY, m_fltPaperWidth * 0.9f, m_fltY);

            //m_fltY += 5;

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
            float result;
            if (!this.m_blnSummaryEmptyVisible && this.m_dtbSample.Rows[0]["SUMMARY_VCHR"].ToString().Trim() == "")
            {
                result = p_fltY;
            }
            else
            {
                float num = p_fltY + 10f;
                string p_strContent = this.m_dtbSample.Rows[0]["SUMMARY_VCHR"].ToString().Trim();
                this.m_printMethodTool.m_mthDrawString(this.m_strSummary, this.m_fntSmallBold, p_fltX, num);
                num += (float)this.m_fntSmallBold.Height + this.m_fltTitleSpace;
                SizeF sizeF = this.m_rectGetPrintStringRectangle(this.m_fntSmallBold, this.m_fntSmallNotBold, p_strContent, p_fltPrintWidth, this.m_fltTitleSpace, this.m_fltItemSpace);
                Rectangle rectPrint = new Rectangle((int)p_fltX, (int)num, (int)sizeF.Width, (int)sizeF.Height);
                new clsPrintRichTextContext(Color.Black, this.m_fntSmallNotBold).m_mthPrintText(this.m_dtbSample.Rows[0]["SUMMARY_VCHR"].ToString().Trim(), this.m_dtbSample.Rows[0]["XML_SUMMARY_VCHR"].ToString().Trim(), this.m_fntSmallNotBold, Color.Black, rectPrint, this.m_printMethodTool.m_printEventArg.Graphics);
                num += (float)rectPrint.Height;
                result = num;
            }
            return result;

            //if (!m_blnSummaryEmptyVisible && m_dtbSample.Rows[0]["SUMMARY_VCHR"].ToString().Trim() == "")
            //    return p_fltY;
            //float fltY = p_fltY + 10;
            //string strSummary = m_dtbSample.Rows[0]["SUMMARY_VCHR"].ToString().Trim();
            //m_printMethodTool.m_mthDrawString(m_strSummary, m_fntSmallBold, p_fltX, fltY);
            //fltY += m_fntSmallBold.Height + m_fltTitleSpace;
            //SizeF sf = m_rectGetPrintStringRectangle(m_fntSmallBold, m_fntSmallNotBold, strSummary, p_fltPrintWidth, m_fltTitleSpace,
            //    m_fltItemSpace);
            //Rectangle rectSummary = new Rectangle((int)p_fltX, (int)fltY, (int)sf.Width, (int)sf.Height);
            //new com.digitalwave.controls.clsPrintRichTextContext(Color.Black, m_fntSmallNotBold).m_mthPrintText(m_dtbSample.Rows[0]["SUMMARY_VCHR"].ToString().Trim(),
            //    m_dtbSample.Rows[0]["XML_SUMMARY_VCHR"].ToString().Trim(), m_fntSmallNotBold, Color.Black, rectSummary, m_printMethodTool.m_printEventArg.Graphics);
            //fltY += rectSummary.Height;
            //return fltY;
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




            //if (m_blnDocked)
            //{
            //    if (m_fltY < m_fltEndY)
            //    {
            //        m_fltY = m_fltEndY;
            //    }
            //}
            //float m_fltEnd = 0.0f;
            //m_fltEnd = m_fltY;
            //m_fltEnd += 10;

            //// 是否打印采样时间
            //bool isPrintCYSJ = false;
            //// 是否使用A4
            //bool isUseA4 = (com.digitalwave.iCare.gui.HIS.clsPublic.ConvertObjToDecimal(com.digitalwave.iCare.gui.HIS.clsPublic.m_strReadXML("Lis", "IsUseA4", "AnyOne")) == 1 ? true : false);
            //// 采样时间
            //float diff = 0;
            //string str = string.Empty;

            //if (isUseA4) m_fltEnd -= 30;//50;

            //if (m_dtbSample.Rows[0]["SAMPLING_DATE_DAT"] != DBNull.Value)
            //{
            //    isPrintCYSJ = true;
            //    str = m_strReportDate;      // "采样时间:";
            //    m_printMethodTool.m_mthDrawString(str, m_fntSmallBold, m_fltStartX, m_fltEnd);
            //    diff = m_printMethodTool.m_fltGetStringWidth(str, m_fntSmallBold);
            //    str = Convert.ToDateTime(m_dtbSample.Rows[0]["CONFIRM_DAT"]).ToString("yyyy-MM-dd HH:mm");  // Convert.ToDateTime(m_dtbSample.Rows[0]["SAMPLING_DATE_DAT"]).ToString("yyyy-MM-dd HH:mm");
            //    m_printMethodTool.m_mthDrawString(str, m_fntSmallBold, m_fltStartX + diff + 5, m_fltEnd);
            //    diff += m_printMethodTool.m_fltGetStringWidth(str, m_fntSmallBold) + 65;
            //}
            ////Notice
            //m_printMethodTool.m_mthDrawString(m_strNotice, m_fntSmallNotBold, m_fltStartX + diff, m_fltEnd);
            //float fltNoticeWidth = m_printMethodTool.m_fltGetStringWidth(m_strNotice, m_fntSmallNotBold);
            ////附注
            //bool blnPrintAnnotation = false;
            //if (m_dtbSample.Rows[0]["ANNOTATION_VCHR"].ToString().Trim() != "" || m_blnAnnotationEmptyVisible)
            //{
            //    blnPrintAnnotation = true;
            //}
            //if (blnPrintAnnotation)
            //{
            //    m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallNotBold, m_fntSmallNotBold, m_strAnnotation, m_dtbSample.Rows[0]["ANNOTATION_VCHR"].ToString().Trim(),
            //        m_fltStartX + fltNoticeWidth, m_fltEnd);
            //}
            //if (isUseA4)
            //{
            //    m_fltEnd += m_printMethodTool.m_fltGetStringHeight(m_strAnnotation, m_fntSmallNotBold);
            //    //画线
            //    m_printMethodTool.m_mthDrawLine(m_fltStartX - 5, m_fltEnd, m_fltPaperWidth * 0.9f, m_fltEnd);

            //    m_fltEnd += 15;
            //}
            //else
            //{
            //    m_fltEnd += m_printMethodTool.m_fltGetStringHeight(m_strAnnotation, m_fntSmallNotBold) + 3;
            //    //画线
            //    m_printMethodTool.m_mthDrawLine(m_fltStartX - 5, m_fltEnd, m_fltPaperWidth * 0.9f, m_fltEnd);

            //    m_fltEnd += 6;
            //}

            ////column
            //float fltColumn1 = m_fltStartX;
            //float fltColumn2 = m_fltPaperWidth * 1.4f / 3;
            //float fltColumn3 = m_fltPaperWidth * 2.1f / 3;

            //if (isPrintCYSJ)
            //    //采样时间
            //    m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, "采样时间:", Convert.ToDateTime(m_dtbSample.Rows[0]["SAMPLING_DATE_DAT"]).ToString("yyyy-MM-dd HH:mm"),
            //        fltColumn1, m_fltEnd);
            //else
            //    //报告日期
            //    m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strReportDate, Convert.ToDateTime(m_dtbSample.Rows[0]["CONFIRM_DAT"]).ToString("yyyy-MM-dd HH:mm"),
            //        fltColumn1, m_fltEnd);
            ////检验医生
            //if (m_dtbSample.Columns.IndexOf("reportorSign") >= 0)
            //{
            //    if (m_dtbSample.Rows[0]["reportorSign"] == DBNull.Value)
            //    {
            //        m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strCheckDoc, m_dtbSample.Rows[0]["reportor"].ToString().Trim(), fltColumn2, m_fltEnd);
            //    }
            //    else
            //    {
            //        MemoryStream ms = new MemoryStream((byte[])m_dtbSample.Rows[0]["reportorSign"]);
            //        m_printMethodTool.DrawImage(m_strCheckDoc, m_fntSmallBold, Image.FromStream(ms), fltColumn2, m_fltEnd, isUseA4);
            //    }
            //}
            //else
            //{
            //    m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strCheckDoc, m_dtbSample.Rows[0]["reportor"].ToString().Trim(), fltColumn2, m_fltEnd);
            //}

            ////审核者
            //if (m_dtbSample.Columns.IndexOf("confirmerSign") >= 0)
            //{
            //    if (m_dtbSample.Rows[0]["confirmerSign"] == DBNull.Value)
            //    {
            //        m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strConfirmEmp, m_dtbSample.Rows[0]["confirmer"].ToString().Trim(), fltColumn3, m_fltEnd);
            //    }
            //    else
            //    {
            //        MemoryStream ms = new MemoryStream((byte[])m_dtbSample.Rows[0]["confirmerSign"]);
            //        m_printMethodTool.DrawImage(m_strConfirmEmp, m_fntSmallBold, Image.FromStream(ms), fltColumn3, m_fltEnd, isUseA4);
            //    }
            //}
            //else
            //{
            //    m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strConfirmEmp, m_dtbSample.Rows[0]["confirmer"].ToString().Trim(), fltColumn3, m_fltEnd);
            //}
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
            if (m_blnDocked)
            {
                if (m_fltY < m_fltEndY)
                {
                    m_fltY = m_fltEndY;
                }
            }
            if (m_blnDocked)
            {
                if (m_fltY < m_fltEndY)
                {
                    m_fltY = m_fltEndY;
                }
            }


            //float m_fltEnd = 0.0f;
            //m_fltEnd = m_fltEndY;

            //m_fltEnd += 3;

            ////画线
            ////m_printMethodTool.m_mthDrawLine(m_fltStartX - 5, m_fltY, m_fltPaperWidth * 0.9f, m_fltY);

            //m_fltEnd += 6;

            ////column
            //float fltColumn1 = m_fltStartX;
            //float fltColumn2 = m_fltPaperWidth * 1.4f / 3;
            //float fltColumn3 = m_fltPaperWidth * 2.1f / 3;

            //bool isPrintCYSJ = false;
            //bool isUseA4 = (com.digitalwave.iCare.gui.HIS.clsPublic.ConvertObjToDecimal(com.digitalwave.iCare.gui.HIS.clsPublic.m_strReadXML("Lis", "IsUseA4", "AnyOne")) == 1 ? true : false);
            //if (isUseA4) m_fltEnd -= 30;    // 50;

            //if (m_dtbSample.Rows[0]["SAMPLING_DATE_DAT"] != DBNull.Value)
            //{
            //    isPrintCYSJ = true;
            //}
            //if (isUseA4)
            //{
            //    //m_fltEnd -= 3;
            //    //画线
            //    m_printMethodTool.m_mthDrawLine(m_fltStartX - 5, m_fltEnd, m_fltPaperWidth * 0.9f, m_fltEnd);
            //    m_fltEnd += 12;
            //}
            //else
            //{
            //    //画线
            //    m_printMethodTool.m_mthDrawLine(m_fltStartX - 5, m_fltEnd, m_fltPaperWidth * 0.9f, m_fltEnd);
            //    m_fltEnd += 6;
            //}
            //if (isPrintCYSJ)
            //    //采样时间
            //    m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, "采样时间:", Convert.ToDateTime(m_dtbSample.Rows[0]["SAMPLING_DATE_DAT"]).ToString("yyyy-MM-dd HH:mm"),
            //        fltColumn1, m_fltEnd);
            //else
            //    //报告日期
            //    m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strReportDate, Convert.ToDateTime(m_dtbSample.Rows[0]["CONFIRM_DAT"]).ToString("yyyy-MM-dd HH:mm"),
            //        fltColumn1, m_fltEnd);
            ////检验医生
            //if (m_dtbSample.Columns.IndexOf("reportorSign") >= 0)
            //{
            //    if (m_dtbSample.Rows[0]["reportorSign"] == DBNull.Value)
            //    {
            //        m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strCheckDoc, m_dtbSample.Rows[0]["reportor"].ToString().Trim(), fltColumn2, m_fltEnd);
            //    }
            //    else
            //    {
            //        MemoryStream ms = new MemoryStream((byte[])m_dtbSample.Rows[0]["reportorSign"]);
            //        m_printMethodTool.DrawImage(m_strCheckDoc, m_fntSmallBold, Image.FromStream(ms), fltColumn2, m_fltEnd, isUseA4);
            //    }
            //}
            //else
            //{
            //    m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strCheckDoc, m_dtbSample.Rows[0]["reportor"].ToString().Trim(), fltColumn2, m_fltEnd);
            //}

            ////审核者
            //if (m_dtbSample.Columns.IndexOf("confirmerSign") >= 0)
            //{
            //    if (m_dtbSample.Rows[0]["confirmerSign"] == DBNull.Value)
            //    {
            //        m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strConfirmEmp, m_dtbSample.Rows[0]["confirmer"].ToString().Trim(), fltColumn3, m_fltEnd);
            //    }
            //    else
            //    {
            //        MemoryStream ms = new MemoryStream((byte[])m_dtbSample.Rows[0]["confirmerSign"]);
            //        m_printMethodTool.DrawImage(m_strConfirmEmp, m_fntSmallBold, Image.FromStream(ms), fltColumn3, m_fltEnd, isUseA4);
            //    }
            //}
            //else
            //{
            //    m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strConfirmEmp, m_dtbSample.Rows[0]["confirmer"].ToString().Trim(), fltColumn3, m_fltEnd);
            //}
            //m_fltEnd += m_printMethodTool.m_fltGetStringHeight(m_strReportDate, m_fntSmallBold) + 6;

            //////画线
            ////m_printMethodTool.m_mthDrawLine(m_fltStartX - 5, m_fltEnd, m_fltPaperWidth * 0.9f, m_fltEnd);
            ////m_fltEnd += 6;

            //// 采样时间
            //float diff = 0;
            //string str = string.Empty;
            //if (isPrintCYSJ)
            //{
            //    str = m_strReportDate;  // "采样时间:";
            //    m_printMethodTool.m_mthDrawString(str, m_fntSmallBold, m_fltStartX, m_fltEnd);
            //    diff = m_printMethodTool.m_fltGetStringWidth(str, m_fntSmallBold);
            //    str = Convert.ToDateTime(m_dtbSample.Rows[0]["CONFIRM_DAT"]).ToString("yyyy-MM-dd HH:mm");  // Convert.ToDateTime(m_dtbSample.Rows[0]["SAMPLING_DATE_DAT"]).ToString("yyyy-MM-dd HH:mm");
            //    m_printMethodTool.m_mthDrawString(str, m_fntSmallBold, m_fltStartX + diff + 5, m_fltEnd);
            //    diff += m_printMethodTool.m_fltGetStringWidth(str, m_fntSmallBold) + 65;
            //}
            ////Notice
            //m_printMethodTool.m_printEventArg.Graphics.DrawString(m_strNotice, new Font("SimSun", 11f, FontStyle.Regular), Brushes.Red, m_fltStartX + diff, m_fltEnd);
            ////m_printMethodTool.m_mthDrawString(m_strNotice, m_fntSmallNotBold, m_fltStartX, m_fltY);
            //float fltNoticeWidth = m_printMethodTool.m_fltGetStringWidth(m_strNotice, new Font("SimSun", 11f, FontStyle.Regular));
            ////附注
            //bool blnPrintAnnotation = false;
            //if (m_dtbSample.Rows[0]["annotation_vchr"].ToString().Trim() != "" || m_blnAnnotationEmptyVisible)
            //{
            //    blnPrintAnnotation = true;
            //}
            //if (blnPrintAnnotation)
            //{
            //    m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallNotBold, m_fntSmallNotBold, m_strAnnotation, m_dtbSample.Rows[0]["annotation_vchr"].ToString().Trim(),
            //        m_fltStartX + fltNoticeWidth, m_fltEnd);
            //}
        }
        private void m_mthPrintDetail()
        {
            string p_strContent = this.m_dtbSample.Rows[0]["SUMMARY_VCHR"].ToString().Trim();
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

            //string strSummary = m_dtbSample.Rows[0]["SUMMARY_VCHR"].ToString().Trim();
            //SizeF sf = m_rectGetPrintStringRectangle(m_fntSmallBold, m_fntSmallNotBold, strSummary, m_fltPrintWidth, m_fltTitleSpace,
            //    m_fltItemSpace);
            //if (m_objPrintPage == null)
            //{
            //    m_objPrintPage = m_objConstructPrintPageInfo(m_dtbResult, m_fltStartX, m_fltY, m_fltPrintWidth
            //        , m_fltPaperHeight - m_fltEndY - m_fltY, m_fltPaperHeight - 123 - (m_fltPaperHeight - m_fltEndY));
            //    m_intTotalPage = m_objPrintPage.Length;
            //}
            //if (m_intCurrentPageIdx == m_objPrintPage.Length - 1)
            //{
            //    m_printMethodTool.m_printEventArg.HasMorePages = false;
            //}
            //else
            //{
            //    m_printMethodTool.m_printEventArg.HasMorePages = true;
            //}
            //if (m_objPrintPage[m_intCurrentPageIdx].m_objSampleArr != null)
            //{
            //    //打印结果数据
            //    float fltY = m_fltPrintGroupData(m_objPrintPage[m_intCurrentPageIdx].m_objSampleArr);
            //    if (fltY != -1)
            //        m_fltY = fltY;
            //}

            //if (m_blnPrintPIc)
            //{
            //    if (m_objPrintPage[m_intCurrentPageIdx].m_imgArr != null)
            //    {
            //        //打印图形数据
            //        float fltY = m_fltPrintImageArr(m_objPrintPage[m_intCurrentPageIdx].m_imgArr);
            //        if (fltY != -1)
            //            m_fltY = fltY;
            //    }
            //}
            //if (m_printMethodTool.m_printEventArg.HasMorePages == false)
            //{
            //    m_fltY = m_fltPrintSummary(m_fltStartX, m_fltY, m_fltPrintWidth);
            //}
        }
        private void m_mthPrintDetail_DGCS()
        {
            string p_strContent = this.m_dtbSample.Rows[0]["summary_vchr"].ToString().Trim();
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
            //string strSummary = m_dtbSample.Rows[0]["summary_vchr"].ToString().Trim();
            //SizeF sf = m_rectGetPrintStringRectangle(m_fntSmallBold, m_fntSmallNotBold, strSummary, m_fltPrintWidth, m_fltTitleSpace,
            //    m_fltItemSpace);
            //if (m_objPrintPage == null)
            //{
            //    m_objPrintPage = m_objConstructPrintPageInfo(m_dtbResult, m_fltStartX, m_fltY, m_fltPrintWidth
            //        , m_fltEndY - m_fltY, m_fltEndY - m_fltY);
            //    m_intTotalPage = m_objPrintPage.Length;
            //}
            //if (m_intCurrentPageIdx == m_objPrintPage.Length - 1)
            //{
            //    m_printMethodTool.m_printEventArg.HasMorePages = false;
            //}
            //else
            //{
            //    m_printMethodTool.m_printEventArg.HasMorePages = true;
            //}
            //if (m_objPrintPage[m_intCurrentPageIdx].m_objSampleArr != null)
            //{
            //    //打印结果数据
            //    float fltY = m_fltPrintGroupData_DGCS(m_objPrintPage[m_intCurrentPageIdx].m_objSampleArr);
            //    if (fltY != -1)
            //        m_fltY = fltY;
            //}
            //if (m_blnPrintPIc)
            //{
            //    if (m_objPrintPage[m_intCurrentPageIdx].m_imgArr != null)
            //    {
            //        //打印图形数据
            //        float fltY = m_fltPrintImageArr(m_objPrintPage[m_intCurrentPageIdx].m_imgArr);
            //        if (fltY != -1)
            //            m_fltY = fltY;
            //    }
            //}
            //if (m_printMethodTool.m_printEventArg.HasMorePages == false)
            //{
            //    m_fltY = m_fltPrintSummary(m_fltStartX, m_fltY, m_fltPrintWidth);
            //}
        }
        private float m_fltPrintGroupData(clsSampleResultInfo[] p_objArr)
        {
            float fltY = 0;
            if (p_objArr == null)
                return -1;
            bool blnHasTwoPart = false;
            if (p_objArr[p_objArr.Length - 1].m_fltX > m_fltStartX)
                blnHasTwoPart = true;
            float[] fltColumnArr = null;
            float fltResultPrintWidth;
            if (blnHasTwoPart)
            {
                fltColumnArr = new float[] { m_fltPrintWidth * 0.04f, m_fltPrintWidth * 0.25f, m_fltPrintWidth * 0.35f };
                fltResultPrintWidth = (fltColumnArr[1] - fltColumnArr[0]) * 0.9f;
            }
            else
            {
                fltColumnArr = new float[] { m_fltPrintWidth * 0.04f, m_fltPrintWidth * 0.30f, m_fltPrintWidth * 0.45f };
                fltResultPrintWidth = (fltColumnArr[1] - fltColumnArr[0]) * 0.5f;
            }

            float fltBseY;
            float fltTitleHeight = m_fltGetPrintElementHeight(m_fntSmallBold, m_fltTitleSpace);
            float fltItemHeight = m_fltGetPrintElementHeight(m_fntSmall2NotBold, m_fltTitleSpace);
            for (int i = 0; i < p_objArr.Length; i++)
            {
                fltBseY = p_objArr[i].m_fltY;
                float fltColumn1 = p_objArr[i].m_fltX;
                float fltColumn2 = fltColumn1 + fltColumnArr[0];
                float fltColumn3 = fltColumn1 + fltColumnArr[1];
                float fltColumn4 = fltColumn1 + fltColumnArr[2];

                //打印标题
                m_printMethodTool.m_mthDrawString("代号", m_fntSmallBold, fltColumn1, fltBseY);
                m_printMethodTool.m_mthDrawString(p_objArr[i].m_strPrintTitle, m_fntSmallBold, fltColumn2, fltBseY);
                m_printMethodTool.m_mthDrawString(m_strResult, m_fntSmallBold, fltColumn3, fltBseY);
                m_printMethodTool.m_mthDrawString(m_strReference, m_fntSmallBold, fltColumn4, fltBseY);
                fltBseY += fltTitleHeight;
                for (int j = 0; j < p_objArr[i].m_intCount; j++)
                {
                    if ((p_objArr[i].m_intStartIdx + j) < p_objArr[i].m_dtvResult.Count)
                    {
                        string strResult = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["result_vchr"].ToString().Trim();
                        string strAbnormal = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["abnormal_flag_chr"].ToString().Trim();
                        string strUnit = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["unit_vchr"].ToString().Trim();
                        string strRefRange = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["refrange_vchr"].ToString() + " " + strUnit;
                        string strCheckItemName = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["rptno_chr"].ToString().Trim();
                        string strEnglisName = p_objArr[i].m_dtvResult[p_objArr[i].m_intPageIdx + j]["check_item_english_name_vchr"].ToString().Trim();

                        //打印英文名称
                        m_printMethodTool.m_mthDrawString(strEnglisName, m_fntSmall2NotBold, fltColumn1, fltBseY);

                        //打印项目
                        m_printMethodTool.m_mthDrawString(strCheckItemName, m_fntSmall2NotBold, fltColumn2, fltBseY);

                        //异常标志
                        if (strAbnormal != null)
                        {
                            System.Drawing.Font objBoldFont = new Font("SimSun", 9, FontStyle.Bold);
                            string strPR;

                            strPR = strResult + " " + "↑";
                            float fltResultWidth = m_printMethodTool.m_fltGetStringWidth(strPR, objBoldFont);

                            if (strAbnormal == "H")
                            {
                                strPR = strResult + " " + "↑";
                                float fltStartPos = fltColumn3 + fltResultPrintWidth - fltResultWidth;
                                m_printMethodTool.m_mthDrawString(strPR, objBoldFont, fltColumn3, fltBseY);
                            }
                            else if (strAbnormal == "L")
                            {
                                // 20160913
                                //if (strResult.Contains(">") || strResult.Contains("<"))
                                //    strPR = strResult + " " + "↑";
                                //else
                                strPR = strResult + " " + "↓";
                                float fltStartPos = fltColumn3 + fltResultPrintWidth - fltResultWidth;
                                m_printMethodTool.m_mthDrawString(strPR, objBoldFont, fltColumn3, fltBseY);
                            }
                            else
                            {
                                strPR = strResult + " " + " ";
                                float fltStartPos = fltColumn3 + fltResultPrintWidth - fltResultWidth;
                                m_printMethodTool.m_mthDrawString(strPR, m_fntSmall2NotBold, fltColumn3, fltBseY);
                            }
                        }
                        m_printMethodTool.m_mthDrawString(strRefRange, m_fntSmall2NotBold, fltColumn4, fltBseY);

                        //Locate Y 
                        fltBseY += m_fntSmall2NotBold.Height + m_fltItemSpace;
                        if (fltY < fltBseY)
                        {
                            fltY = fltBseY;
                        }
                    }
                }
            }
            return fltY;
        }
        private float m_fltPrintGroupData_DGCS(clsSampleResultInfo[] p_objArr)
        {
            float fltY = 0;
            if (p_objArr == null)
                return -1;
            bool blnHasTwoPart = false;
            if (p_objArr[p_objArr.Length - 1].m_fltX > m_fltStartX)
                blnHasTwoPart = true;
            float[] fltColumnArr = null;
            float fltResultPrintWidth;
            if (blnHasTwoPart)
            {
                fltColumnArr = new float[] { m_fltPrintWidth * 0.22f, m_fltPrintWidth * 0.30f, m_fltPrintWidth * 0.375f };
                fltResultPrintWidth = (fltColumnArr[1] - fltColumnArr[0]) * 0.9f;
            }
            else
            {
                fltColumnArr = new float[] { m_fltPrintWidth * 0.30f, m_fltPrintWidth * 0.50f, m_fltPrintWidth * 0.62f };
                fltResultPrintWidth = (fltColumnArr[1] - fltColumnArr[0]) * 0.90f;
            }

            float fltBseY;
            float fltTitleHeight = m_fltGetPrintElementHeight(m_fntSmallBold, m_fltTitleSpace);
            float fltItemHeight = m_fltGetPrintElementHeight(m_fntSmall2NotBold, m_fltTitleSpace);

            float fltItemNameWidth;    //记录检验项目名称的宽度
            Font m_fntItemName = new Font("SimSun", 11f, FontStyle.Regular);
            Font m_fntResultNotBold = new Font("SimSun", 11f, FontStyle.Regular);

            for (int i = 0; i < p_objArr.Length; i++)
            {
                fltBseY = p_objArr[i].m_fltY;
                float fltColumn1 = p_objArr[i].m_fltX;
                float fltColumn2 = fltColumn1 + fltColumnArr[0];
                float fltColumn3 = fltColumn1 + fltColumnArr[1];
                float fltColumn4 = fltColumn1 + fltColumnArr[2];


                //打印标题
                m_printMethodTool.m_mthDrawString(p_objArr[i].m_strPrintTitle, m_fntSmallBold, fltColumn1, fltBseY);
                if (blnHasTwoPart)
                {
                    m_strResult = "结果";
                    m_printMethodTool.m_mthDrawString(m_strResult, m_fntSmallBold, fltColumn2 + 6, fltBseY);
                }
                else
                {
                    m_strResult = "结     果";
                    m_printMethodTool.m_mthDrawString(m_strResult, m_fntSmallBold, fltColumn2 + 60, fltBseY);
                }
                m_printMethodTool.m_mthDrawString(m_strResultUnit, m_fntSmallBold, fltColumn3, fltBseY);
                m_printMethodTool.m_mthDrawString(m_strReference, m_fntSmallBold, fltColumn4, fltBseY);

                fltBseY += fltTitleHeight;
                float fltStartPosTemp;
                for (int j = 0; j < p_objArr[i].m_intCount; j++)
                {
                    if ((p_objArr[i].m_intStartIdx + j) < p_objArr[i].m_dtvResult.Count)
                    {
                        string strResult = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["result_vchr"].ToString().Trim();
                        string strAbnormal = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["abnormal_flag_chr"].ToString().Trim();
                        string strUnit = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["unit_vchr"].ToString().Trim();
                        string strRefRange = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["refrange_vchr"].ToString().Trim();
                        string strCheckItemName = p_objArr[i].m_dtvResult[p_objArr[i].m_intStartIdx + j]["rptno_chr"].ToString().Trim();

                        // 打印项目 如果项目超出打印范围：1、截断；2、缩小字体。这里采用第二种
                        //for (int i2 = strCheckItemName.Length; i2 >= 0; i2--)
                        //{
                        //    strCheckItemName = strCheckItemName.Substring(0, i2);
                        //    fltItemNameWidth = m_printMethodTool.m_fltGetStringWidth(strCheckItemName, m_fntItemName);
                        //    if (fltItemNameWidth + m_fltTitleSpace > fltColumnArr[0])
                        //    {
                        //        continue;
                        //    }
                        //    else
                        //    {
                        //        break;
                        //    }
                        //}
                        //m_printMethodTool.m_mthDrawString(strCheckItemName, m_fntItemName, fltColumn1, fltBseY);

                        int ifntSize = Convert.ToInt32(m_fntItemName.Size);
                        Font m_fntItemNameTemp = m_fntItemName;
                        for (int iSize = ifntSize; iSize > 0; iSize--)
                        {
                            m_fntItemNameTemp = new Font(m_fntItemName.Name, iSize, FontStyle.Regular);
                            fltItemNameWidth = m_printMethodTool.m_fltGetStringWidth(strCheckItemName, m_fntItemNameTemp);
                            if (fltItemNameWidth + m_fltTitleSpace > fltColumnArr[0])
                            {
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        m_printMethodTool.m_mthDrawString(strCheckItemName, m_fntItemNameTemp, fltColumn1, fltBseY);


                        //异常标志
                        if (strAbnormal != null)
                        {
                            System.Drawing.Font objBoldFont = new Font("SimSun", 11f, FontStyle.Bold);
                            string strPR;

                            strPR = strResult + " " + "↑";
                            float fltResultWidth = m_printMethodTool.m_fltGetStringWidth(strPR, objBoldFont);

                            #region 检验结果的x坐标值   -baojian.mo 2007.09.04 Modify

                            if (fltResultPrintWidth - fltResultWidth > 0)
                            {
                                fltStartPosTemp = fltColumn2 + fltResultPrintWidth - fltResultWidth;
                            }
                            else   //实际结果宽度比预设宽度大的情况
                            {
                                fltStartPosTemp = fltColumn2;
                            }
                            #endregion

                            if (strAbnormal == "H")
                            {
                                strPR = strResult + " " + "↑";
                                float fltStartPos = fltStartPosTemp;
                                m_printMethodTool.m_mthDrawString(strPR, objBoldFont, fltStartPos, fltBseY);
                            }
                            else if (strAbnormal == "L")
                            {
                                if (strResult.Contains(">") || strResult.Contains("<"))
                                    strPR = strResult + " " + "↑";
                                else
                                    strPR = strResult + " " + "↓";
                                float fltStartPos = fltStartPosTemp;
                                m_printMethodTool.m_mthDrawString(strPR, objBoldFont, fltStartPos, fltBseY);
                            }
                            else
                            {
                                strPR = strResult + " " + " ";
                                float fltStartPos = fltStartPosTemp;
                                m_printMethodTool.m_mthDrawString(strPR, m_fntResultNotBold, fltStartPos, fltBseY);
                            }
                        }
                        if (!string.IsNullOrEmpty(strUnit))
                            m_printMethodTool.m_mthDrawString(strUnit, m_fntSmall2NotBold, fltColumn3, fltBseY);
                        if (!string.IsNullOrEmpty(strRefRange))
                            m_printMethodTool.m_mthDrawString(strRefRange, m_fntSmall2NotBold, fltColumn4, fltBseY);


                        //Locate Y 
                        fltBseY += m_fntSmall2NotBold.Height + m_fltItemSpace;
                        if (fltY < fltBseY)
                        {
                            fltY = fltBseY;
                        }
                    }
                }
            }
            return fltY;
        }
        private float m_fltPrintImageArr(clsPrintImage[] p_objArr)
        {
            //float num = 0f;
            //float result;
            //if (p_objArr == null)
            //{
            //    result = -1f;
            //}
            //else
            //{
            //    for (int i = 0; i < p_objArr.Length; i++)
            //    {
            //        this.m_printMethodTool.m_printEventArg.Graphics.DrawImage(p_objArr[i].m_img, p_objArr[i].m_fltX, p_objArr[i].m_fltY, p_objArr[i].m_fltWidth, p_objArr[i].m_fltHeight);
            //        if (num < p_objArr[i].m_fltY + p_objArr[i].m_fltHeight)
            //        {
            //            num = p_objArr[i].m_fltY + p_objArr[i].m_fltHeight;
            //        }
            //    }
            //    result = num;
            //}
            //return result;
            float fltY = 0;
            if (p_objArr == null)
                return -1;
            for (int i = 0; i < p_objArr.Length; i++)
            {
                m_printMethodTool.m_printEventArg.Graphics.DrawImage(p_objArr[i].m_img, p_objArr[i].m_fltX,
                    p_objArr[i].m_fltY, p_objArr[i].m_fltWidth, p_objArr[i].m_fltHeight);
                if (fltY < p_objArr[i].m_fltY + p_objArr[i].m_fltHeight)
                {
                    fltY = p_objArr[i].m_fltY + p_objArr[i].m_fltHeight;
                }
            }
            return fltY;
        }
        private SizeF m_rectGetPrintStringRectangle(Font p_fntTitle, Font p_fntContent, string p_strContent, float p_fltWidth, float p_fltTitleSpace, float p_fltItemSpace)
        {
            if ((p_strContent == "" || p_strContent == null) && !m_blnSummaryEmptyVisible)
            {
                return new SizeF(0, 0);
            }
            float fltTitleHeight = p_fntTitle.Height;
            float fltContentHeight = p_fntContent.Height;
            float fltHeight = 0;
            if (p_strContent != null && p_strContent != "")
            {
                SizeF sfString = m_printMethodTool.m_printEventArg.Graphics.MeasureString(p_strContent, p_fntContent);
                //fltHeight = (sfString.Width / p_fltWidth + 1) * fltContentHeight;
                fltHeight = sfString.Height;
            }
            else
            {
                fltHeight = fltTitleHeight + p_fltTitleSpace + fltContentHeight;
            }
            SizeF sf = new SizeF(p_fltWidth, fltHeight);
            return sf;
        }
        private clsPrintPerPageInfo[] m_objConstructPrintPageInfo(DataTable p_dtbResult, float p_fltX, float p_fltY, float p_fltWidth, float p_fltHeight, float p_fltMaxHeight)
        {
            //DataView dataView = this.m_dtvFilterRows(p_dtbResult, "IS_GRAPH_RESULT_NUM = 0");
            //DataView dataView2 = this.m_dtvFilterRows(p_dtbResult, "IS_GRAPH_RESULT_NUM = 1");
            //dataView.Sort = "REPORT_PRINT_SEQ_INT ASC,GROUPID_CHR ASC,SAMPLE_PRINT_SEQ_INT ASC";
            //dataView2.Sort = "REPORT_PRINT_SEQ_INT ASC,GROUPID_CHR ASC,SAMPLE_PRINT_SEQ_INT ASC";
            //clsSampleResultInfo[] array = this.m_objConstructSampleResultArr(dataView);
            //clsPrintImage[] array2 = this.m_objConstructPrintImage(dataView2);
            //float num = 0f;
            //if (this.m_blnPrintPIc)
            //{
            //    if (array2 != null && array2.Length > 0)
            //    {
            //        num = array2[0].m_fltHeight + 5f;
            //    }
            //}
            //int num2 = 0;
            //ArrayList arrayList = new ArrayList();
            //float num3 = 0f;
            //float num4 = 0f;
            //float num5 = this.m_fltGetPrintElementHeight(this.m_fntSmallBold, this.m_fltTitleSpace);
            //float num6 = this.m_fltGetPrintElementHeight(this.m_fntSmall2NotBold, this.m_fltItemSpace);
            //int num7 = dataView.Count;
            //float num8 = 0f;
            //if ((float)num7 * num6 + (float)array.Length * num5 <= (p_fltHeight - num) * 2f)
            //{
            //    num8 = p_fltHeight - num;
            //}
            //else
            //{
            //    num8 = p_fltMaxHeight - num;
            //}
            //ArrayList arrayList2 = new ArrayList();
            //bool flag = false;
            //for (int i = 0; i < array.Length; i++)
            //{
            //    int j = array[i].m_dtvResult.Count;
            //    array[i].m_fltHeight = this.m_fltGetPrintGroupHeight(array[i], this.m_fntSmallBold, this.m_fntSmall2NotBold, this.m_fltTitleSpace, this.m_fltItemSpace);
            //    if (!flag && array[i].m_fltHeight < num8 - num3)
            //    {
            //        array[i].m_fltX = p_fltX;
            //        array[i].m_fltY = num3 + p_fltY;
            //        array[i].m_intStartIdx = 0;
            //        array[i].m_intCount = array[i].m_dtvResult.Count;
            //        array[i].m_intPageIdx = num2;
            //        num3 += array[i].m_fltHeight + this.m_fltTitleSpace;
            //        arrayList2.Add(array[i]);
            //        num7 -= array[i].m_intCount;
            //    }
            //    else
            //    {
            //        if (num3 >= num8 / 2f && num6 * (float)num7 + this.m_fltImgSpace * (float)num7 + this.m_fltTitleSpace * (float)(array.Length - i - 1) + num5 * (float)(array.Length - i - 1) < num8)
            //        {
            //            flag = true;
            //            array[i].m_fltX = p_fltX + p_fltWidth / 2f;
            //            array[i].m_fltY = num4 + p_fltY;
            //            array[i].m_intStartIdx = 0;
            //            array[i].m_intCount = array[i].m_dtvResult.Count;
            //            array[i].m_intPageIdx = num2;
            //            num4 += array[i].m_fltHeight + this.m_fltTitleSpace;
            //            arrayList2.Add(array[i]);
            //            num7 -= array[i].m_intCount;
            //        }
            //        else
            //        {
            //            while (j > 0)
            //            {
            //                if (num5 + num6 < num8 - num3)
            //                {
            //                    int num9 = 1;
            //                    while ((float)(num9 + 1) * num6 + num5 < num8 - num3)
            //                    {
            //                        if (num9 >= j)
            //                        {
            //                            break;
            //                        }
            //                        num9++;
            //                    }
            //                    clsSampleResultInfo clsSampleResultInfo = new clsSampleResultInfo(array[i].m_dtvResult);
            //                    clsSampleResultInfo.m_strPrintTitle = array[i].m_strPrintTitle;
            //                    clsSampleResultInfo.m_fltX = p_fltX;
            //                    clsSampleResultInfo.m_fltY = num3 + p_fltY;
            //                    clsSampleResultInfo.m_intStartIdx = array[i].m_dtvResult.Count - j;
            //                    clsSampleResultInfo.m_intCount = num9;
            //                    clsSampleResultInfo.m_intPageIdx = num2;
            //                    num3 += (float)num9 * num6 + num5 + this.m_fltTitleSpace;
            //                    arrayList2.Add(clsSampleResultInfo);
            //                    j -= num9;
            //                    num7 -= num9;
            //                }
            //                else
            //                {
            //                    if (num5 + num6 * (float)j < num8 - num4)
            //                    {
            //                        clsSampleResultInfo clsSampleResultInfo = new clsSampleResultInfo(array[i].m_dtvResult);
            //                        clsSampleResultInfo.m_strPrintTitle = array[i].m_strPrintTitle;
            //                        clsSampleResultInfo.m_fltX = p_fltX + p_fltWidth / 2f;
            //                        clsSampleResultInfo.m_fltY = num4 + p_fltY;
            //                        clsSampleResultInfo.m_intStartIdx = array[i].m_dtvResult.Count - j;
            //                        clsSampleResultInfo.m_intCount = j;
            //                        clsSampleResultInfo.m_intPageIdx = num2;
            //                        num4 += (float)j * num6 + num5 + this.m_fltTitleSpace;
            //                        arrayList2.Add(clsSampleResultInfo);
            //                        j -= j;
            //                        num7 -= j;
            //                    }
            //                    else
            //                    {
            //                        if (num5 + num6 < num8 - num4)
            //                        {
            //                            int num9 = 1;
            //                            while ((float)(num9 + 1) * num6 + num5 < num8 - num4)
            //                            {
            //                                num9++;
            //                            }
            //                            clsSampleResultInfo clsSampleResultInfo = new clsSampleResultInfo(array[i].m_dtvResult);
            //                            clsSampleResultInfo.m_strPrintTitle = array[i].m_strPrintTitle;
            //                            clsSampleResultInfo.m_fltX = p_fltX + p_fltWidth / 2f;
            //                            clsSampleResultInfo.m_fltY = num4 + p_fltY;
            //                            clsSampleResultInfo.m_intStartIdx = array[i].m_dtvResult.Count - j;
            //                            clsSampleResultInfo.m_intCount = num9;
            //                            clsSampleResultInfo.m_intPageIdx = num2;
            //                            num4 += (float)num9 * num6 + num5 + this.m_fltTitleSpace;
            //                            arrayList2.Add(clsSampleResultInfo);
            //                            j -= num9;
            //                            num7 -= num9;
            //                        }
            //                        else
            //                        {
            //                            num3 = 0f;
            //                            num4 = 0f;
            //                            flag = false;
            //                            num2++;
            //                            arrayList.Add(arrayList2);
            //                            arrayList2 = new ArrayList();
            //                            if ((float)num7 * num6 + (float)array.Length * num5 <= p_fltHeight * 2f)
            //                            {
            //                                num8 = p_fltHeight;
            //                            }
            //                            else
            //                            {
            //                                num8 = p_fltMaxHeight;
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //if (arrayList2.Count > 0)
            //{
            //    arrayList.Add(arrayList2);
            //}
            //float num10 = Math.Max(num3, num4);
            //ArrayList arrayList3 = null;
            //ArrayList arrayList4 = null;
            //if (this.m_blnPrintPIc)
            //{
            //    if (array2 != null && array2.Length > 0)
            //    {
            //        arrayList3 = new ArrayList();
            //        arrayList4 = new ArrayList();
            //        float num11 = 0f;
            //        for (int i = 0; i < array2.Length; i++)
            //        {
            //            if (array2[i].m_fltHeight >= p_fltMaxHeight || array2[i].m_fltWidth >= p_fltWidth)
            //            {
            //                break;
            //            }
            //            bool flag2 = false;
            //            while (!flag2)
            //            {
            //                if (p_fltMaxHeight - num10 > array2[i].m_fltHeight)
            //                {
            //                    if (p_fltWidth - num11 > array2[i].m_fltWidth)
            //                    {
            //                        array2[i].m_fltX = num11 + p_fltX;
            //                        array2[i].m_fltY = num10 + p_fltY;
            //                        array2[i].m_intPageIdx = num2;
            //                        arrayList4.Add(array2[i]);
            //                        num11 += array2[i].m_fltWidth + this.m_fltImgSpace + 20f;
            //                        flag2 = true;
            //                    }
            //                    else
            //                    {
            //                        if (i > 0)
            //                        {
            //                            num10 += array2[i].m_fltHeight + this.m_fltImgSpace;
            //                            num11 = 0f;
            //                        }
            //                    }
            //                }
            //                else
            //                {
            //                    num11 = 0f;
            //                    num10 = 0f;
            //                    if (arrayList4.Count > 0)
            //                    {
            //                        arrayList3.Add(arrayList4);
            //                        arrayList4 = new ArrayList();
            //                    }
            //                    num2++;
            //                }
            //            }
            //        }
            //        if (arrayList4.Count > 0)
            //        {
            //            arrayList3.Add(arrayList4);
            //        }
            //    }
            //}
            //string p_strContent = this.m_dtbSample.Rows[0]["summary_vchr"].ToString().Trim();
            //SizeF sizeF = this.m_rectGetPrintStringRectangle(this.m_fntSmallBold, this.m_fntSmallNotBold, p_strContent, this.m_fltPrintWidth, this.m_fltTitleSpace, this.m_fltItemSpace);
            //if (sizeF.Height > 0f && sizeF.Height > p_fltMaxHeight - num10)
            //{
            //    num2++;
            //}
            //clsPrintPerPageInfo[] array3 = new clsPrintPerPageInfo[num2 + 1];
            //int num12 = -1;
            //if (arrayList3 != null)
            //{
            //    num12 = ((clsPrintImage[])((ArrayList)arrayList3[0]).ToArray(typeof(clsPrintImage)))[0].m_intPageIdx;
            //}
            //for (int i = 0; i < array3.Length; i++)
            //{
            //    array3[i] = new clsPrintPerPageInfo();
            //    if (i <= arrayList.Count - 1)
            //    {
            //        array3[i].m_objSampleArr = (clsSampleResultInfo[])((ArrayList)arrayList[i]).ToArray(typeof(clsSampleResultInfo));
            //    }
            //    if (arrayList3 != null)
            //    {
            //        if (num12 <= i && i <= num12 + arrayList3.Count - 1)
            //        {
            //            array3[i].m_imgArr = (clsPrintImage[])((ArrayList)arrayList3[i - num12]).ToArray(typeof(clsPrintImage));
            //        }
            //    }
            //}
            //return array3;
            //过滤出结果数据和图形数据
            DataView dtvData = m_dtvFilterRows(p_dtbResult, "IS_GRAPH_RESULT_NUM = 0");
            DataView dtvImage = m_dtvFilterRows(p_dtbResult, "IS_GRAPH_RESULT_NUM = 1");

            //排序
            dtvData.Sort = "REPORT_PRINT_SEQ_INT ASC,GROUPID_CHR ASC,SAMPLE_PRINT_SEQ_INT ASC";
            dtvImage.Sort = "REPORT_PRINT_SEQ_INT ASC,GROUPID_CHR ASC,SAMPLE_PRINT_SEQ_INT ASC";

            // 
            clsSampleResultInfo[] objDataArr = m_objConstructSampleResultArr(dtvData);

            clsPrintImage[] objImgArr = m_objConstructPrintImage(dtvImage);

            #region xing.chen add 2005.9.22

            float fltImgHeight = 0;
            if (m_blnPrintPIc)
            {
                if (objImgArr != null && objImgArr.Length > 0)
                {
                    fltImgHeight = objImgArr[0].m_fltHeight + 5;      //baojian.mo -2007.9.3 modify
                }
            }
            #endregion

            int intPage = 0;

            //打印与分页
            ArrayList arlPageData = new ArrayList();

            #region 结果数据打印分页
            float fltLeft = 0;
            float fltRight = 0;
            float fltTitleHeight = m_fltGetPrintElementHeight(m_fntSmallBold, m_fltTitleSpace);
            float fltItemHeight = m_fltGetPrintElementHeight(m_fntSmall2NotBold, m_fltItemSpace);
            //记录分页剩余的记录个数
            int intTotalLeftItemCount = dtvData.Count;
            float fltHeight = 0;
            if (intTotalLeftItemCount * fltItemHeight + objDataArr.Length * fltTitleHeight <= (p_fltHeight - fltImgHeight) * 2)	//xing.chen modify
            {
                fltHeight = p_fltHeight - fltImgHeight;	//xing.chen modify
            }
            else
            {
                fltHeight = p_fltMaxHeight - fltImgHeight;	//xing.chen modify
            }

            ArrayList arlPrintData = new ArrayList();
            //指示当前是否在右边打印
            bool blnPrintRight = false;
            for (int i = 0; i < objDataArr.Length; i++)
            {
                int intDataCount = objDataArr[i].m_dtvResult.Count;
                objDataArr[i].m_fltHeight = m_fltGetPrintGroupHeight(objDataArr[i], m_fntSmallBold, m_fntSmall2NotBold, m_fltTitleSpace, m_fltItemSpace);
                //左边打印
                if (!blnPrintRight && objDataArr[i].m_fltHeight < fltHeight - fltLeft)
                {
                    objDataArr[i].m_fltX = p_fltX;
                    objDataArr[i].m_fltY = fltLeft + p_fltY;
                    objDataArr[i].m_intStartIdx = 0;
                    objDataArr[i].m_intCount = objDataArr[i].m_dtvResult.Count;
                    objDataArr[i].m_intPageIdx = intPage;
                    fltLeft += objDataArr[i].m_fltHeight + m_fltTitleSpace;
                    arlPrintData.Add(objDataArr[i]);
                    intTotalLeftItemCount -= objDataArr[i].m_intCount;
                }
                else
                {
                    //判断余下的记录能否在另一边打完,并且当前已经打印的记录个数必须大于或等于单列打印个数的1/2
                    if (fltLeft >= fltHeight / 2 && (fltItemHeight * intTotalLeftItemCount + m_fltImgSpace * intTotalLeftItemCount + m_fltTitleSpace * (objDataArr.Length - i - 1) + fltTitleHeight * (objDataArr.Length - i - 1)) < fltHeight)
                    {
                        blnPrintRight = true;
                        objDataArr[i].m_fltX = p_fltX + p_fltWidth / 2;
                        objDataArr[i].m_fltY = fltRight + p_fltY;
                        objDataArr[i].m_intStartIdx = 0;
                        objDataArr[i].m_intCount = objDataArr[i].m_dtvResult.Count;
                        objDataArr[i].m_intPageIdx = intPage;
                        fltRight += objDataArr[i].m_fltHeight + m_fltTitleSpace;
                        arlPrintData.Add(objDataArr[i]);
                        intTotalLeftItemCount -= objDataArr[i].m_intCount;
                    }
                    else
                    {
                        while (intDataCount > 0)
                        {
                            if (fltTitleHeight + fltItemHeight < fltHeight - fltLeft)
                            {
                                int intPrintItemCount = 1;

                                while ((intPrintItemCount + 1) * fltItemHeight + fltTitleHeight < fltHeight - fltLeft)
                                {
                                    if (intPrintItemCount >= intDataCount)
                                    {
                                        break;
                                    }
                                    intPrintItemCount++;

                                }
                                clsSampleResultInfo obj = new clsSampleResultInfo(objDataArr[i].m_dtvResult);
                                obj.m_strPrintTitle = objDataArr[i].m_strPrintTitle;
                                obj.m_fltX = p_fltX;
                                obj.m_fltY = fltLeft + p_fltY;
                                obj.m_intStartIdx = objDataArr[i].m_dtvResult.Count - intDataCount;
                                obj.m_intCount = intPrintItemCount;
                                obj.m_intPageIdx = intPage;
                                fltLeft += intPrintItemCount * fltItemHeight + fltTitleHeight + m_fltTitleSpace;

                                arlPrintData.Add(obj);
                                intDataCount -= intPrintItemCount;
                                intTotalLeftItemCount -= intPrintItemCount;
                            }
                            else
                            {
                                //右边打印
                                if (fltTitleHeight + fltItemHeight * intDataCount < fltHeight - fltRight)
                                {
                                    clsSampleResultInfo obj = new clsSampleResultInfo(objDataArr[i].m_dtvResult);
                                    obj.m_strPrintTitle = objDataArr[i].m_strPrintTitle;
                                    obj.m_fltX = p_fltX + p_fltWidth / 2;
                                    obj.m_fltY = fltRight + p_fltY;
                                    obj.m_intStartIdx = objDataArr[i].m_dtvResult.Count - intDataCount;
                                    obj.m_intCount = intDataCount;
                                    obj.m_intPageIdx = intPage;
                                    fltRight += intDataCount * fltItemHeight + fltTitleHeight + m_fltTitleSpace;
                                    arlPrintData.Add(obj);
                                    intDataCount -= intDataCount;
                                    intTotalLeftItemCount -= intDataCount;
                                }
                                else
                                {
                                    if (fltTitleHeight + fltItemHeight < fltHeight - fltRight)
                                    {
                                        int intPrintItemCount = 1;
                                        while ((intPrintItemCount + 1) * fltItemHeight + fltTitleHeight < fltHeight - fltRight)
                                        {
                                            intPrintItemCount++;
                                        }
                                        clsSampleResultInfo obj = new clsSampleResultInfo(objDataArr[i].m_dtvResult);
                                        obj.m_strPrintTitle = objDataArr[i].m_strPrintTitle;
                                        obj.m_fltX = p_fltX + p_fltWidth / 2;
                                        obj.m_fltY = fltRight + p_fltY;
                                        obj.m_intStartIdx = objDataArr[i].m_dtvResult.Count - intDataCount;
                                        obj.m_intCount = intPrintItemCount;
                                        obj.m_intPageIdx = intPage;
                                        fltRight += intPrintItemCount * fltItemHeight + fltTitleHeight + m_fltTitleSpace;
                                        arlPrintData.Add(obj);
                                        intDataCount -= intPrintItemCount;
                                        intTotalLeftItemCount -= intPrintItemCount;
                                    }
                                    else
                                    {
                                        fltLeft = 0;
                                        fltRight = 0;
                                        blnPrintRight = false;
                                        intPage++;
                                        arlPageData.Add(arlPrintData);
                                        arlPrintData = new ArrayList();
                                        if (intTotalLeftItemCount * fltItemHeight + objDataArr.Length * fltTitleHeight <= p_fltHeight * 2)
                                        {
                                            fltHeight = p_fltHeight;
                                        }
                                        else
                                        {
                                            fltHeight = p_fltMaxHeight;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (arlPrintData.Count > 0)
            {
                arlPageData.Add(arlPrintData);
            }
            #endregion

            float fltY = Math.Max(fltLeft, fltRight);
            //			fltY += 4*m_fltTitleSpace;
            int intImgStartIdx = intPage;
            ArrayList arlPageImg = null;
            ArrayList arlImg = null;

            if (m_blnPrintPIc)
            {
                #region 图形数据打印分页
                if (objImgArr != null && objImgArr.Length > 0)
                {
                    arlPageImg = new ArrayList();
                    arlImg = new ArrayList();
                    float fltX = 0;
                    for (int i = 0; i < objImgArr.Length; i++)
                    {
                        if (objImgArr[i].m_fltHeight < p_fltMaxHeight && objImgArr[i].m_fltWidth < p_fltWidth)
                        {
                            bool blnDrawed = false;
                            while (!blnDrawed)
                            {
                                if (p_fltMaxHeight - fltY > objImgArr[i].m_fltHeight)
                                {
                                    if (p_fltWidth - fltX > objImgArr[i].m_fltWidth)
                                    {
                                        objImgArr[i].m_fltX = fltX + p_fltX;
                                        //objImgArr[i].m_fltX = (fltX == 0 ? fltX + p_fltX : fltX + p_fltX + m_fltImgSpace);
                                        objImgArr[i].m_fltY = fltY + p_fltY;
                                        objImgArr[i].m_intPageIdx = intPage;
                                        arlImg.Add(objImgArr[i]);
                                        fltX += objImgArr[i].m_fltWidth + m_fltImgSpace + 20;
                                        blnDrawed = true;
                                    }
                                    else
                                    {
                                        if (i > 0)
                                        {
                                            fltY += objImgArr[i].m_fltHeight + m_fltImgSpace;
                                            fltX = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    fltX = 0;
                                    fltY = 0;
                                    if (arlImg.Count > 0)
                                    {
                                        arlPageImg.Add(arlImg);
                                        arlImg = new ArrayList();
                                    }
                                    intPage++;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (arlImg.Count > 0)
                    {
                        arlPageImg.Add(arlImg);
                    }
                }
            }
                #endregion

            //实验室提示
            string strSummary = m_dtbSample.Rows[0]["summary_vchr"].ToString().Trim();
            SizeF sf = m_rectGetPrintStringRectangle(m_fntSmallBold, m_fntSmallNotBold, strSummary, m_fltPrintWidth, m_fltTitleSpace,
                m_fltItemSpace);
            if (sf.Height > 0 && sf.Height > p_fltMaxHeight - fltY)
            {
                intPage++;
            }

            #region 构造页面打印信息
            clsPrintPerPageInfo[] objArr = new clsPrintPerPageInfo[intPage + 1];
            int intStartImgIdx = -1;
            if (arlPageImg != null)
            {
                intStartImgIdx = ((clsPrintImage[])((ArrayList)arlPageImg[0]).ToArray(typeof(clsPrintImage)))[0].m_intPageIdx;
            }
            for (int i = 0; i < objArr.Length; i++)
            {
                objArr[i] = new clsPrintPerPageInfo();
                if (i <= arlPageData.Count - 1)
                {
                    objArr[i].m_objSampleArr = (clsSampleResultInfo[])((ArrayList)arlPageData[i]).ToArray(typeof(clsSampleResultInfo));
                }
                if (arlPageImg != null)
                {
                    if (intStartImgIdx <= i && i <= intStartImgIdx + arlPageImg.Count - 1)
                    {
                        objArr[i].m_imgArr = (clsPrintImage[])((ArrayList)arlPageImg[i - intStartImgIdx]).ToArray(typeof(clsPrintImage));
                    }
                }
            }
            #endregion

            return objArr;
        }
        private float m_fltGetPrintGroupHeight(clsSampleResultInfo p_objData, Font p_fntTitle, Font p_fntItem, float p_fltTitleSpace, float p_fltItemSpace)
        {
            //float num = 0f;
            //return num + ((float)p_fntTitle.Height + p_fltTitleSpace + (float)p_objData.m_intCount * ((float)p_fntItem.Height + p_fltItemSpace));
            float fltHeight = 0;
            fltHeight += (p_fntTitle.Height + p_fltTitleSpace) + (p_objData.m_intCount * (p_fntItem.Height + p_fltItemSpace));
            return fltHeight;
        }
        
        private float m_fltGetPrintElementHeight(Font p_fnt, float p_fltPrintSpace)
        {
            //float num = 0f;
            //return num + ((float)p_fnt.Height + p_fltPrintSpace);
            float fltHeight = 0;
            fltHeight += p_fnt.Height + p_fltPrintSpace;
            return fltHeight;
        }
        private clsSampleResultInfo[] m_objConstructSampleResultArr(DataView p_dtvData)
        {
            //ArrayList arrayList = new ArrayList();
            //clsSampleResultInfo[] array = null;
            //for (int i = 0; i < p_dtvData.Count; i++)
            //{
            //    if (i > 0)
            //    {
            //        if (p_dtvData[i]["groupid_chr"].ToString().Trim() != p_dtvData[i - 1]["groupid_chr"].ToString().Trim())
            //        {
            //            arrayList.Add(p_dtvData[i]["groupid_chr"].ToString().Trim());
            //        }
            //    }
            //    else
            //    {
            //        arrayList.Add(p_dtvData[i]["groupid_chr"].ToString().Trim());
            //    }
            //}
            //if (arrayList.Count > 0)
            //{
            //    array = new clsSampleResultInfo[arrayList.Count];
            //    for (int i = 0; i < arrayList.Count; i++)
            //    {
            //        DataView dataView = new DataView(p_dtvData.Table);
            //        dataView.RowFilter = "IS_GRAPH_RESULT_NUM = 0 AND groupid_chr = " + arrayList[i].ToString().Trim();
            //        array[i] = new clsSampleResultInfo(dataView);
            //        array[i].m_dtvResult.Sort = "SAMPLE_PRINT_SEQ_INT ASC";
            //        array[i].m_strPrintTitle = dataView[0]["print_title_vchr"].ToString().Trim();
            //        array[i].m_fltHeight = this.m_fltGetPrintGroupHeight(array[i], this.m_fntSmallBold, this.m_fntSmall2NotBold, this.m_fltTitleSpace, this.m_fltItemSpace);
            //        array[i].m_intCount = array[i].m_dtvResult.Count;
            //    }
            //}
            //clsSampleResultInfo[] result;
            //if (array == null)
            //{
            //    result = new clsSampleResultInfo[0];
            //}
            //else
            //{
            //    result = array;
            //}
            //return result;
            ArrayList arlGroupID = new ArrayList();
            clsSampleResultInfo[] objArr = null;
            for (int i = 0; i < p_dtvData.Count; i++)
            {
                if (i > 0)
                {
                    if (p_dtvData[i]["groupid_chr"].ToString().Trim() != p_dtvData[i - 1]["groupid_chr"].ToString().Trim())
                    {
                        arlGroupID.Add(p_dtvData[i]["groupid_chr"].ToString().Trim());
                    }
                }
                else
                {
                    arlGroupID.Add(p_dtvData[i]["groupid_chr"].ToString().Trim());
                }
            }
            if (arlGroupID.Count > 0)
            {
                objArr = new clsSampleResultInfo[arlGroupID.Count];
                for (int i = 0; i < arlGroupID.Count; i++)
                {
                    DataView dtv = new DataView(p_dtvData.Table);
                    dtv.RowFilter = "IS_GRAPH_RESULT_NUM = 0 AND groupid_chr = " + arlGroupID[i].ToString().Trim();
                    objArr[i] = new clsSampleResultInfo(dtv);
                    objArr[i].m_dtvResult.Sort = "SAMPLE_PRINT_SEQ_INT ASC";
                    objArr[i].m_strPrintTitle = dtv[0]["print_title_vchr"].ToString().Trim();
                    //if (dtv[0]["print_title_vchr"].ToString().Trim() != "")
                    //    objArr[i].m_strPrintTitle = dtv[0]["print_title_vchr"].ToString().Trim();
                    //else if (dtv[0]["groupid_chr"].ToString().Trim() == "000360")
                    //    objArr[i].m_strPrintTitle = "网织红";
                    //else
                    //    objArr[i].m_strPrintTitle = "血常规34项";
                    objArr[i].m_fltHeight = m_fltGetPrintGroupHeight(objArr[i], m_fntSmallBold, m_fntSmall2NotBold, m_fltTitleSpace, m_fltItemSpace);
                    objArr[i].m_intCount = objArr[i].m_dtvResult.Count;
                }
            }

            if (objArr == null)
            {
                return new clsSampleResultInfo[0];
            }
            return objArr;
        }
        private clsPrintImage[] m_objConstructPrintImage(DataView p_dtvData)
        {
            //int count = p_dtvData.Count;
            //ArrayList arrayList = new ArrayList();
            //for (int i = 0; i < count; i++)
            //{
            //    if (!(p_dtvData[i]["GRAPH_IMG"] is DBNull))
            //    {
            //        Image image = this.m_imgDrawGraphic((byte[])p_dtvData[i]["GRAPH_IMG"], p_dtvData[i]["GRAPH_FORMAT_NAME_VCHR"].ToString());
            //        if (image != null)
            //        {
            //            clsPrintImage clsPrintImage = new clsPrintImage(image);
            //            clsPrintImage.m_fltWidth = this.m_fltXRate * clsPrintImage.m_fltWidth;
            //            clsPrintImage.m_fltHeight = this.m_fltYRate * clsPrintImage.m_fltHeight;
            //            arrayList.Add(clsPrintImage);
            //        }
            //    }
            //}
            //return (clsPrintImage[])arrayList.ToArray(typeof(clsPrintImage));
            int intCount = p_dtvData.Count;
            clsPrintImage[] objImgArr = null;
            ArrayList arl = new ArrayList();
            for (int i = 0; i < intCount; i++)
            {
                if (p_dtvData[i]["GRAPH_IMG"] is System.DBNull)
                {
                    continue;
                }
                Image img = m_imgDrawGraphic((byte[])p_dtvData[i]["GRAPH_IMG"], p_dtvData[i]["GRAPH_FORMAT_NAME_VCHR"].ToString());
                if (img != null)
                {
                    clsPrintImage objImg = new clsPrintImage(img);
                    objImg.m_fltWidth = m_fltXRate * objImg.m_fltWidth;
                    objImg.m_fltHeight = m_fltYRate * objImg.m_fltHeight;
                    arl.Add(objImg);
                }
            }
            objImgArr = (clsPrintImage[])arl.ToArray(typeof(clsPrintImage));
            return objImgArr;
        }
        private DataView m_dtvFilterRows(DataTable p_dtbSource, string p_strFltExp)
        {
            //return new DataView(p_dtbSource)
            //{
            //    RowFilter = p_strFltExp
            //};
            DataView dtv = new DataView(p_dtbSource);
            dtv.RowFilter = p_strFltExp;
            return dtv;
        }
        private void m_mthPrint()
        {
            //this.m_mthPrintBseInfo();
            //lisprintBiz biz = new lisprintBiz();
            //string text = biz.m_strGetSysparm("7006");

            //if (text != null)
            //{
            //    if (text == "")
            //    {
            //        this.m_strNotice = "祝您身体健康!此报告仅对检测标本负责,结果供医生参考!";
            //    }
            //    if (text == "0")
            //    {
            //        this.m_strNotice = "祝您身体健康!此报告仅对检测标本负责,结果供医生参考!";
            //    }
            //    if (text == "1")
            //    {
            //        this.m_strNotice = string.Empty;
            //    }
            //}
            //else 
            //    this.m_strNotice = biz.m_strGetSysparm("7006");

            //if (this.BillStyle == 0)
            //{
            //    this.m_mthPrintEnd();
            //    this.m_mthPrintDetail();
            //}
            //else
            //{
            //    this.m_mthPrintEnd_DGCS();
            //    this.m_mthPrintDetail_DGCS();
            //}
            //if (this.m_intTotalPage - 1 > this.m_intCurrentPageIdx)
            //    this.m_intCurrentPageIdx++;
            m_mthPrintBseInfo();

            #region 自定义报告说明 -加入7006参数 mobaojian 2007.09.04
            switch (com.digitalwave.iCare.gui.HIS.clsPublic.m_strGetSysparm("7006"))
            {
                case "":
                    m_strNotice = "祝您身体健康!此报告仅对检测标本负责,结果供医生参考!";
                    break;
                case "0":
                    m_strNotice = "祝您身体健康!此报告仅对检测标本负责,结果供医生参考!";
                    break;
                case "1":
                    m_strNotice = string.Empty;
                    break;
                default:
                    m_strNotice = com.digitalwave.iCare.gui.HIS.clsPublic.m_strGetSysparm("7006");
                    break;
            }

            #endregion
            //0 项目加多图片缩小样式 1 字体加大图片缩小样式(茶山样式)
            if (BillStyle == 0)
            {
                m_mthPrintEnd();
                m_mthPrintDetail();

            }
            else
            {
                m_mthPrintEnd_DGCS();
                m_mthPrintDetail_DGCS();

            }
            if (m_intTotalPage - 1 > m_intCurrentPageIdx)
            {
                m_intCurrentPageIdx++;
            }
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
