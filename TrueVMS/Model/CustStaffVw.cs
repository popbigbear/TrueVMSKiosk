using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueVMS.Model
{
    public class CustStaffVw
    {


        public long CustStaffId { get; set; }
    public int CustCompanyId { get; set; }
    public int CustGroupId { get; set; }
    public int CustStaffTypeId { get; set; }
    public string CustStaffName { get; set; }
    public string CustStaffMobile { get; set; }
    public string CustStaffEmail { get; set; }
    public string CustStaffCompanyName { get; set; }
    public string CustStaffCardType { get; set; }
    public string CustStaffCardId { get; set; }
    public string CustStaffCarLicense { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public int? LastReviewDayLeft { get; set; }
    public string Remark { get; set; }
    public int? Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int? UpdatedBy { get; set; }
    public decimal? TcVersion { get; set; }
    public decimal? PrivacyMarketingVersion { get; set; }
    public decimal? PrivacySensitiveVersion { get; set; }
    public string CustCompanyName { get; set; }
    public string StaffTypeName { get; set; }
    public string CustGroupName { get; set; }
    public string CreatedByName { get; set; }
    public string UpdatedByName { get; set; }
}
}
