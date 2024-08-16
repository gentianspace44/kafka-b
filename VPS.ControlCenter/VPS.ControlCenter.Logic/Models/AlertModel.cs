using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPS.ControlCenter.Logic.Models
{
    public class AlertModel
    {
        public string Message { get; set; }
        public int Order { get; set; }
        public bool IsVisible { get; set; }
        public string Title { get; set; }
    }
}
