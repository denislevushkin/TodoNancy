using Nancy;

namespace TodoNancy.NancyModules
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = _ => HttpStatusCode.OK;
        }
    }
}