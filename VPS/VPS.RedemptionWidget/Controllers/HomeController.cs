using Microsoft.AspNetCore.Mvc;
using VPS.Domain.Models.VRW.Voucher;
using VPS.Services.VRW.Interface;

namespace VPS.RedemptionWidget.Controllers
{
    public class HomeController : Controller
    {
        private readonly IVoucherRedemptionWidgetService _voucherRedemptionWidgetService;

        public HomeController(IVoucherRedemptionWidgetService service)
        {
            _voucherRedemptionWidgetService = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string ClientId, string DevicePlatform)
        {
            VrwViewModel model = await _voucherRedemptionWidgetService.InitializeViewModel(ClientId, DevicePlatform);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> RedeemVoucher(VrwViewModel model)
        {
            VrwViewModel models = await _voucherRedemptionWidgetService.RedeemVoucher(model);
            return Json(models);
        }
    }
}
