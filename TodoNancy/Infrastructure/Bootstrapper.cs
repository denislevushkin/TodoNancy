using System.Configuration;
using Nancy;
using Nancy.Conventions;
using Nancy.TinyIoc;
using TodoNancy.Abstract;

namespace TodoNancy.Infrastructure
{
    //TODO: NServiceBus.CastleWindsor to be installed

    public class Bootstrapper : DefaultNancyBootstrapper
    {
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
    }
}