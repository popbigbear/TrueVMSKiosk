using System;
using System.Collections.Generic;


namespace TrueVMS.Model
{
    public partial class MLocationBuildingVw
    {
        public int LocationBuildingId { get; set; }
        public string LocationBuildingCode { get; set; }
        public int LocationSiteId { get; set; }
        public string Title { get; set; }
        public string Remark { get; set; }
        public int? Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public string StatusName { get; set; }
        public string LocationSiteName { get; set; }
        public string LocationSiteCode { get; set; }
        public string CreatedByName { get; set; }
        public string UpdatedByName { get; set; }
    }
}
