using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VPS.ControlCenter.Api.Extensions;
using VPS.ControlCenter.Logic.IServices;
using VPS.ControlCenter.Logic.Models;

namespace VPS.ControlCenter.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
  // 9 = super admin, check HollyTopUp.dbo.UserType for more roles
    public class AlertsController : ControllerBase
    {
        private readonly IAlertService _alertService;

        public AlertsController(IAlertService alertService)
        {
            _alertService = alertService;
        }

        [HttpGet("GetList")]
        public async Task<IResult> Get()
        {
            return Results.Ok(await _alertService.GetAlerts());
        }

        [CustomAuthorize(9)]
        [HttpPost("UpdateList")]
        public IResult CreateOrUpdate([FromBody]List<AlertModel> model)
        {
            _alertService.CreateOrUpdateAlert(model);
            return Results.Ok();
        }
    }
}
