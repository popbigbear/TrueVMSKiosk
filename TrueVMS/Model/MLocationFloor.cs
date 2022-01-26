using System;
using System.Collections.Generic;

namespace TrueVMS.Model
{
    public partial class MLocationFloor
    {
        public MLocationFloor()
        {
            MLocationRooms = new HashSet<MLocationRoom>();
        }

        public int LocationFloorId { get; set; }
        public int LocationBuildingId { get; set; }
        public int? Sequence { get; set; }
        public string Title { get; set; }
        public string Remark { get; set; }
        public int? Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }

        public virtual MLocationBuilding LocationBuilding { get; set; }
        //public virtual MStatus StatusNavigation { get; set; }
        public virtual ICollection<MLocationRoom> MLocationRooms { get; set; }
    }
}
