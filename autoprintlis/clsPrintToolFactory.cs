using com.digitalwave.iCare.gui.LIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace autoprintlis
{
    internal class clsPrintToolFactory
    {
        public static infPrintRecord Create(string reportGroupId)
        {
            infPrintRecord result = null;
            if (reportGroupId != null)
            {
                if (reportGroupId == "000001")
                {
                    result = new clsMarrowReportPrintTool();
                    clsMarrowReportPrintTool.blnSurePrintDiagnose = true;
                    return result;
                }
                if (reportGroupId == "000002")
                {
                    result = new clsGermReportPrinTool();
                    clsGermReportPrinTool.blnSurePrintDiagnose = true;
                    return result;
                }
                if (reportGroupId == "000004")
                {
                    result = new clsGermReportPrinToolV2();
                    clsGermReportPrinToolV2.blnSurePrintDiagnose = true;
                    return result;
                }
            }
            string text = null;
            long num = clsPrintToolFactory.m_lngGetCollocate(out text, "4003");
            if (num > 0L)
            {
                if (text != "")
                {
                    string text2 = text;
                    if (text2 != null)
                    {
                        if (text2 == "0")
                        {
                            result = new clsUnifyReportPrint();
                            clsUnifyReportPrint.blnSurePrintDiagnose = true;
                            return result;
                        }
                    }
                    result = new clsUnifyReportPrint();
                    clsUnifyReportPrint.blnSurePrintDiagnose = true;
                }
            }
            return result;
        }
        public static long m_lngGetCollocate(out string p_strFlag, string p_strSetID)
        {
            p_strFlag = null;
            IPrincipal p_objPrincipal = null;
            lisprintBiz biz = new lisprintBiz();
            return biz.m_lngGetCollocate(p_objPrincipal, out p_strFlag, p_strSetID);
        }
    }
}
