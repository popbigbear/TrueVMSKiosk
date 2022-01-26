using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueVMS.Model
{
    public class ActionLogModel
    {

        public DateTime LogTime { get; set; }
        public string CardNo { get; set; }
        public long WorkPermitID { get; set; }
        public int walkinObjectiveID { get; set; }
        public string StaffTypeName { get; set; }
        public string VisitorType { get; set; }
        public int LocationSiteId { get; set; }
        public int LocationBuildingId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string CustStaffCardType { get; set; }
        public string CustStaffCardId { get; set; }
        public string NodeType { get; set; }
        public string NodeID { get; set; }
        public string NodeIP { get; set; }
        public string Type { get; set; }
        public string Event { get; set; }
        public string SubEvent { get; set; }
        public string Description { get; set; }


        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
