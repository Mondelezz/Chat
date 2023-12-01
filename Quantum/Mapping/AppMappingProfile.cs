using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Quantum.Models;
using Quantum.Models.DTO;

namespace Quantum.Mapping
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            CreateMap<RegistrationUserDTO, User>().ReverseMap();
            CreateMap<User, UserInfoOutput>().ReverseMap();
            CreateMap<User, UsersOpenData>().ReverseMap();
        }
    }
}
