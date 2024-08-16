using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VPS.ControlCenter.Core;
using VPS.ControlCenter.Core.Entities;
using VPS.ControlCenter.Core.HollyTopUp;
using VPS.ControlCenter.Logic.IServices;
using VPS.ControlCenter.Logic.Models;
using VPS.ControlCenter.Logic.RedisServices;

namespace VPS.ControlCenter.Logic.EFServices
{
    public class EFVoucherProviderService : IVoucherProviderService
    {
        private VpsDbContext _ctx;
        private HollyTopUpEntities _HtuContex;
        private IMapper _mapper;
        private IRedisRepository _redis;
        private ILogger<EFVoucherProviderService> _logger;

        public EFVoucherProviderService(VpsDbContext ctx, HollyTopUpEntities HtuCtx, IMapper mapper, IRedisRepository redisRepository, ILogger<EFVoucherProviderService> logger)
        {
            _ctx = ctx;
            _mapper = mapper;
            _redis = redisRepository;
            _logger = logger;
            _HtuContex = HtuCtx;
        }

        public async Task<int> CreateVoucherProvider(VoucherProviderModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            using (var scope = await _ctx.Database.BeginTransactionAsync())
            {
                try
                {
                    _ctx.VoucherProviders.Add(_mapper.Map<VoucherProvider>(model));
                    await _ctx.SaveChangesAsync();

                    await scope.CommitAsync(); // Commit the transaction
                    _logger.LogInformation("Provider created successfully. Provider ID: {providerid}", args: new object[] {model.VoucherProviderId});
                    return model.VoucherProviderId; // Return the ID of the newly created record
                }
                catch (Exception ex)
                {
                    await scope.RollbackAsync();
                    // Handle exceptions or log errors as needed
                    _logger.LogError(ex, "Failed to create provider");
                    return 0; // Indicate failure
                }
            }
        }

        public async Task<List<VoucherProviderModel>> GetAll()
        {
            var redisData = await _redis.Get("providers");
            var providers = JsonConvert.DeserializeObject<List<VoucherProviderModel>>(redisData);
         
            return providers;
        }

        private async Task<List<VoucherProviderModel>> GetFromSource()
        {
            var providers = await _ctx.VoucherProviders
                .Include(z => z.VoucherType).ToListAsync();
            return providers.Select(z => _mapper.Map<VoucherProviderModel>(z)).ToList();
        }

        public async Task<bool> UpdateVoucherProvider(VoucherProviderModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            using (var scope = await _ctx.Database.BeginTransactionAsync())
            {
                try
                {
                    var entity = await _ctx.VoucherProviders.FirstOrDefaultAsync(c => c.VoucherProviderId == model.VoucherProviderId);
                    _mapper.Map(model, entity);
                    await _ctx.SaveChangesAsync();

                    await scope.CommitAsync(); // Commit the transaction
                    await SetOrUpdateRedis();
                    _logger.LogInformation("Update Voucher Provider Successfully. Provider: {provider}", args: new object[] { JsonConvert.SerializeObject(model) });
                    return true; // Indicate success
                }
                catch (Exception ex)
                {
                    await scope.RollbackAsync();
                    // Handle exceptions or log errors as needed
                    _logger.LogError(ex, "Failed to Update Voucher Provider");
                    return false; // Indicate failure
                }
            }
        }

