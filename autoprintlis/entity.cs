using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace autoprintlis
{
    public class entityLisInfo
    {
        public int n { get; set; }
        public string cardNo { get; set; }
        public string barCode { get; set; }
        public string patName { get; set; }
        public string sex{ get; set; }
        public string age { get; set; }
        public string name { get; set; }
        public string appDate { get; set; }
        public string rptDate { get; set; }
        public string rptGroupId { get; set; }
        public string applicationId { get; set; }
        public string printeded { get; set; }
        public string checkContent { get; set; }
    }

    public class clsDeviceReslutVO
    {
        public string m_strAbnormalFlag { get; set; }
        public string m_strDeviceCheckItemName { get; set; }
        public string m_strResult { get; set; }
        public string m_strDeviceSampleID { get; set; }
    }

    public class EntityAidRemark
    {
        public string appUnitId { get; set; }
        public string appUnitName { get; set; }
        public int sex { get; set; }
        public int highOrLow { get; set; }
        public string remarkInfo { get; set; }
        public string keyWord { get; set; }
        public int appunitgroup { get; set; }
        public string checkItemId { get; set; }
    }
}
