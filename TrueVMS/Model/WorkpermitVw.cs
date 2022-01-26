using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueVMS.Model
{
    public class WorkpermitVw
    {

        public string WorkpermitId { get; set; }
        public string WorkpermitCode { get; set; }
        public string AccountCode { get; set; }
        public string CustomerCode { get; set; }
        public DateTime WorkStartDatetime { get; set; }
        public DateTime WorkEndDatetime { get; set; }
        public int? CustStaffId { get; set; }
        public int? TotalAttendeesCount { get; set; }
        public bool? WithEquipmentIn { get; set; }
        public string Objective { get; set; }
        public string Rack { get; set; }
        public int? Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public string CustProjectName { get; set; }
        public string CustCompanyName { get; set; }
        public string CustStaffName { get; set; }
        public long? Attendees { get; set; }
        public string StatusName { get; set; }
        public string Location { get; set; }
    }
}
