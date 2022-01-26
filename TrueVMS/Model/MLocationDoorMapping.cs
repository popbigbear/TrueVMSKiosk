using System;
using System.Collections.Generic;


namespace TrueVMS.Model
{
    public partial class MLocationDoorMapping
    {
        public int LocationDoorMappingId { get; set; }
        public int? LocationDoorId { get; set; }
        public int? LocationRoomId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }

        public virtual MLocationDoor LocationDoor { get; set; }
        public virtual MLocationRoom LocationRoom { get; set; }
    }
}
