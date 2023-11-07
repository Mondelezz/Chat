using AutoMapper;
using Quantum.Data;
using Quantum.Interfaces;
using Quantum.Models.DTO;

namespace Quantum.Services
{
    public class AuthorizationService : IAuthorization
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;
        public AuthorizationService(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        public string AuthenticateUser(AuthorizationUserDTO authorizationUserDTO)
        {
            throw new NotImplementedException();
        }
    }
}
