using Nancy.Security;
using TodoNancy.Model;

namespace TodoNancy.Infrastructure
{
    public class TokenService
    {
        public string GetToken(string userName)
        {
            return userName;
        }

        public IUserIdentity GetUserFromToken(string token)
        {
            return new User { UserName = token };
        }
    }
}