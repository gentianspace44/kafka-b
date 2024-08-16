using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPS.ControlCenter.Logic.Models;

namespace VPS.ControlCenter.Logic.IServices
{
    public interface IAuthenticationService
    {
        Task<SecurityTokenDto> Login(LoginDto loginDto);
        Task<SecurityTokenDto> RefreshToken(string token);
        Task<bool> Logout(string token);
    }
}
