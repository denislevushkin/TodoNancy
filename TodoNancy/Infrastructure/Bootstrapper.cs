using System.Configuration;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.TinyIoc;
using Nancy.ViewEngines.Razor;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using TodoNancy.Abstract;

namespace TodoNancy.Infrastructure
{
    //TODO: NServiceBus.CastleWindsor to be installed

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        // Very important line to initialize RazorViewEngine - !!!
        public static RazorViewEngine EnsureRazorIsLoaded;
        private Logger log = LogManager.GetLogger("RequestLogger");

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            SimpleConfigurator.ConfigureForTargetLogging(new AsyncTargetWrapper(new EventLogTarget()));

            LogAllRequests(pipelines);
            LogAllResponseCodes(pipelines);
            LogUnhandledExceptions(pipelines);
        }

        private void LogAllRequests(IPipelines pipelines)
        {
            pipelines.BeforeRequest += ctx =>
            {
                log.Info("Handling request {0} \"{1}\"", ctx.Request.Method, ctx.Request.Path);
                return null;
            };
        }

        private void LogAllResponseCodes(IPipelines pipelines)
        {
            pipelines.AfterRequest += ctx =>
              log.Info("Responding {0} to {1} \"{2}\"", ctx.Response.StatusCode, ctx.Request.Method, ctx.Request.Path);
        }

        private void LogUnhandledExceptions(IPipelines pipelines)
        {
            pipelines.OnError.AddItemToStartOfPipeline((ctx, err) =>
            {
                log.ErrorException(string.Format("Request {0} \"{1}\" failed", ctx.Request.Method, ctx.Request.Path), err);
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

        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("/docs", "Docs")
                );
        }

        protected override IRootPathProvider RootPathProvider
        {
            get
            {
                return new CustomRootPathProvider();
            }
        }
    }
}