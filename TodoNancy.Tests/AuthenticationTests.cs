using System.Linq;
using Nancy;
using Nancy.Authentication.WorldDomination;
using Nancy.Security;
using TodoNancy.Infrastructure;
using Xunit;
using Nancy.Testing;
using WorldDomination.Web.Authentication;

namespace TodoNancy.Tests
{
    public class AuthenticationTests
    {
        [Fact]
        public void Should_set_user_identity_when_cookie_is_set()
        {
            const string expected = "chr_horsdal";
            var userNameToken = new TokenService().GetToken(expected);
            var sut = new Browser(new Bootstrapper());

            sut.Get("/testing", with => with.Cookie("todoUser", userNameToken));

            Assert.Equal(expected, TestingModule.ActualUser.UserName);
        }

        [Fact]
        public void Should_set_user_identity_to_anumynous_when_cookie_is_not_set()
        {
            var sut = new Browser(new Bootstrapper());

            sut.Get("/testing");

            Assert.Equal("anonymous", TestingModule.ActualUser.UserName);
        }

        [Fact]
        public void Should_redirect_to_root_on_social_authc_callback()
        {
            new Browser(new Bootstrapper()).Get("/testing");
            var callbackData = new AuthenticateCallbackData
            {
                AuthenticatedClient = new AuthenticatedClient("")
                {
                    UserInformation = new UserInformation { UserName = "chr_horsdal" }
                }
            };

            var sut = new SocialAuthenticationCallbackProvider();

            var actual = sut.Process(TestingModule.ActualModule, callbackData);

            Assert.Equal(HttpStatusCode.SeeOther, actual.StatusCode);
        }

        [Fact]
        public void Should_set_todo_user_cookie_on_social_authc_callback()
        {
            new Browser(new Bootstrapper()).Get("/testing");
            var userNameToken = new TokenService().GetToken("chr_horsdal");
            var callbackData = new AuthenticateCallbackData
            {
                AuthenticatedClient = new AuthenticatedClient("")
                {
                    UserInformation = new UserInformation { UserName = "chr_horsdal" }
                }
            };

            var sut = new SocialAuthenticationCallbackProvider();

            var actual = (Response)sut.Process(TestingModule.ActualModule, callbackData);

            Assert.Contains("todoUser", actual.Cookies.Select(cookie => cookie.Name));
            Assert.Contains(userNameToken, actual.Cookies.Select(cookie => cookie.Value));
        }
    }

    public class TestingModule : NancyModule
    {
        public static IUserIdentity ActualUser;
        public static TestingModule ActualModule;

        public TestingModule()
        {
            Get["/testing"] = _ =>
            {
                ActualUser = Context.CurrentUser;
                ActualModule = this;
                return HttpStatusCode.OK;
            };
        }
    }
}
