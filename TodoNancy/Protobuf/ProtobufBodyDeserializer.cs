//using System;
//using System.IO;
//using System.Linq;
//using ProtoBuf.Meta;
using Nancy.ModelBinding;

namespace TodoNancy.Protobuf
{
    public class ProtobufBodyDeserializer : IBodyDeserializer
    {
        public bool CanDeserialize(string contentType, BindingContext context)
        {
            return contentType == Constants.ProtoBufContentType;
        }

        public object Deserialize(string contentType, System.IO.Stream bodyStream, BindingContext context)
        {
            return ProtoBuf.Serializer.NonGeneric.Deserialize(context.DestinationType, bodyStream);
        }
    }

    //public sealed class ProtobufNetBodyDeserializer : IBodyDeserializer
    //{
    //    public bool CanDeserialize(string contentType, BindingContext context)
    //    {
    //        return IsProtoBufType(contentType);
    //    }

    //    public object Deserialize(string contentType, Stream bodyStream, BindingContext context)
    //    {
    //        // deserialize the body stream into the destination type
    //        return RuntimeTypeModel.Default.Deserialize(bodyStream, null, context.DestinationType);
    //    }

    //    private static bool IsProtoBufType(string contentType)
    //    {
    //        if (string.IsNullOrWhiteSpace(contentType))
    //        {
    //            return false;
    //        }

    //        var contentMimeType = contentType.Split(';').First();

    //        return contentMimeType.Equals(
    //            Constants.ProtoBufContentType, StringComparison.InvariantCultureIgnoreCase);
    //    }
    //}
}