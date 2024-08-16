using AutoMapper;
using VPS.ControlCenter.Core.Entities;

namespace VPS.ControlCenter.Logic.Models
{
    internal class VpsMappingProfile : Profile
    {
        public VpsMappingProfile()
        {
            CreateMap<VoucherProvider, VoucherProviderModel>().ReverseMap();
            CreateMap<VoucherType, VoucherTypeModel>().ReverseMap();
        }
    }
}
