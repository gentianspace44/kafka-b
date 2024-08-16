using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VPS.ControlCenter.Logic.IServices;
using VPS.ControlCenter.Logic.Models;

namespace VPS.ControlCenter.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            try
            {
                return Ok(await _authenticationService.Login(loginDto));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
      
        [HttpPut("refreshToken")]
        public async Task<IActionResult> RefreshToken(TokenModel tokenModel)
        {
            try
            {
                return Ok(await _authenticationService.RefreshToken(tokenModel.Token));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("logout")]
        public async Task<IActionResult> Logout(TokenModel tokenModel)
        {
            try
            {
                return Ok(await _authenticationService.Logout(tokenModel.Token));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
