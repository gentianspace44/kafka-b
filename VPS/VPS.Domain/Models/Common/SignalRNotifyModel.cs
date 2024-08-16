using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPS.Domain.Models.Common
{
    public class SignalRNotifyModel
    {
        public string? ClientId { get; set; }
        public string? Message { get; set; }
        public decimal Balance { get; set; }
    }
}
