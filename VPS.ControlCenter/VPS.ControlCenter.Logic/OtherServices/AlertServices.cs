using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VPS.ControlCenter.Logic.EFServices;
using VPS.ControlCenter.Logic.IServices;
using VPS.ControlCenter.Logic.Models;
using VPS.ControlCenter.Logic.RedisServices;

namespace VPS.ControlCenter.Logic.OtherServices
{
    public class AlertServices: IAlertService
    {
        private IRedisRepository _redisRepository;
        private ILogger<EFVoucherProviderService> _logger;

        public AlertServices(IRedisRepository redis, ILogger<EFVoucherProviderService> logger)
        {
            _redisRepository = redis;
            _logger = logger;
        }

        public void CreateOrUpdateAlert(List<AlertModel> alertModel)
        {
            try
            {
                alertModel = alertModel.OrderBy(z => z.Order).ToList();
                _redisRepository.Add("VrwMessages", alertModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save alerts.");
            }
        }

        public async Task<List<AlertModel>> GetAlerts()
        {
            try
            {
                var redisData = await _redisRepository.Get("VrwMessages");
                return JsonConvert.DeserializeObject<List<AlertModel>>(redisData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get alerts.");
                return default(List<AlertModel>);
            }
        }
    }
}
