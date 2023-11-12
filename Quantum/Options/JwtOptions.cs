using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Quantum.Options
{
    public class JwtOptions
    {
        public const string AUDIENCE = "MyClient";
        public const string ISSUER = "MyApp";
        public const string KEY = "TEPERVCEBUDETNORTEPERVCEBUDETNORLOL";
        public const int KEYLIFE = 1000;
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
