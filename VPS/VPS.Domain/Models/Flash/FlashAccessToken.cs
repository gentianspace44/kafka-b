using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPS.Domain.Models.Flash
{
    public class FlashAccessToken
    {
        public string Access_Token { get; set; } = string.Empty;
        public string Sscope { get; set; } = string.Empty;
        public string Token_Type { get; set; } = string.Empty;
        public string Expires_In { get; set; } = string.Empty;
    }
}
