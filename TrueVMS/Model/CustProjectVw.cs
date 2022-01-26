using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueVMS.Model
{
    public class CustProjectVw
    {

        public int CustProjectId { get; set; }
        public string CustomerCode { get; set; }
        public string CustProjectName { get; set; }
        public int CustCompanyId { get; set; }
        public DateTime CustProjectStart { get; set; }
        public DateTime CustProjectEnd { get; set; }
        public int IdcServerCatId { get; set; }
        public string Remark { get; set; }
        public DateTime? LastReviewDate { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public int? LastReviewDayLeft { get; set; }
        public int? Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public string CustCompanyName { get; set; }
        public string IdcServerCatName { get; set; }
        public string StatusName { get; set; }
        public string Location { get; set; }
        public string LocationRoomIdList { get; set; }
        public string UserIdList { get; set; }
        public string CreatedByName { get; set; }
        public string UpdatedByName { get; set; }
    }
}
