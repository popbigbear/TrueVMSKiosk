using System;
using System.Collections.Generic;

namespace TrueVMS.Model
{
    public partial class MLocationDoorVw
    {
        public int LocationDoorId { get; set; }
        public string LocationDoorCode { get; set; }
        public string LocationDoorName { get; set; }
        public int? Sequence { get; set; }
        public string Remark { get; set; }
        public int? Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public string StatusName { get; set; }
        public string LocationRoomIdList { get; set; }
        public string CreatedByName { get; set; }
        public string UpdatedByName { get; set; }
    }
}
