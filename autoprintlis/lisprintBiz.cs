using com.digitalwave.iCare.gui.LIS;
using com.digitalwave.iCare.ValueObject;
using com.digitalwave.security;
using com.digitalwave.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Text;
using weCare.Core.Dac;
using weCare.Core.Utils;

namespace autoprintlis
{
    public class lisprintBiz
    {
        #region  QueryAreaReport
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="ipNo"></param>
        /// <returns></returns>
        public List<entityLisInfo> QueryAreaReport(string startDate, string endDate, string ipNo)
        {
            string Sql = string.Empty;
            DataTable dt = null;
            SqlHelper svc = null;
            List<entityLisInfo> data = new List<entityLisInfo>();

            try
            {
                svc = new SqlHelper(EnumBiz.onlineDB);

                Sql = @"select distinct a.name_vchr,
                        c.application_id_chr,
                        c.appl_dat,
                        c.patient_name_vchr,
                        c.sex_chr,
                        c.age_chr,
                        c.patient_inhospitalno_chr,
                        c.bedno_chr,
                        c.barcode_vchr,
                        d.report_group_id_chr,
                        d.report_print_chr,
                        d.report_dat,
                        d.confirm_dat
                        from t_opr_bih_order a
                        inner join t_opr_attachrelation b
                        on a.orderid_chr = b.sourceitemid_vchr
                        inner join t_opr_lis_sample c
                        on b.attachid_vchr = c.application_id_chr
                        inner join t_opr_lis_app_report d
                        on c.application_id_chr = d.application_id_chr
                        where {0} (c.appl_dat between ? and ?) 
                        and d.report_dat is not null {1} order by c.appl_dat, 
                        c.patient_inhospitalno_chr  ";
                IDataParameter[] param = null;
                DateTime dateTime = Convert.ToDateTime(startDate + " 00:00:00");
                DateTime dateTime2 = Convert.ToDateTime(endDate + " 23:59:59");

                if (!string.IsNullOrEmpty(ipNo))
                {
                    Sql = string.Format(Sql, "", "and trim(c.patient_inhospitalno_chr) = ?");
                    param = svc.CreateParm(3);
                    param[0].Value = dateTime;
                    param[1].Value = dateTime2;
                    param[2].Value = ipNo;
                }

                dt = svc.GetDataTable(Sql,param);

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        entityLisInfo vo = new entityLisInfo();
                        vo.cardNo = dr["patient_inhospitalno_chr"].ToString();
                        vo.barCode = dr["barcode_vchr"].ToString();
                        vo.patName = dr["patient_name_vchr"].ToString();
                        vo.sex = dr["sex_chr"].ToString();
                        vo.age = dr["age_chr"].ToString();
                        vo.name = dr["name_vchr"].ToString();
                        vo.appDate = Function.Datetime(dr["appl_dat"]).ToString("yyyy-MM-dd HH:mm");
                        vo.rptDate = Function.Datetime(dr["report_dat"]).ToString("yyyy-MM-dd HH:mm");
                        vo.rptGroupId = dr["report_group_id_chr"].ToString();
                        vo.applicationId = dr["application_id_chr"].ToString();
                        vo.n = 0;

                        if (data.Any(t => t.applicationId == vo.applicationId && t.rptGroupId == vo.rptGroupId))
                        {
                            int i = data.FindIndex(t => t.applicationId == vo.applicationId && t.rptGroupId == vo.rptGroupId);
                            data[i].name = data[i].name + "," + vo.name;
                        }
                        else
                        {
                            data.Add(vo);
                            for (int i = 0; i < data.FindAll(t => t.cardNo == vo.cardNo).Count; i++)
                            {
                                int j = data.FindIndex(t => t.cardNo == vo.cardNo)+i;
                                data[j].n++;
                            }
                        }
                    }
                }
            }
            catch (Exception objEx)
            {
                ExceptionLog.OutPutException("QueryAreaReport-->"+objEx);
            }
            return data;
        }
        #endregion
        
        #region  QueryReport
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="ipNo"></param>
        /// <returns></returns>
        public List<entityLisInfo> QueryReport(string startDate, string endDate, string cardNo,ref int printed,ref int unPrinted)
        {
            string Sql = string.Empty;
            DataTable dt = null;
            SqlHelper svc = null;
            int n = 0;
            List<entityLisInfo> data = new List<entityLisInfo>();

            try
            {
                svc = new SqlHelper(EnumBiz.onlineDB);

                Sql = @"select distinct 
                                d.patientcardid_chr, 
                                s.application_id_chr,
                                s.appl_dat,
                                b.report_dat,
                                a.check_content_vchr,
                                s.patient_name_vchr,
                                s.sex_chr,
                                s.age_chr,
                                s.patient_inhospitalno_chr,
                                b.report_group_id_chr,
                                b.report_print_chr,
                                b.status_int
                                from t_opr_lis_application a
                                inner join t_opr_lis_app_report b
                                on a.application_id_chr = b.application_id_chr
                                and b.status_int > 0
                                inner join t_bse_patientcard d
                                on a.patientcardid_chr = d.patientcardid_chr 
                                and d.status_int != 0
                                left join t_opr_lis_sample s
                                on a.application_id_chr = s.application_id_chr
                                where a.pstatus_int = 2
                                and b.status_int = 2
                                and b.report_dat is not null
                                and s.status_int >= 3
                                and s.status_int <= 6 
                                and s.patient_type_chr = 2
                                and d.patientcardid_chr = ?
                                and s.modify_dat between to_date(?,'yyyy-mm-dd hh24:mi:ss')
                                    and to_date(?,'yyyy-mm-dd hh24:mi:ss')
                                order by s.application_id_chr  ";

             
                IDataParameter[] param = null;

                param = svc.CreateParm(3);
                param[0].Value = cardNo;
                param[1].Value = startDate + " 00:00:00"; ;
                param[2].Value = endDate + " 23:59:59";
                
                dt = svc.GetDataTable(Sql, param);

                #region 赋值
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        entityLisInfo vo = new entityLisInfo();
                        //vo.barCode = dr["barcode_vchr"].ToString();
                        vo.name = dr["check_content_vchr"].ToString();
                        vo.cardNo = dr["patientcardid_chr"].ToString();
                        vo.patName = dr["patient_name_vchr"].ToString();
                        vo.sex = dr["sex_chr"].ToString();
                        vo.age = dr["age_chr"].ToString();
                        vo.appDate = Function.Datetime(dr["appl_dat"]).ToString("yyyy-MM-dd HH:mm");
                        vo.rptDate = Function.Datetime(dr["report_dat"]).ToString("yyyy-MM-dd HH:mm");
                        vo.rptGroupId = dr["report_group_id_chr"].ToString();
                        vo.applicationId = dr["application_id_chr"].ToString();
                        vo.checkContent = dr["check_content_vchr"].ToString().Trim();
                        vo.printeded = dr["report_print_chr"].ToString();
                        if (vo.printeded == "" || vo.printeded == "0")
                        {
                            unPrinted++;
                            if (vo.checkContent.Contains("性激素6项") && vo.checkContent.Contains("绒毛膜促性腺激素定量"))
                            {
                                unPrinted++;
                                entityLisInfo vo1 = new entityLisInfo();
                                vo1.checkContent = dr["check_content_vchr"].ToString().Trim();
                                vo1.printeded = "1";
                                data.Add(vo1);
                            }
                            else 
                            {
                                vo.checkContent = string.Empty;
                            }
                        }
                        else
                            printed++;

                        vo.n = ++n;
                        data.Add(vo);                   
                    }
                }
                #endregion
            }
            catch (Exception objEx)
            {
                ExceptionLog.OutPutException("QueryReport-->" + objEx);
                printed = -1;
            }
            return data;
        }
        #endregion

        public long m_lngGetReportObject(IPrincipal p_objPrincipal, string p_strApplicationID, out clsReportObject p_objReportObject)
        {
            p_objReportObject = null;
            long result = -1;

            SqlHelper svc = null;
            svc = new SqlHelper(EnumBiz.onlineDB);
            string Sql = "select * from t_opr_lis_report_object  where application_id_chr = ? ";
            IDataParameter[] param = null;

            param = svc.CreateParm(1);
            param[0].Value = p_strApplicationID;
            DataTable dataTable = null ;
            dataTable = svc.GetDataTable(Sql,param);

            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                p_objReportObject = new clsReportObject();
                p_objReportObject.strApplicationID = p_strApplicationID;
                p_objReportObject.bytReportObjectArr = (dataTable.Rows[0]["REPORT_OBJECT_LOB"] as byte[]);
                result = 1;
            }

            return result;
        }

        public long m_lngGetReportInfoByReportGroupIDAndApplicationID(IPrincipal p_objPrincipal, string p_strReportGroupID, string p_strApplID, bool p_blnConfirmed, out DataTable p_dtbReportInfo)
        {
            p_dtbReportInfo = null;
            long result = -1;
            
            SqlHelper svc = null;
            svc = new SqlHelper(EnumBiz.onlineDB);

            string str = "";
            if (p_blnConfirmed)
                str = " AND t2.status_int = 2";
            else
                str = " AND t2.status_int > 0";

            string Sql = @"SELECT t1.*, t2.*, t4.deptname_vchr, t5.lastname_vchr AS reportor,
                            t6.lastname_vchr AS confirmer, t7.lastname_vchr AS applyer,
                            t8.sample_type_desc_vchr, t9.application_form_no_chr AS check_no_chr,
                            t10.print_title_vchr, t9.SUMMARY_VCHR AS application_summary,
                            t21.sign_grp as reportorsign, t22.sign_grp as confirmersign, 
                            t23.sign_grp as applyersign 
                            FROM t_opr_lis_sample t1,
                            t_opr_lis_app_report t2,
                            t_opr_lis_app_sample t3,
                            T_BSE_DEPTDESC t4,
                            t_bse_employee t5,
                            t_bse_employee t6,
                            t_bse_employee t7,
                            t_aid_lis_sampletype t8,
                            t_opr_lis_application t9,
                            t_aid_lis_report_group t10,
                            t_bse_empsign t21,
                            t_bse_empsign t22,
                            t_bse_empsign t23 
                            WHERE t2.application_id_chr = ? 
                            AND t2.report_group_id_chr = ?
                            AND t2.report_group_id_chr = t3.report_group_id_chr
                            AND t2.application_id_chr = t3.application_id_chr
                            AND t3.sample_id_chr = t1.sample_id_chr
                            AND t2.reportor_id_chr = t5.empid_chr(+)
                            AND t2.confirmer_id_chr = t6.empid_chr(+)
                            AND t9.appl_empid_chr = t7.empid_chr(+)
                            AND t9.appl_deptid_chr = t4.deptid_chr(+)
                            AND t1.sample_type_id_chr = t8.sample_type_id_chr(+)
                            AND t2.application_id_chr = t9.application_id_chr
                            AND t2.report_group_id_chr = t10.report_group_id_chr
                            and t5.empid_chr = t21.empid_chr(+)
                            and t6.empid_chr = t22.empid_chr(+)
                            and t7.empid_chr = t23.empid_chr(+)
                            AND t9.pstatus_int > 0
                            AND t1.status_int > 0
                            AND t2.status_int > 0";

            Sql += str;

            try
            {
                IDataParameter[] param = null;
                param = svc.CreateParm(2);
                param[0].Value = p_strApplID;
                param[1].Value = p_strReportGroupID;
                p_dtbReportInfo = svc.GetDataTable(Sql, param);
            }
            catch (Exception objEx)
            {
                ExceptionLog.OutPutException("m_lngGetReportInfoByReportGroupIDAndApplicationID-->" + objEx);
            }
            if (p_dtbReportInfo != null && p_dtbReportInfo.Rows.Count > 0)
                result = 1;

            return result;
        }

        public long m_lngGetCheckResultByReportGroupIDAndApplicationID(IPrincipal p_objPrincipal, string p_strApplicationID, string p_strReportGroupID, bool p_blnConfirmed, out DataTable p_dtbCheckResult)
        {
            p_dtbCheckResult = null;
            long result = -1;
            string str = "";
            if (p_blnConfirmed)
                str = " and t2.status_int = 2";
            else
                str = " and t2.status_int > 0";

            string Sql = @"select t4.oringin_dat,t3.sample_id_chr
                                    from t_opr_lis_app_report t2,
                                    t_opr_lis_app_sample t3, t_opr_lis_application t4
                                    where t2.application_id_chr = ?
                                    and t2.report_group_id_chr = ?
                                    and t4.application_id_chr = t2.application_id_chr
                                    and t4.pstatus_int >= 0
                                    and t3.application_id_chr = t2.application_id_chr
                                    and t3.report_group_id_chr = t2.report_group_id_chr ";

            string Sql2 = @"select /*+ use_hash(t1) */
                                    t1.*,t9.print_title_vchr,
                                    t5.print_seq_int as report_print_seq_int,
                                    t8.item_print_seq_int as sample_print_seq_int,
                                    t7.rptno_chr,
                                    t7.check_item_english_name_vchr,
                                    t7.assist_code02_chr as item_type,
                                    t7.shortname_chr
                                    from (select /*+ all_rows */
                                    *
                                    from t_opr_lis_check_result
                                    where sample_id_chr = ?
                                    and modify_dat >= to_date(?, 'yyyy-mm-dd hh24:mi:ss')
                                    and status_int = 1) t1,
                                    t_aid_lis_report_group_detail t5,
                                    t_bse_lis_check_item t7,
                                    v_lis_bse_sample_group_items t8,
                                    t_aid_lis_sample_group t9
                                    where t9.sample_group_id_chr = t1.groupid_chr
                                    and t7.check_item_id_chr = t1.check_item_id_chr
                                    and t8.check_item_id_chr = t1.check_item_id_chr
                                    and t8.sample_group_id_chr = t1.groupid_chr
                                    and t5.sample_group_id_chr = t1.groupid_chr";
            Sql += str;
            try
            {
                SqlHelper svc = null;
                svc = new SqlHelper(EnumBiz.onlineDB);
                IDataParameter[] param = null;
                param = svc.CreateParm(2);
                param[0].Value = p_strApplicationID;
                param[1].Value = p_strReportGroupID;
                p_dtbCheckResult = svc.GetDataTable(Sql,param);

                if (p_dtbCheckResult != null && p_dtbCheckResult.Rows.Count > 0)
                {
                    string value = p_dtbCheckResult.Rows[0]["sample_id_chr"].ToString().Trim();
                    string value2 = p_dtbCheckResult.Rows[0]["oringin_dat"].ToString().Trim();
                    IDataParameter[] param2 = null;
                    param2 = svc.CreateParm(2);
                    param2[0].Value = value;
                    param2[1].Value = value2;
                    p_dtbCheckResult = null;
                    p_dtbCheckResult = svc.GetDataTable(Sql2,param2);
                }
            }
            catch (Exception objEx)
            {
                ExceptionLog.OutPutException("m_lngGetCheckResultByReportGroupIDAndApplicationID-->" + objEx);
            }
            if (p_dtbCheckResult != null && p_dtbCheckResult.Rows.Count > 0)
                result = 1;

            return result;
        }

        public long m_lngUpdatePrinctTime(IPrincipal p_objPrincipal, string p_strApplicaionID)
        {
            long result = 0;

            try
            {
                SqlHelper svc = new SqlHelper(EnumBiz.onlineDB);

                string Sql = @"update t_opr_lis_app_report t
                                set t.report_print_chr = report_print_chr + 1,
                                t.report_print_dat = decode(report_print_chr,
                                0,sysdate,report_print_dat)
                                where t.application_id_chr = ?
                                and status_int = 2";
                IDataParameter[] param = null;
                param = svc.CreateParm(1);
                param[0].Value = p_strApplicaionID;
                result = svc.ExecSql(Sql, param);
            }
            catch (Exception objEx)
            {
                ExceptionLog.OutPutException("m_lngUpdatePrinctTime-->"+objEx);
            }
            finally
            {
            }

            return result;
        }

        public long m_lngGetCollocate(IPrincipal p_objPrincipal, out string strFlag, string strsetid)
        {
            long result = 0;
            strFlag = "";
            
            string strSQLCommand = "select SETSTATUS_INT from t_sys_setting where  setid_chr='" + strsetid + "'";
            DataTable dataTable = new DataTable();
            try
            {
                SqlHelper svc = new SqlHelper(EnumBiz.onlineDB);
                dataTable = svc.GetDataTable(strSQLCommand);
            }
            catch (Exception ex)
            {
                ExceptionLog.OutPutException("m_lngGetCollocate-->" + ex);
            }
            if (dataTable.Rows.Count > 0 && dataTable.Rows[0]["SETSTATUS_INT"].ToString().Trim() != "")
            {
                strFlag = dataTable.Rows[0]["SETSTATUS_INT"].ToString().Trim();
            }
            if (dataTable != null && dataTable.Rows.Count > 0)
                result = 1;

            return result;
        }

        public int m_intGetSysParm(string setid)
        {
            int result = -999;
            try
            {
                SqlHelper svc = new SqlHelper(EnumBiz.onlineDB);
                IDataParameter[] param = null;
                param = svc.CreateParm(1);
                param[0].Value = setid;
                string Sql = "select setstatus_int from t_sys_setting where setid_chr = ?";
                DataTable dataTable = new DataTable();
                dataTable = svc.GetDataTable(Sql,param);
                if (dataTable.Rows.Count == 1)
                {
                    result = Convert.ToInt32(dataTable.Rows[0][0].ToString());
                }
            }
            catch (Exception objEx)
            {
                ExceptionLog.OutPutException("m_intGetSysParm-->" + objEx);
            }
            return result;
        }

        public string m_strGetSysparm(string parmcode)
        {
            string result = "";
            try
            {
                string Sql = @"select parmvalue_vchr from t_bse_sysparm where status_int = 1 and parmcode_chr = ?";
                DataTable dataTable = new DataTable();
                SqlHelper svc = new SqlHelper(EnumBiz.onlineDB);
                IDataParameter[] param = null;
                param = svc.CreateParm(1);
                param[0].Value = parmcode;
                dataTable = svc.GetDataTable(Sql, param);
                if (dataTable.Rows.Count > 0)
                {
                    result = dataTable.Rows[0][0].ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                ExceptionLog.OutPutException("m_strGetSysparm-->"+ex);
            }
            return result;
        }

        public long m_lngGetReportPrintInfo(string p_strReportGroupID, string p_strApplID, bool p_blnConfirmed, out clsPrintValuePara p_objPrintContent)
        {
            p_objPrintContent = null;
            long num = 0L;
            DataTable dtbBaseInfo = null;
            DataTable dtbResult = null;
            
            num = m_lngGetReportInfoByReportGroupIDAndApplicationID(null, p_strReportGroupID, p_strApplID, p_blnConfirmed, out dtbBaseInfo);
            if (num > 0L)
            {
                num = 0L;
                num = m_lngGetCheckResultByReportGroupIDAndApplicationID(null, p_strApplID, p_strReportGroupID, p_blnConfirmed, out dtbResult);
            }
            if (num > 0L)
            {
                p_objPrintContent = new clsPrintValuePara();
                p_objPrintContent.m_dtbBaseInfo = dtbBaseInfo;
                p_objPrintContent.m_dtbResult = dtbResult;
            }
            return num;
        }

        public string getPatName(string cardNo)
        {
            string name = string.Empty;

            try
            {
                string Sql = @"select a.lastname_vchr from t_bse_patient a 
                                left join t_bse_patientcard b 
                                on a.patientid_chr = b.patientid_chr
                                where b.patientcardid_chr = ? ";
                DataTable dataTable = new DataTable();
                SqlHelper svc = new SqlHelper(EnumBiz.onlineDB);
                IDataParameter[] param = null;
                param = svc.CreateParm(1);
                param[0].Value = cardNo;
                dataTable = svc.GetDataTable(Sql, param);
                if (dataTable.Rows.Count > 0)
                {
                    name = dataTable.Rows[0][0].ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                ExceptionLog.OutPutException("getPatName-->" + ex);
            }

            return name;
        }

    }
}
