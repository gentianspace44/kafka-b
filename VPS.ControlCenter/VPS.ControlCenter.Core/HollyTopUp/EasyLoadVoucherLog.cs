using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPS.ControlCenter.Core.HollyTopUp
{
    public class EasyLoadVoucherLog
    {
        public EasyLoadVoucherLog()
        {
                
        }


        [Key]
        public long VoucherLogId { get; set; }
        public long ClientID { get; set; }
        public string VoucherPin { get; set; }
        public decimal Amount { get; set; }
        public long VoucherReferenceID { get; set; }
        public Guid UniqueReference { get; set; }
        public int VoucherTypeID { get; set; }
        public bool CreditedOnSyx { get; set; }
        public string SyXPlatform { get; set; }
        public string? ApiResponse { get; set; }
        public DateTime DateAdded { get; set; }
        public int VoucherStatusID { get; set; }
        public string OriginEndpoint { get; set; }
        public DateTime? ApiResponseDate { get; set; }
        public string? SyxResponse { get; set; }
        public DateTime? SyxResponseDate { get; set; }
    }
}
