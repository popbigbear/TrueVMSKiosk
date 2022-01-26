using System;
using System.Collections.Generic;

namespace TrueVMS.Model
{
    public partial class MLocationSite
    {
        public MLocationSite()
        {
            MLocationBuildings = new HashSet<MLocationBuilding>();
            //VmsActionLogs = new HashSet<VmsActionLog>();
            //VmsUserLocMappings = new HashSet<VmsUserLocMapping>();
            //WorkpermitLocMappings = new HashSet<WorkpermitLocMapping>();
        }

        public int LocationSiteId { get; set; }
        public string LocationSiteCode { get; set; }
        public string Title { get; set; }
        public string Remark { get; set; }
        public int? Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }

        //public virtual MStatus StatusNavigation { get; set; }
        public virtual ICollection<MLocationBuilding> MLocationBuildings { get; set; }
        //public virtual ICollection<VmsActionLog> VmsActionLogs { get; set; }
        //public virtual ICollection<VmsUserLocMapping> VmsUserLocMappings { get; set; }
        //public virtual ICollection<WorkpermitLocMapping> WorkpermitLocMappings { get; set; }
    }
}
