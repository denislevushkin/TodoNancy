using System.Web.Script.Serialization;

namespace TodoNancy.Model
{
  using ProtoBuf;

  [ProtoContract]
    public class Todo
    {
    [ProtoMember(1)]
        public long Id { get; set; }
    [ProtoMember(2)]
        public string Title { get; set; }
    [ProtoMember(3)]
        public int Order { get; set; }
    [ProtoMember(4)]
        public bool Completed { get; set; }
        public string UserName { get; set; }

        public string ToJson()
        {
            return new JavaScriptSerializer().Serialize(this);
        }
    }
}