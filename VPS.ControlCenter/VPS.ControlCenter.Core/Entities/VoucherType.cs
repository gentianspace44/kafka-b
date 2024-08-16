using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPS.ControlCenter.Core.Entities
{
    public class VoucherType
    {
        [Key] 
        public int VoucherTypeId { get; set; }
        [MaxLength(150)]
        public required string Name { get; set; }

        [MaxLength(50)]
        public required string VoucherLength { get; set; }
    }
}
