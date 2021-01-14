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
        private string m_cardType = "证件类型:";
        private string m_cardNo = "证件号码:";
        private bool isCov2019 = false;
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
        public int m_intTotalPage = 0;
        private bool m_blnSummaryEmptyVisible = false;
        private bool m_blnAnnotationEmptyVisible = false;
        private int BillStyle = 0;
        public static bool blnSurePrintDiagnose = false;
        private Image objImage;
        public bool IsDocked { get; set; }

        private List<string> lstAppUnitID{ get; set; }
        private EntityAppUnit CurrAppUnit{ get; set; }
        List<EntityAidRemark> lstAidRemark { get; set; }
        List<string> lstCov2019 { get; set; }
        /// <summary>
        /// Mejer 尿沉渣带图片报告格式
        /// </summary>
        string mejerParm { get; set; }


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
        //private string GetAllergenRemarkInfo(string appId)
        //{
        //    string result;
        //    try
        //    {
        //        if (this.lstAppUnitID == null || this.lstAppUnitID.Count == 0)
        //        {
        //            this.CurrAppUnit = null;
        //            result = "";
        //        }
        //        else
        //        {
        //            if (this.CurrAppUnit != null && this.CurrAppUnit.appId == appId)
        //            {
        //                result = this.CurrAppUnit.remarkInfo;
        //            }
        //            else
        //            {
        //                this.CurrAppUnit = null;
        //                lisprintBiz biz = new lisprintBiz();
        //                List<string> appUnitIdByAppId = biz.GetAppUnitIdByAppId(appId);
        //                if (appUnitIdByAppId != null && appUnitIdByAppId.Count > 0)
        //                {
        //                    foreach (string current in appUnitIdByAppId)
        //                    {
        //                        if (this.lstAppUnitID.IndexOf(current) >= 0)
        //                        {
        //                            this.CurrAppUnit = new EntityAppUnit();
        //                            this.CurrAppUnit.appId = appId;
        //                            string text = string.Empty + Environment.NewLine;
        //                            text = text + "0:无[0.00-0.34 IU/ml]\t\t\t\t1:低[0.35-0.69 IU/ml]\t\t2:增加[0.70-3.49 IU/ml]" + Environment.NewLine;
        //                            text = text + "3:显著增加[3.50-17.49 IU/ml]\t\t4:高[17.5-49.9 IU/ml]\t\t5:较高[50.0-100.0 IU/ml]" + Environment.NewLine;
        //                            text += "6:极高[>100 IU/ml]";
        //                            this.CurrAppUnit.remarkInfo = text;
        //                            result = this.CurrAppUnit.remarkInfo;
        //                            return result;
        //                        }
        //                    }
        //                }
        //                result = "";
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        result = "";
        //    }
        //    return result;
        //}

        #region 过敏源分级备注信息

        /// <summary>
        /// 过敏源分级备注信息
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        string GetAllergenRemarkInfo(string appId, string contrastStr, string contrastSex)
        {
            try
            {
                if (this.lstAidRemark == null || this.lstAidRemark.Count == 0)
                {
                    this.CurrAppUnit = null;
                    return "";
                }

                if (this.CurrAppUnit != null && this.CurrAppUnit.appId == appId)
                {
                    return this.CurrAppUnit.remarkInfo;
                }

                this.CurrAppUnit = null;
                lisprintBiz biz = new lisprintBiz();
                List<string> lstTempId = biz.GetAppUnitIdByAppId(appId);

                if (lstTempId != null && lstTempId.Count > 0)
                {
                    string remarkInfoStr = string.Empty;
                    foreach (string id in lstTempId)
                    {
                        // 2020-07-15
                        if (lstAidRemark.Any(p => p.appUnitId.IndexOf(id) >= 0))
                        {
                            List<EntityAidRemark> lstAidRemarkVO = lstAidRemark.FindAll(p => p.appUnitId.IndexOf(id) >= 0);
                            foreach(EntityAidRemark aidRemarkVO in lstAidRemarkVO)
                            {
                                // 校验
                                // 1. 是否已人工添加
                                if (!string.IsNullOrEmpty(aidRemarkVO.keyWord))
                                {
                                    if (contrastStr.IndexOf(aidRemarkVO.keyWord) >= 0) continue;    // 已存在
                                }
                                // 2. 男/女                            
                                if (aidRemarkVO.sex == 1)  // 限男
                                {
                                    if (contrastSex == "女") continue;
                                }
                                else if (aidRemarkVO.sex == 2)  // 限女
                                {
                                    if (contrastSex == "男") continue;
                                }
                                // 3. 偏高(1) / 偏低(2)
                                if (aidRemarkVO.highOrLow == 1 || aidRemarkVO.highOrLow == 2 || aidRemarkVO.highOrLow == 3)
                                {
                                    bool isPass = false;
                                    List<clsDeviceReslutVO> lstResult = null;
                                    if (m_dtbResult != null)
                                    {
                                        clsDeviceReslutVO vo = null;
                                        lstResult = new List<clsDeviceReslutVO>();
                                        foreach (DataRow dr in m_dtbResult.Rows)
                                        {
                                            vo = new clsDeviceReslutVO();
                                            vo.m_strAbnormalFlag = dr["abnormal_flag_chr"].ToString();
                                            vo.m_strDeviceCheckItemName = dr["device_check_item_name_vchr"].ToString();
                                            vo.m_strResult = dr["result_vchr"].ToString();
                                            vo.m_strDeviceSampleID = dr["check_item_id_chr"].ToString();
                                            lstResult.Add(vo);

                                        }
                                    }
                                    if (lstResult != null)
                                    {
                                        foreach (clsDeviceReslutVO item in lstResult)
                                        {
                                            if (aidRemarkVO.highOrLow == 1)
                                            {
                                                if (!string.IsNullOrEmpty(aidRemarkVO.checkItemId))
                                                {
                                                    clsDeviceReslutVO vo = lstResult.Find(r => r.m_strDeviceSampleID == aidRemarkVO.checkItemId);
                                                    if (vo != null)
                                                    {
                                                        if (vo.m_strAbnormalFlag == "H")
                                                        {
                                                            isPass = true;
                                                            break;
                                                        }
                                                    }
                                                    break;
                                                }

                                                if (item.m_strAbnormalFlag == "H")
                                                {
                                                    isPass = true;
                                                    break;
                                                }
                                            }
                                            else if (aidRemarkVO.highOrLow == 2)
                                            {
                                                if (!string.IsNullOrEmpty(aidRemarkVO.checkItemId))
                                                {
                                                    clsDeviceReslutVO vo = lstResult.Find(r => r.m_strDeviceSampleID == aidRemarkVO.checkItemId);
                                                    if (vo != null)
                                                    {
                                                        if (vo.m_strAbnormalFlag == "L")
                                                        {
                                                            isPass = true;
                                                            break;
                                                        }
                                                    }
                                                    break;
                                                }

                                                if (item.m_strAbnormalFlag == "L")
                                                {
                                                    isPass = true;
                                                    break;
                                                }
                                            }
                                            else if (aidRemarkVO.highOrLow == 3)
                                            {
                                                if (!string.IsNullOrEmpty(aidRemarkVO.checkItemId))
                                                {
                                                    clsDeviceReslutVO vo = lstResult.Find(r => r.m_strDeviceSampleID == aidRemarkVO.checkItemId);
                                                    if (vo != null)
                                                    {
                                                        if (vo.m_strResult.Contains("阳"))
                                                        {
                                                            isPass = true;
                                                            break;
                                                        }
                                                    }
                                                    break;
                                                }

                                                if (item.m_strResult.Contains("阳"))
                                                {
                                                    isPass = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    if (isPass == false) continue;
                                }
                                if (!remarkInfoStr.Contains("项目") && aidRemarkVO.appunitgroup == 1)
                                {
                                    remarkInfoStr += "项目    卵泡期      排卵期     黄体期     绝经期      妊娠期    未妊娠    单位" + Environment.NewLine;
                                }

                                remarkInfoStr += aidRemarkVO.remarkInfo + Environment.NewLine;
                            }  
                        }
                    }
                    if (!string.IsNullOrEmpty(remarkInfoStr))
                    {
                        this.CurrAppUnit = new EntityAppUnit()
                        {
                            appId = appId,
                            remarkInfo = remarkInfoStr
                        };

                        return this.CurrAppUnit.remarkInfo;
                    }
                }
                return "";
            }
            catch
            {
                return "";
            }
        }
        #endregion

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
        public void m_mthInitalPrintTool()
        {
            PrintDocument p_printDoc = new PrintDocument();
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
            if (m_dtbSample == null)
                return;


            float fltColumn1 = m_fltStartX;
            float fltColumn2 = m_fltPaperWidth * 0.25f;
            float fltColumn3 = m_fltPaperWidth * 0.40f;
            float fltColumn4 = m_fltPaperWidth * 0.62f;

            bool isUseA4 = com.digitalwave.iCare.gui.HIS.clsPublic.ConvertObjToDecimal(com.digitalwave.iCare.gui.HIS.clsPublic.m_strReadXML("Lis", "IsUseA4", "AnyOne")) == 1 ? true : false;
            if (isUseA4)
            {
                m_fltY = 30;
            }
            else
            {
                m_fltY = 5;
            }

            //图标
            m_printMethodTool.m_mthPrintImage(objImage, fltColumn1, m_fltY);

            //string m_strTitleImg = m_dtbSample.Rows[0]["print_title_vchr"].ToString().Trim().Remove
            //    (m_dtbSample.Rows[0]["print_title_vchr"].ToString().Trim().Length - 5);

            string m_strTitleImg = "东 莞 市 茶 山 医 院";
            string m_strTitleImgEng = "ChaShan Hospital of DongGuang";

            //医院名称
            // m_printMethodTool.m_mthDrawString(m_strTitleImg, m_fntSmallBold, fltColumn1 + objImage.Width, m_fltY + 16);

            //英文
            // m_printMethodTool.m_mthDrawString(m_strTitleImgEng, m_fntsamll3NotBold, fltColumn1 + objImage.Width, m_fltY + 30);

            m_fltY += objImage.Height - 40;

            string m_strTitle = m_dtbSample.Rows[0]["print_title_vchr"].ToString().Trim().Substring
                (m_dtbSample.Rows[0]["print_title_vchr"].ToString().Trim().Length - 5);

            if (!m_strTitle.Contains("检验报告单"))
            {
                m_strTitle = "检验报告单";
            }

            //DrawTitle
            m_printMethodTool.m_mthPrintTitle(m_strTitle, m_fntTitle, m_fltY, m_fltPaperWidth);

            //Locate Y
            m_fltY += 3 + m_printMethodTool.m_fltGetStringHeight(m_dtbSample.Rows[0]["print_title_vchr"].ToString().Trim(), m_fntTitle);
            m_printMethodTool.m_mthDrawLine(m_fltStartX - 5, m_fltY, m_fltPaperWidth * 0.9f, m_fltY);
            if (isUseA4)
            {
                //Locate Y
                m_fltY += 12;
            }
            else
            {
                //Locate Y
                m_fltY += 3;
            }

            //姓名
            m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntHeadNotBold, m_strPatientName,
                m_dtbSample.Rows[0]["patient_name_vchr"].ToString().Trim(), fltColumn1, m_fltY);


            //性别
            m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold,
                m_strSex, m_dtbSample.Rows[0]["sex_chr"].ToString().Trim(), fltColumn2, m_fltY);

            //年龄
            m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold,
                m_strAge, m_dtbSample.Rows[0]["age_chr"].ToString().Trim(), fltColumn3, m_fltY);

            //住院号、门诊卡号、体检号
            string strPatientType = m_dtbSample.Rows[0]["patient_type_chr"].ToString().Trim();
            string strPrintContent = null;
            switch (strPatientType)
            {
                case "2":
                    m_strInPatientNo = "诊疗卡号:";
                    strPrintContent = m_dtbSample.Rows[0]["patientcardid_chr"].ToString().Trim();
                    break;

                case "3":
                    m_strInPatientNo = "体检号:";
                    strPrintContent = m_dtbSample.Rows[0]["patient_inhospitalno_chr"].ToString().Trim();
                    break;

                default:
                    m_strInPatientNo = "住院号:";
                    strPrintContent = m_dtbSample.Rows[0]["patient_inhospitalno_chr"].ToString().Trim();
                    break;
            }


            m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold, m_strInPatientNo,
                strPrintContent, fltColumn4, m_fltY);

            //Locate Y
            m_fltY += 5 + m_printMethodTool.m_fltGetStringHeight(m_strSampleID, m_fntSmallBold);

            #region 新冠基本信息
            List<string> lstTempId = new lisprintBiz().GetAppUnitIdByAppId(m_dtbSample.Rows[0]["application_id_chr"].ToString().Trim()); 
            if (lstTempId != null && lstTempId.Count > 0)
            {
                foreach (string id in lstTempId)
                {
                    if (lstCov2019.IndexOf(id) >= 0)
                    {
                        isCov2019 = true;
                        string cardNo = new lisprintBiz().GetIdCardNo(m_dtbSample.Rows[0]["patientid_chr"].ToString().Trim());
                        if (!string.IsNullOrEmpty(cardNo))
                        {
                            //证件类型
                            m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold,
                               m_cardType, "身份证", fltColumn1, m_fltY);
                        }
                        else
                        {
                            //证件类型
                            m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold,
                               m_cardType, "".Trim(), fltColumn1, m_fltY);
                        }

                        //证件号码
                        m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold,
                            m_cardNo, cardNo, fltColumn2, m_fltY);

                        //Locate Y
                        m_fltY += 5 + m_printMethodTool.m_fltGetStringHeight(m_strSampleID, m_fntSmallBold);
                        break;
                    }

                }
            }
            #endregion

            //科  室
            m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold, m_strDepartment,
                m_dtbSample.Rows[0]["deptname_vchr"].ToString().Trim(), fltColumn1, m_fltY);


            //床  号
            m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold, m_strBedNo,
                m_dtbSample.Rows[0]["bedno_chr"].ToString().Trim(), fltColumn2, m_fltY);

            //样本类型
            m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold, m_strSampleType,
                m_dtbSample.Rows[0]["sample_type_desc_vchr"].ToString().Trim(), fltColumn3, m_fltY);

            //检验编号
            string temp_No = m_dtbSample.Rows[0]["check_no_chr"].ToString().Trim();
            m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallNotBold, m_strCheckNo,
                m_dtbSample.Rows[0]["check_no_chr"].ToString().Trim(), fltColumn4, m_fltY);
            try
            {
                if (temp_No.Substring(0, 2) == "18")
                {
                    m_strReference = "MIC";
                }
                else
                {
                    m_strReference = "参考区间";
                }
            }
            catch
            {

            }

            //Locate Y
            m_fltY += 5 + m_printMethodTool.m_fltGetStringHeight(m_strSampleID, m_fntSmallBold);

            m_printMethodTool.m_mthDrawLine(m_fltStartX - 5, m_fltY, m_fltPaperWidth * 0.9f, m_fltY);

            m_fltY += 5;

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
            string summaryStr = this.m_dtbSample.Rows[0]["SUMMARY_VCHR"].ToString().Trim() + "\r\n" + this.GetAllergenRemarkInfo(this.m_dtbSample.Rows[0]["application_id_chr"].ToString(),m_dtbSample.Rows[0]["SUMMARY_VCHR"].ToString().Trim(), m_dtbSample.Rows[0]["sex_chr"].ToString().Trim());
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
            //if (m_blnDocked)
            //{
            //    if (m_fltY < m_fltEndY)
            //    {
            //        m_fltY = m_fltEndY;
            //    }
            //}
            float m_fltEnd = 0.0f;
            m_fltEnd = m_fltEndY;

            m_fltEnd += 3;

            //画线
            //m_printMethodTool.m_mthDrawLine(m_fltStartX - 5, m_fltY, m_fltPaperWidth * 0.9f, m_fltY);

            m_fltEnd += 6;

            //column
            float fltColumn1 = m_fltStartX;
            float fltColumn2 = m_fltPaperWidth * 1.4f / 3;
            float fltColumn3 = m_fltPaperWidth * 2.1f / 3;

            bool isPrintCYSJ = false;
            bool isUseA4 = com.digitalwave.iCare.gui.HIS.clsPublic.ConvertObjToDecimal(com.digitalwave.iCare.gui.HIS.clsPublic.m_strReadXML("Lis", "IsUseA4", "AnyOne")) == 1 ? true : false;
            if (isUseA4) m_fltEnd -= 30;    // 50;

            if (m_dtbSample.Rows[0]["SAMPLING_DATE_DAT"] != DBNull.Value)
            {
                isPrintCYSJ = true;
            }
            if (!string.IsNullOrEmpty(mejerParm))
            {
                Image graph = new lisprintBiz().GetMejerImage(m_dtbSample.Rows[0]["application_id_chr"].ToString(),mejerParm);
                if (graph != null)
                {
                    float m_fltWidth = 0.9f * graph.Width;
                    float m_fltHeight = 0.9f * graph.Height;
                    m_printMethodTool.m_printEventArg.Graphics.DrawImage(graph, fltColumn3 - 190,
                        m_fltEnd - 180, m_fltWidth, m_fltHeight);
                }
            }
            if (isUseA4)
            {
                //m_fltEnd -= 3;
                //画线
                m_printMethodTool.m_mthDrawLine(m_fltStartX - 5, m_fltEnd, m_fltPaperWidth * 0.9f, m_fltEnd);
                m_fltEnd += 12;
            }
            else
            {
                //画线
                m_printMethodTool.m_mthDrawLine(m_fltStartX - 5, m_fltEnd, m_fltPaperWidth * 0.9f, m_fltEnd);
                m_fltEnd += 6;
            }
            if (isPrintCYSJ)
                //采样时间
                m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, "采样时间:", Convert.ToDateTime(m_dtbSample.Rows[0]["SAMPLING_DATE_DAT"]).ToString("yyyy-MM-dd HH:mm"),
                    fltColumn1, m_fltEnd);
            else
                //报告日期
                m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strReportDate, Convert.ToDateTime(m_dtbSample.Rows[0]["CONFIRM_DAT"]).ToString("yyyy-MM-dd HH:mm"),
                    fltColumn1, m_fltEnd);
            //检验医生
            if (m_dtbSample.Columns.IndexOf("reportorSign") >= 0)
            {

                if (isCov2019)
                {
                    if (m_dtbSample.Rows[0]["reportorSign"] == DBNull.Value)
                    {
                        m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strCheckDoc, m_dtbSample.Rows[0]["reportor"].ToString().Trim(), fltColumn2 - 102, m_fltEnd);
                    }
                    else
                    {
                        MemoryStream ms = new MemoryStream((byte[])m_dtbSample.Rows[0]["reportorSign"]);
                        m_printMethodTool.DrawImage(m_strCheckDoc, m_fntSmallBold, Image.FromStream(ms), fltColumn2 - 102, m_fltEnd, isUseA4);
                    }
                }
                else
                {
                    if (m_dtbSample.Rows[0]["reportorSign"] == DBNull.Value)
                    {
                        m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strCheckDoc, m_dtbSample.Rows[0]["reportor"].ToString().Trim(), fltColumn2, m_fltEnd);
                    }
                    else
                    {
                        MemoryStream ms = new MemoryStream((byte[])m_dtbSample.Rows[0]["reportorSign"]);
                        m_printMethodTool.DrawImage(m_strCheckDoc, m_fntSmallBold, Image.FromStream(ms), fltColumn2, m_fltEnd, isUseA4);
                    }
                }

            }
            else
            {
                m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strCheckDoc, m_dtbSample.Rows[0]["reportor"].ToString().Trim(), fltColumn2, m_fltEnd);
            }

            //审核者
            if (m_dtbSample.Columns.IndexOf("confirmerSign") >= 0)
            {
                if (isCov2019)
                {
                    if (m_dtbSample.Rows[0]["confirmerSign"] == DBNull.Value)
                    {
                        m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strConfirmEmp, m_dtbSample.Rows[0]["confirmer"].ToString().Trim(), fltColumn3 - 140, m_fltEnd);
                    }
                    else
                    {
                        MemoryStream ms = new MemoryStream((byte[])m_dtbSample.Rows[0]["confirmerSign"]);
                        m_printMethodTool.DrawImage(m_strConfirmEmp, m_fntSmallBold, Image.FromStream(ms), fltColumn3 - 140, m_fltEnd, isUseA4);
                    }
                }
                else
                {
                    if (m_dtbSample.Rows[0]["confirmerSign"] == DBNull.Value)
                    {
                        m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strConfirmEmp, m_dtbSample.Rows[0]["confirmer"].ToString().Trim(), fltColumn3, m_fltEnd);
                    }
                    else
                    {
                        MemoryStream ms = new MemoryStream((byte[])m_dtbSample.Rows[0]["confirmerSign"]);
                        m_printMethodTool.DrawImage(m_strConfirmEmp, m_fntSmallBold, Image.FromStream(ms), fltColumn3, m_fltEnd, isUseA4);
                    }
                }
            }
            else
            {
                m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, m_strConfirmEmp, m_dtbSample.Rows[0]["confirmer"].ToString().Trim(), fltColumn3, m_fltEnd);
            }
            if (isCov2019)
            {
                m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallBold, m_fntSmallBold, "检测机构：机构名称（盖章）", "", fltColumn3 + 10, m_fltEnd);
            }

            m_fltEnd += m_printMethodTool.m_fltGetStringHeight(m_strReportDate, m_fntSmallBold) + 6;

            ////画线
            //m_printMethodTool.m_mthDrawLine(m_fltStartX - 5, m_fltEnd, m_fltPaperWidth * 0.9f, m_fltEnd);
            //m_fltEnd += 6;

            // 采样时间
            float diff = 0;
            string str = string.Empty;
            if (isPrintCYSJ)
            {
                str = m_strReportDate;  // "采样时间:";
                m_printMethodTool.m_mthDrawString(str, m_fntSmallBold, m_fltStartX, m_fltEnd);
                diff = m_printMethodTool.m_fltGetStringWidth(str, m_fntSmallBold);
                str = Convert.ToDateTime(m_dtbSample.Rows[0]["CONFIRM_DAT"]).ToString("yyyy-MM-dd HH:mm");  // Convert.ToDateTime(m_dtbSample.Rows[0]["SAMPLING_DATE_DAT"]).ToString("yyyy-MM-dd HH:mm");
                m_printMethodTool.m_mthDrawString(str, m_fntSmallBold, m_fltStartX + diff + 5, m_fltEnd);
                diff += m_printMethodTool.m_fltGetStringWidth(str, m_fntSmallBold) + 65;
            }
            if (isCov2019)
            {
                //Notice
                m_printMethodTool.m_printEventArg.Graphics.DrawString(m_strNotice, new Font("SimSun", 11f, FontStyle.Regular), Brushes.Red, fltColumn2 - 102, m_fltEnd);
                //m_printMethodTool.m_mthDrawString(m_strNotice, m_fntSmallNotBold, m_fltStartX, m_fltY);
            }
            else
            {
                //Notice
                m_printMethodTool.m_printEventArg.Graphics.DrawString(m_strNotice, new Font("SimSun", 11f, FontStyle.Regular), Brushes.Red, m_fltStartX + diff, m_fltEnd);
                //m_printMethodTool.m_mthDrawString(m_strNotice, m_fntSmallNotBold, m_fltStartX, m_fltY);
            }

            float fltNoticeWidth = m_printMethodTool.m_fltGetStringWidth(m_strNotice, new Font("SimSun", 11f, FontStyle.Regular));
            //附注
            bool blnPrintAnnotation = false;
            if (m_dtbSample.Rows[0]["annotation_vchr"].ToString().Trim() != "" || m_blnAnnotationEmptyVisible)
            {
                blnPrintAnnotation = true;
            }
            if (blnPrintAnnotation)
            {
                m_printMethodTool.m_mthDrawTextAndContent(m_fntSmallNotBold, m_fntSmallNotBold, m_strAnnotation, m_dtbSample.Rows[0]["annotation_vchr"].ToString().Trim(),
                                m_fltStartX + fltNoticeWidth, m_fltEnd);
            }
        }
        private void m_mthPrintDetail()
        {
            string p_strContent = this.m_dtbSample.Rows[0]["SUMMARY_VCHR"].ToString().Trim() + "\r\n" + this.GetAllergenRemarkInfo(this.m_dtbSample.Rows[0]["application_id_chr"].ToString(), m_dtbSample.Rows[0]["SUMMARY_VCHR"].ToString().Trim(), m_dtbSample.Rows[0]["sex_chr"].ToString().Trim());
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
            string p_strContent = this.m_dtbSample.Rows[0]["summary_vchr"].ToString().Trim() + "\r\n" + this.GetAllergenRemarkInfo(this.m_dtbSample.Rows[0]["application_id_chr"].ToString(), m_dtbSample.Rows[0]["SUMMARY_VCHR"].ToString().Trim(), m_dtbSample.Rows[0]["sex_chr"].ToString().Trim());
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

                Graphics grfx = Graphics.FromImage(new Bitmap(1, 1));
                SizeF bounds1 = grfx.MeasureString(p_strContent, p_fntContent);
            }
            else
            {
                fltHeight = fltTitleHeight + p_fltTitleSpace + fltContentHeight;
            }
            SizeF sf = new SizeF(p_fltWidth, fltHeight);
            return sf;
        }

        private SizeF m_rectGetPrintStringRectangleGetPage(Font p_fntTitle, Font p_fntContent, string p_strContent, float p_fltWidth, float p_fltTitleSpace, float p_fltItemSpace)
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
                Graphics grfx = Graphics.FromImage(new Bitmap(1, 1));
                SizeF sfString = grfx.MeasureString(p_strContent, p_fntContent);
                fltHeight = sfString.Height;
            }
            else
            {
                fltHeight = fltTitleHeight + p_fltTitleSpace + fltContentHeight;
            }
            SizeF sf = new SizeF(p_fltWidth, fltHeight);
            return sf;
        }

        private SizeF m_rectGetPrintStringRectangle(Font p_fntTitle, Font p_fntContent, string p_strContent, float p_fltWidth, float p_fltTitleSpace, float p_fltItemSpace,int flg)
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

        public clsPrintPerPageInfo[] m_objConstructPrintPageInfo(DataTable p_dtbResult, float p_fltX, float p_fltY, float p_fltWidth, float p_fltHeight, float p_fltMaxHeight)
        {
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
            string strSummary = m_dtbSample.Rows[0]["summary_vchr"].ToString().Trim() + "\r\n" + GetAllergenRemarkInfo(m_dtbSample.Rows[0]["application_id_chr"].ToString(), m_dtbSample.Rows[0]["summary_vchr"].ToString().Trim(), m_dtbSample.Rows[0]["sex_chr"].ToString().Trim());
            SizeF sf = m_rectGetPrintStringRectangle(m_fntSmallBold, m_fntSmallNotBold, strSummary, m_fltPrintWidth, m_fltTitleSpace, m_fltItemSpace);
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

        public clsPrintPerPageInfo[] m_objConstructPrintPageInfoGetPage(DataTable p_dtbResult, float p_fltX, float p_fltY, float p_fltWidth, float p_fltHeight, float p_fltMaxHeight)
        {
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
            string strSummary = m_dtbSample.Rows[0]["summary_vchr"].ToString().Trim() + "\r\n" + GetAllergenRemarkInfo(m_dtbSample.Rows[0]["application_id_chr"].ToString(), m_dtbSample.Rows[0]["summary_vchr"].ToString().Trim(), m_dtbSample.Rows[0]["sex_chr"].ToString().Trim());
            SizeF sf = m_rectGetPrintStringRectangleGetPage(m_fntSmallBold, m_fntSmallNotBold, strSummary, m_fltPrintWidth, m_fltTitleSpace, m_fltItemSpace);
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
            lstCov2019 = new List<string>();
            lisprintBiz biz = new lisprintBiz();
            string parmValue = biz.m_strGetSysparm("7011");
            if (!string.IsNullOrEmpty(parmValue) && parmValue.Trim() != "")
            {
                this.lstAppUnitID = new List<string>();
                this.lstAppUnitID.AddRange(parmValue.Split(';'));
            }

            this.lstAidRemark = biz.GetAidRemark();

            //新冠报告参数
            string appUnitIdCov2019 = biz.m_strGetSysparm("7012");

            if (!string.IsNullOrEmpty(appUnitIdCov2019))
                lstCov2019 = new List<string>(appUnitIdCov2019.Split(';'));
            else
                lstCov2019 = new List<string>();

            mejerParm = biz.m_strGetSysparm("7015");

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
