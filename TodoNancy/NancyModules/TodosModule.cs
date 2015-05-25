using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;
//using Nancy.Security;
using Nancy.ViewEngines.Razor;
using TodoNancy.Abstract;
using TodoNancy.Model;

namespace TodoNancy.NancyModules
{
    public class TodosModule : NancyModule
    {
        public static Dictionary<long, Todo> Store = new Dictionary<long, Todo>();

        // Very important line to initialize RazorViewEngine - !!!
        public static RazorViewEngine EnsureRazorIsLoaded;

        public TodosModule(IDataStore todoStore)
            : base("todos")
        {
            //this.RequiresAuthentication();
            //this.RequiresHttps();
            //this.RequiresClaims(AlistOfClaims);

            Get["/"] = _ => Negotiate
                .WithModel(todoStore.GetAll().Where(todo => todo.UserName == Context.CurrentUser.UserName).ToArray())
                .WithView("Todos");
            //Get["/"] = _ => Response.AsJson(todoStore.GetAll());
            Post["/"] = _ =>
            {
                var newTodo = this.Bind<Todo>();
                newTodo.UserName = Context.CurrentUser.UserName;
                if (newTodo.Id == 0)
                {
                    newTodo.Id = todoStore.Count + 1;
                }
                if (!todoStore.TryAdd(newTodo))
                {
                    return HttpStatusCode.NotAcceptable;
                }
                return Negotiate.WithModel(newTodo)
                    .WithStatusCode(HttpStatusCode.Created)
                    .WithView("Created");
                //return Response.AsJson(newTodo)
                //    .WithStatusCode(HttpStatusCode.Created);
            };
            Put["/{id}"] = p =>
            {
                var updatedTodo = this.Bind<Todo>();
                updatedTodo.UserName = Context.CurrentUser.UserName;
                if (!todoStore.TryUpdate(updatedTodo, Context.CurrentUser.UserName))
                {
                    return HttpStatusCode.NotFound;
                }
                return updatedTodo;
            };
            Delete["/{id}"] = p =>
            {
                return !todoStore.TryRemove(p.id, Context.CurrentUser.UserName)
                    ? HttpStatusCode.NotFound
                    : HttpStatusCode.OK;
            };
        }
    }
}