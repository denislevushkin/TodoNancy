using System.Configuration;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.TinyIoc;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using TodoNancy.Abstract;
using TodoNancy.Model;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Providers;

namespace TodoNancy.Infrastructure
{
    //TODO: NServiceBus.CastleWindsor to be installed

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private readonly Logger _log = LogManager.GetLogger("RequestLogger");
        private const string TwitterConsumerKey = "qpRLKRr8kHwtsxZItaLbbw";
        private const string TwitterConsumerSecret = "8RXsfD8ixUpLI7DeStaRSv2zUpYOWORq9uYC9O9ViCY";

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            SetCurrentUserWhenLoggedIn(pipelines);
            SimpleConfigurator.ConfigureForTargetLogging(new AsyncTargetWrapper(new EventLogTarget()));

            LogAllRequests(pipelines);
            LogAllResponseCodes(pipelines);
            LogUnhandledExceptions(pipelines);
        }

        private static void SetCurrentUserWhenLoggedIn(IPipelines pipelines)
        {
            pipelines.BeforeRequest += context =>
            {
                context.CurrentUser = context.Request.Cookies.ContainsKey("todoUser") 
                    ? new TokenService().GetUserFromToken(context.Request.Cookies["todoUser"])
                    : User.Anonymous;
                return null;
            };
        }

        private void LogAllRequests(IPipelines pipelines)
        {
            pipelines.BeforeRequest += ctx =>
            {
                _log.Info("Handling request {0} \"{1}\"", ctx.Request.Method, ctx.Request.Path);
                return null;
            };
        }

        private void LogAllResponseCodes(IPipelines pipelines)
        {
            pipelines.AfterRequest += ctx =>
              _log.Info("Responding {0} to {1} \"{2}\"", ctx.Response.StatusCode, ctx.Request.Method, ctx.Request.Path);
        }

        private void LogUnhandledExceptions(IPipelines pipelines)
        {
            pipelines.OnError.AddItemToStartOfPipeline((ctx, err) =>
            {
                _log.ErrorException(string.Format("Request {0} \"{1}\" failed", ctx.Request.Method, ctx.Request.Path), err);
                //TODO: write also log into the file on hard disk
                return null;
            });
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            var connectionString = ConfigurationManager.AppSettings.Get("MONGOLAB_URI");
            var mongoDataStore = new MongoDataBase(connectionString);
            container.Register<IDataStore>(mongoDataStore);

            var authenticationService = new AuthenticationService();
            var twitterProvider = new TwitterProvider(new ProviderParams() { Key = TwitterConsumerKey, Secret = TwitterConsumerSecret });
            authenticationService.AddProvider(twitterProvider);
            container.Register<IAuthenticationService>(authenticationService);
        }
        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("/docs", "Docs")
                );
        }
    }
}