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
        public static infPrintRecord Create(string reportGroupId )
        {
            infPrintRecord printTool = null;
            switch (reportGroupId)
            {
                case "000001":
                    printTool = new clsMarrowReportPrintTool();
                    clsMarrowReportPrintTool.blnSurePrintDiagnose = true;
                    break;
                case "000002":
                    printTool = new clsGermReportPrinTool();
                    clsGermReportPrinTool.blnSurePrintDiagnose = true;
                    break;
                case "000004":
                    printTool = new clsGermReportPrinToolV2();
                    clsGermReportPrinToolV2.blnSurePrintDiagnose = true;
                    break;
                default:
                    string strFlag = null;
                    //4003:检验报告格式  0:默认格式 1:格式一 2:格式二
                    long lngRes = m_lngGetCollocate(out strFlag, "4003");
                    if (lngRes > 0)
                    {
                        if (strFlag != "")
                        {
                            switch (strFlag)
                            {
                                case "0":
                                    printTool = new clsUnifyReportPrint();
                                    clsUnifyReportPrint.blnSurePrintDiagnose = true;

                                    break;
                                //case "1":
                                //    printTool = new clsUnifyReportPrintForChildHospital();
                                //    clsUnifyReportPrintForChildHospital.blnSurePrintDiagnose = true;
                                //    break;
                                //case "2":
                                //    printTool = new clsUnifyReportPrintForChildHospital_B5();
                                //    clsUnifyReportPrintForChildHospital_B5.blnSurePrintDiagnose = true;
                                //    break;
                                default:
                                    printTool = new clsUnifyReportPrint();
                                    clsUnifyReportPrint.blnSurePrintDiagnose = true;
                                    break;
                            }
                        }
                    }
                    break;
            }

            return printTool;
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
