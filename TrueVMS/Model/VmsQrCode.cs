using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueVMS.Model
{
    public partial class VmsQrCode
    {
        public long QrCodeId { get; set; }
        public DateTime QrCodeGenerateDatetime { get; set; }
        public int QrCodeLifetimeSecond { get; set; }
        public string BundledOtp { get; set; }
        public string RefWorkpermitId { get; set; }
        public string RefUserTable { get; set; }
        public int RefUserId { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }

        //public virtual MStatus StatusNavigation { get; set; }
    }
}
