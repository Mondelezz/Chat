using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Quantum.Models.DTO;
using Quantum.UserP.Models;

namespace Quantum.Mapping
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            CreateMap<RegistrationUserDTO, User>().ReverseMap();
            CreateMap<User, UserInfoOutput>().ReverseMap();
            CreateMap<User, UserOpenData>().ReverseMap();
        }
    }
}
