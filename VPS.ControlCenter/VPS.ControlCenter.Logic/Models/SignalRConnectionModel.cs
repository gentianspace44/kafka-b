using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPS.ControlCenter.Logic.Models
{
    public class SignalRConnectionModel
    {
        public string ConnectionId  { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public SignalRConnectionModel(string connectionId)
        {
            ConnectionId = connectionId;
        }
    }
}
