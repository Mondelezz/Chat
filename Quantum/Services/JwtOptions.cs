using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Quantum.Services
{
    public class JwtOptions
    {
        public const string AUDIENCE = "MyClient";
        public const string ISSUER = "MyApp";
        public const string KEY = "bd1a1ccf8095037f361a4d351e7c0de65f0776bfc2f478ea8d312c763bb6caca";
        public const int KEYLIFE = 1000;
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
