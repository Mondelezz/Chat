using AutoMapper;
using Quantum.Models;
using Quantum.Models.DTO;

namespace Quantum.Mapping
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            CreateMap<UserDTO, User>();
        }
    }
}
