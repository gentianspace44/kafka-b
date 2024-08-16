using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPS.ControlCenter.Core.Entities
{
    public class VoucherProvider
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VoucherProviderId { get; set; }
        [MaxLength(250)]
        public required string Name { get; set; }
        public required int VoucherTypeId { get; set; }
        [MaxLength(1000)]
        public required string ImageSource { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsVisible { get; set; }
        [MaxLength(1000)]
        public required string MicroServiceUrl { get; set; }


        [MaxLength(1000)]
        public string? SyxCreditServiceUrl { get; set; }

        public bool? UseSxyCreditEndPoint { get; set; }

        //Nav propties
        public VoucherType VoucherType { get; set; }


    }
}
