//using System.Web.Script.Serialization;
using Nancy.Json;

namespace TodoNancy.Model
{
    public class Todo
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public bool Completed { get; set; }

        public string ToJson()
        {
            return new JavaScriptSerializer().Serialize(this);
        }
    }
}