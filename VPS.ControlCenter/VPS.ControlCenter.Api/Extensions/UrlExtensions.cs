using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using VPS.ControlCenter.Api.Hubs;
using VPS.ControlCenter.Logic.Helpers;
using VPS.ControlCenter.Logic.IServices;
using VPS.ControlCenter.Logic.Models;
using VPS.ControlCenter.Logic.SignalrServices;

namespace VPS.ControlCenter.Api.Extensions
{
    public static class UrlExtensions
    {
       
        public static void ConfigureEndpoints(this WebApplication app, ILogger<NotificationsHub> logger)
        {

            app.MapGet("/getProviders", async (IVoucherProviderService svc) =>
            {
                logger.LogInformation("Get All Providers");
                var items = await svc.GetAll();
                return Results.Ok(items);
            });

            app.MapPost("/CreateProvider", async (IVoucherProviderService svc, VoucherProviderModel model) =>
            {
                logger.LogInformation("CreateProvider. Provider: {provider}",  JsonConvert.SerializeObject(model));
                var newId = await svc.CreateVoucherProvider(model);
                return Results.Ok(newId);
            });

            app.MapPost("/VerifyRedemptionStatus", async (IVoucherProviderService svc, RedemptionStatusRequest redemptionStatusRequest) =>
            {
                logger.LogInformation("Verify Voucher Redemption Status. Request Object: {redemptionStatusRequest}", JsonConvert.SerializeObject(redemptionStatusRequest));
                var statusId = await svc.VerifyRedemptionStatus(redemptionStatusRequest);
                return Results.Ok(statusId);
            });

            app.MapPut("/UpdateProvider", [CustomAuthorize(9)] async (IVoucherProviderService svc, VoucherProviderModel model) =>
            {
                logger.LogInformation("UpdateProvider. Provider: {provider}", JsonConvert.SerializeObject(model));
                var newId = await svc.UpdateVoucherProvider(model);
                return Results.Ok(newId);
            });

            app.MapPut("/SingleProviderUpdate", [CustomAuthorize(9)] async (IHubContext<DynamicSettingsHub> hubCtx, IVoucherProviderService svc, VoucherProviderModel model) =>
            {
                logger.LogInformation("SingleProviderUpdate. Provider: {provider}", JsonConvert.SerializeObject(model) );
                var success = await svc.UpdateVoucherProvider(model);
                if (success)
                {
                    await hubCtx.Clients.All.SendAsync("ClearCache", "UpdateVoucherProvider");
                }
                return Results.Ok();
            });

            app.MapPost("/BulkProviderUpdate", async (IHubContext<DynamicSettingsHub> hubCtx, IVoucherProviderService svc, [FromBody] VoucherProviderList modelList) =>
            {
                logger.LogInformation("BulkProviderUpdate. VoucherProviderList: {modelList}", JsonConvert.SerializeObject(modelList));
                var success = await svc.UpdateMultipleVoucherProviders(modelList.VoucherProviders);
                if (success)
                {
                    await hubCtx.Clients.All.SendAsync("ClearCache", "UpdateMultipleVoucherProviders");
                }
                return Results.Ok();
            });

            app.MapPost("/Notify", async (IHubContext<NotificationsHub> hubCtx, SignalRHelperService signalRHelperService, [FromBody] NotifyModel model) =>
            {
                var connectionId = await signalRHelperService.GetUserConnectionId(model.ClientId);
                logger.LogInformation("Notify. Model: {model}, connection Id: {connectionId}", JsonConvert.SerializeObject(model), connectionId);
                if (!string.IsNullOrWhiteSpace(connectionId))
                {
                    await hubCtx.Clients.Client(connectionId).SendAsync("Notify", model.Message);
                }
                else
                {
                    logger.LogInformation("Notify Failed. Punter Id: {punterid}", model.ClientId );
                }
                return Results.Ok();
            });

            app.MapPost("/NotifyAll", async (IHubContext<NotificationsHub> hubCtx, SignalRHelperService signalRHelperService, [FromBody] NotifyModel model) =>
            {
                logger.LogInformation("NotifyAll. Model: {model}", JsonConvert.SerializeObject(model));
                await hubCtx.Clients.All.SendAsync("Notify", model.Message);
                return Results.Ok();
            });

            app.MapGet("/getSyxToken", () =>
            {
                var returnedToken = SyxSessionHelper.GetSyxSession();               
                logger.LogInformation("Get token. {token}", JsonConvert.SerializeObject(returnedToken));
                return Results.Ok(returnedToken);
            });

            app.MapGet("/forceSyxTokenUpdate", async () =>
            {
                logger.LogInformation("Force Syx Token Refresh");
                return Results.Ok(await SyxSessionHelper.ForceRefresh());
            });

            app.MapGet("/forceRedisVoucherUpdate", async (IVoucherProviderService voucherProviderService) =>
            {
                logger.LogInformation("Force Redis Refresh");
                var providers = await voucherProviderService.SetOrUpdateRedis();
                return Results.Ok(providers);
            });
        }
    }
}
