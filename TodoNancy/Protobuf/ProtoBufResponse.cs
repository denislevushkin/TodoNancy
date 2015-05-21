namespace TodoNancy.Protobuf
{
    using Nancy;
    using ProtoBuf;

    public class ProtoBufResponse : Response
    {
        public ProtoBufResponse(object body)
        {
            Contents = stream => Serializer.Serialize(stream, body);
            ContentType = Constants.ProtoBufContentType;
            StatusCode = HttpStatusCode.OK;
        }

        public ProtoBufResponse WithStatusCode(HttpStatusCode httpStatusCode)
        {
            StatusCode = httpStatusCode;
            return this;
        }
    }
}