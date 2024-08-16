using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPS.ControlCenter.Logic.Models;

namespace VPS.ControlCenter.Logic.IServices
{
    public interface IAlertService
    {
        Task<List<AlertModel>> GetAlerts();
        void CreateOrUpdateAlert(List<AlertModel> alertModel);

    }
}
