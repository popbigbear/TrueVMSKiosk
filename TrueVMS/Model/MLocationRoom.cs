using System;
using System.Collections.Generic;

namespace TrueVMS.Model
{
    public partial class MLocationRoom
    {
        public MLocationRoom()
        {
            //CustProjectLocMappings = new HashSet<CustProjectLocMapping>();
            MLocationDoorMappings = new HashSet<MLocationDoorMapping>();
            //WorkpermitLocMappings = new HashSet<WorkpermitLocMapping>();
        }

        public int LocationRoomId { get; set; }
        public int LocationFloorId { get; set; }
        public string Title { get; set; }
        public string Remark { get; set; }
        public int? Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }

        public virtual MLocationFloor LocationFloor { get; set; }
        //public virtual MStatus StatusNavigation { get; set; }
        //public virtual ICollection<CustProjectLocMapping> CustProjectLocMappings { get; set; }
        public virtual ICollection<MLocationDoorMapping> MLocationDoorMappings { get; set; }
        //public virtual ICollection<WorkpermitLocMapping> WorkpermitLocMappings { get; set; }
    }
}