        public async Task<bool> UpdateMultipleVoucherProviders(List<VoucherProviderModel> models)
        {
            if (models == null || !models.Any())
            {
                throw new ArgumentNullException(nameof(models));
            }

            using (var scope = await _ctx.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var model in models)
                    {
                        var entity = await _ctx.VoucherProviders.FirstOrDefaultAsync(c => c.VoucherProviderId == model.VoucherProviderId);

                        if (entity != null)
                        {
                            _mapper.Map(model, entity);
                        }
                        else
                        {
                            // Handle the case where the entity is not found, maybe log a warning
                        }
                    }

                    await _ctx.SaveChangesAsync();

                    await scope.CommitAsync(); // Commit the transaction
                    await SetOrUpdateRedis();
                    _logger.LogInformation("Update Multiple Voucher Providers Successfully. Provider: {providers}", args: new object[] { JsonConvert.SerializeObject(models) });
                    return true; // Indicate success
                }
                catch (Exception ex)
                {
                    await scope.RollbackAsync();
                    // Handle exceptions or log errors as needed
                    _logger.LogError(ex, "Failed to Update Voucher Provider");
                    return false; // Indicate failure
                }
            }
        }

        public async Task<List<VoucherProviderModel>> SetOrUpdateRedis()
        {
            const string Key = "providers";
            await _redis.Delete(Key);
            var providers = await GetFromSource();
            _redis.Add(Key, providers);
            return providers;
        }

        public async Task<RedemptionStatusResponse> VerifyRedemptionStatus(RedemptionStatusRequest redemptionStatusRequest)
        {

            var providers = await GetAll();                     

            if (!redemptionStatusRequest.DefaultVoucherProvider)
            {
                var selectedProvider = providers.First(x => x.Name.ToLower() == redemptionStatusRequest.VoucherName.ToLower());
                if (selectedProvider != null)
                {
                    return await SwitchUpWithVoucherTypeId(redemptionStatusRequest, selectedProvider.VoucherTypeId);
                }
                else
                {
                    return new RedemptionStatusResponse
                    {
                        IsCreditedOnSyx = false,
                        Message = "Transaction process is still in progress."
                    };
                }
            }
            else
            {
                var selectedProvider = providers.Where(x => x.VoucherType.VoucherLength.Contains(redemptionStatusRequest.VoucherPin.Length.ToString()));
                if (selectedProvider.Any())
                {
                    foreach (var provider in selectedProvider)
                    {
                        var result = await SwitchUpWithVoucherTypeId(redemptionStatusRequest, provider.VoucherTypeId);
                        if (result.IsCreditedOnSyx)
                        {
                            return result;
                        }
                    }

                    return new RedemptionStatusResponse
                    {
                        IsCreditedOnSyx = false,
                        Message = "Transaction process is still in progress."
                    };
                }
                else
                {
                    return new RedemptionStatusResponse
                    {
                        IsCreditedOnSyx = false,
                        Message = "Transaction process is still in progress."
                    };
                }
            }
                       
        }

        private async Task<RedemptionStatusResponse> SwitchUpWithVoucherTypeId(RedemptionStatusRequest redemptionStatusRequest, int voucherTypeId)
        {

            switch (voucherTypeId)
            {
                case 1://Holly TopUp
                    var redeemedVoucherHTU = from x in _HtuContex.HTUVoucherLog
                                             where x.VoucherPin == redemptionStatusRequest.VoucherPin
                                             && x.ClientID == Convert.ToInt32(redemptionStatusRequest.ClientId)
                                             && x.CreditedOnSyx
                                             orderby x.VoucherLogId descending
                                             select new { x.CreditedOnSyx, x.Amount };

                    if (redeemedVoucherHTU.Any())
                    {
                        var result = await redeemedVoucherHTU.FirstOrDefaultAsync();
                        return new RedemptionStatusResponse
                        {
                            IsCreditedOnSyx = true,
                            Message = $"Voucher redeemed successfully for value of: R{result.Amount.ToString("0,0.00")}."
                        };
                    }
                    else
                    {
                        return new RedemptionStatusResponse
                        {
                            IsCreditedOnSyx = false,
                            Message = "Transaction process is still in progress."
                        };
                    }

                case 2://OTT
                    var redeemedVoucherOTT = from x in _HtuContex.OTTVoucherLog
                                             where x.VoucherPin == redemptionStatusRequest.VoucherPin
                                             && x.ClientID == Convert.ToInt32(redemptionStatusRequest.ClientId)
                                              && x.CreditedOnSyx
                                             orderby x.VoucherLogId descending
                                             select new { x.CreditedOnSyx, x.Amount };
                    if (redeemedVoucherOTT.Any())
                    {
                        var result = await redeemedVoucherOTT.FirstOrDefaultAsync();
                        return new RedemptionStatusResponse
                        {
                            IsCreditedOnSyx = true,
                            Message = $"Voucher redeemed successfully for value of: R{result.Amount.ToString("0,0.00")}."
                        };
                    }
                    else
                    {
                        return new RedemptionStatusResponse
                        {
                            IsCreditedOnSyx = false,
                            Message = "Transaction process is still in progress."
                        };
                    }

                case 3://Flash
                    var redeemedVoucherFlash = from x in _HtuContex.FlashVoucherLog
                                               where x.VoucherPin == redemptionStatusRequest.VoucherPin
                                               && x.ClientID == Convert.ToInt32(redemptionStatusRequest.ClientId)
                                                && x.CreditedOnSyx
                                               orderby x.VoucherLogId descending
                                               select new { x.CreditedOnSyx, x.Amount };
                    if (redeemedVoucherFlash.Any())
                    {
                        var result = await redeemedVoucherFlash.FirstOrDefaultAsync();
                        return new RedemptionStatusResponse
                        {
                            IsCreditedOnSyx = true,
                            Message = $"Voucher redeemed successfully for value of: R{result.Amount.ToString("0,0.00")}."
                        };
                    }
                    else
                    {
                        return new RedemptionStatusResponse
                        {
                            IsCreditedOnSyx = false,
                            Message = "Transaction process is still in progress."
                        };
                    }

                case 4:// BluVoucher
                    var redeemedVoucherBlu = from x in _HtuContex.BluVoucherLog
                                             where x.VoucherPin == redemptionStatusRequest.VoucherPin
                                             && x.ClientID == Convert.ToInt32(redemptionStatusRequest.ClientId)
                                              && x.CreditedOnSyx
                                             orderby x.VoucherLogId descending
                                             select new { x.CreditedOnSyx, x.Amount };
                    if (redeemedVoucherBlu.Any())
                    {
                        var result = await redeemedVoucherBlu.FirstOrDefaultAsync();
                        return new RedemptionStatusResponse
                        {
                            IsCreditedOnSyx = true,
                            Message = $"Voucher redeemed successfully for value of: R{result.Amount.ToString("0,0.00")}."
                        };
                    }
                    else
                    {
                        return new RedemptionStatusResponse
                        {
                            IsCreditedOnSyx = false,
                            Message = "Transaction process is still in progress."
                        };
                    }

                case 5: //Easy Load
                    var redeemedVoucherEasyLoad = from x in _HtuContex.EasyLoadVoucherLog
                                                  where x.VoucherPin == redemptionStatusRequest.VoucherPin
                                                  && x.ClientID == Convert.ToInt32(redemptionStatusRequest.ClientId)
                                                   && x.CreditedOnSyx
                                                  orderby x.VoucherLogId descending
                                                  select new { x.CreditedOnSyx, x.Amount };
                    if (redeemedVoucherEasyLoad.Any())
                    {
                        var result = await redeemedVoucherEasyLoad.FirstOrDefaultAsync();
                        return new RedemptionStatusResponse
                        {
                            IsCreditedOnSyx = true,
                            Message = $"Voucher redeemed successfully for value of: R{result.Amount.ToString("0,0.00")}."
                        };
                    }
                    else
                    {
                        return new RedemptionStatusResponse
                        {
                            IsCreditedOnSyx = false,
                            Message = "Transaction process is still in progress."
                        };
                    }

                case 6: //RA Cellular Voucher
                    var redeemedVoucherRACellular = from x in _HtuContex.RAVoucherLog
                                                    where x.VoucherPin == redemptionStatusRequest.VoucherPin
                                                    && x.ClientID == Convert.ToInt32(redemptionStatusRequest.ClientId)
                                                    orderby x.VoucherLogId descending
                                                    select new { x.CreditedOnSyx, x.Amount };

                    if (redeemedVoucherRACellular.Any())
                    {
                        var result = await redeemedVoucherRACellular.FirstOrDefaultAsync();
                        return new RedemptionStatusResponse
                        {
                            IsCreditedOnSyx = true,
                            Message = $"Voucher redeemed successfully for value of: R{result.Amount.ToString("0,0.00")}."
                        };
                    }
                    else
                    {
                        return new RedemptionStatusResponse
                        {
                            IsCreditedOnSyx = false,
                            Message = "Transaction process is still in progress."
                        };
                    }
                default:
                    return new RedemptionStatusResponse
                    {
                        IsCreditedOnSyx = false,
                        Message = "Transaction process is still in progress."
                    };
            }
        }


    
    }
}
