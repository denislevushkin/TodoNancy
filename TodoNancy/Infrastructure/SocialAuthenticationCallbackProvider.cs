using Nancy;
using Nancy.Authentication.WorldDomination;
using Nancy.Cookies;
using TodoNancy.Model;

namespace TodoNancy.Infrastructure
{
    public class SocialAuthenticationCallbackProvider : IAuthenticationCallbackProvider
    {
        public dynamic Process(NancyModule module, AuthenticateCallbackData callbackData)
        {
            module.Context.CurrentUser = new User
            {
                UserName = callbackData.AuthenticatedClient.UserInformation.UserName
            };
            return module.Response
              .AsRedirect("/")
              .WithCookie(new NancyCookie("todoUser", new TokenService().GetToken(module.Context.CurrentUser.UserName)));
        }

        public dynamic OnRedirectToAuthenticationProviderError(NancyModule nancyModule, string errorMessage)
        {
            return "login failed: " + errorMessage;
        }
    }
}