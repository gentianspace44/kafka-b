using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPS.ControlCenter.Logic.Models
{
    public class SecurityTokenDto : BaseResponse
    {
        public SecurityTokenDto()
        {

        }
        public SecurityTokenDto(TokenModel token, TokenModel refreshToken)
        {
            AccessToken = token;
            RefreshToken = refreshToken;
        }

        public TokenModel AccessToken { get; set; }
        public TokenModel RefreshToken { get; set; }
        public bool IsActive { get; set; }
        public int UserId { get; set; }
    }
}
