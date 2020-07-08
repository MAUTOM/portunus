using AutoMapper;
using MAKeys.Entities.DataTransferObjects;
using MAKeys.Entities.Models;

namespace MAKeysd
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PublicKey, PublicKeyDto>();
            CreateMap<PublicKeyForCreationDto, PublicKey>();
        }
    }
}