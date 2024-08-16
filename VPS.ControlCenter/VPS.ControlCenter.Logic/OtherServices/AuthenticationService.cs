using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VPS.ControlCenter.Core.HollyTopUp;
using VPS.ControlCenter.Logic.Helpers;
using VPS.ControlCenter.Logic.IServices;
using VPS.ControlCenter.Logic.Models;

namespace VPS.ControlCenter.Logic.OtherServices
{
    public class AuthenticationService : IAuthenticationService
    {

        private readonly HollyTopUpEntities _db;
        private readonly JwtSettings _jwtSettings;
        public AuthenticationService(HollyTopUpEntities db, IOptions<JwtSettings> jwtSettings)
        {
            _db = db;
            _jwtSettings = jwtSettings.Value;
        }
        public async Task<SecurityTokenDto> Login(LoginDto loginDto)
        {
            var user = await GetUserByUserName(loginDto.Username);
            string hash = GenerateHash(loginDto.Username, loginDto.Password);
            if (user != null && user.Password == hash)
            {
                var token = GenerateAccessTokenAsync(user, _jwtSettings.TokenLifetime);
                var refreshToken = GenerateAccessTokenAsync(user, _jwtSettings.RefreshTokenLifetime);

                return new SecurityTokenDto(token, refreshToken);
            }

            return null;
        }

        public Task<bool> Logout(string token)
        {
            //TODO: Implement this at a later stage
            return default;
        }

        public Task<SecurityTokenDto> RefreshToken(string token)
        {
           //TODO: Implement this at a later stage
           return default;
        }


        private string GenerateHash(string username, string password)
        {
            return HashUtilityHelper.HashCode(username + "|HollyTopUp|" + password);
        }
        private async Task<User> GetUserByUserName(string userName)
        {
            return await _db.User.FirstOrDefaultAsync(usr => usr.UserName == userName);
        }
        public async Task SaveAuditTrail(int userId, String controller, String action, String type, String ipAddress)
        {
            var auditTrail = new AuditTrail
            {
                AuditTrailDateTime = DateTime.Now,
                UserID = userId,
                Controller = controller,
                Action = action,
                Type = type,
                IPAddress = ipAddress
            };

            await _db.AuditTrail.AddAsync(auditTrail);
            await _db.SaveChangesAsync();
        }

        private TokenModel GenerateAccessTokenAsync(User user, int tokenLifetime)
        {
            var claims = GetValidClaimsAsync(user);

            var expiryTime = DateTime.Now.AddMinutes(tokenLifetime);
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiryTime,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                    SecurityAlgorithms.HmacSha256
                )
            );

            return new TokenModel(new JwtSecurityTokenHandler().WriteToken(token), expiryTime);
        }
        private Claim[] GetValidClaimsAsync(User user)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new Claim("UserTypeId", user.UserTypeID.ToString())
            // Add additional claims based on your requirements
        };


            return claims.ToArray();
        }
    }
}
