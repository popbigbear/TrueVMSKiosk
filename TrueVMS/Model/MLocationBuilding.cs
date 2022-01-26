using System;
using System.Collections.Generic;


namespace TrueVMS.Model
{
    public partial class MLocationBuilding
    {
        public MLocationBuilding()
        {
            MLocationFloors = new HashSet<MLocationFloor>();
            //VmsActionLogs = new HashSet<VmsActionLog>();
        }

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

        public virtual MLocationSite LocationSite { get; set; }
        //public virtual MStatus StatusNavigation { get; set; }
        public virtual ICollection<MLocationFloor> MLocationFloors { get; set; }
        //public virtual ICollection<VmsActionLog> VmsActionLogs { get; set; }
    }
}
