using System;
using System.Collections.Generic;
using Nancy.Responses.Negotiation;
//using Nancy;

namespace TodoNancy.Protobuf
{
    public class ProtoBufProcessor : IResponseProcessor
    {
        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, Nancy.NancyContext context)
        {

            if (requestedMediaRange.Matches(new MediaRange(Constants.ProtoBufContentType)))
            {
                return new ProcessorMatch
                {
                    ModelResult = MatchResult.DontCare,
                    RequestedContentTypeResult = MatchResult.ExactMatch
                };
            }
            if (requestedMediaRange.Subtype.ToString().EndsWith("protobuf"))
            {
                return new ProcessorMatch
                {
                    ModelResult = MatchResult.DontCare,
                    RequestedContentTypeResult = MatchResult.NonExactMatch
                };
            }
            return new ProcessorMatch
            {
                ModelResult = MatchResult.DontCare,
                RequestedContentTypeResult = MatchResult.NoMatch
            };
        }

        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get
            {
                return new[] { new Tuple<string, MediaRange>(
                ".protobuf", new MediaRange(Constants.ProtoBufContentType)) };
            }
        }

        public Nancy.Response Process(MediaRange requestedMediaRange, dynamic model, Nancy.NancyContext context)
        {
            return new Nancy.Response
            {
                Contents = stream => ProtoBuf.Serializer.Serialize(stream, model),
                ContentType = Constants.ProtoBufContentType
            };
        }
    }


    //public class ProtoBufProcessor : IResponseProcessor
    //{
    //    public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
    //    {
    //        get
    //        {
    //            return new[] { Tuple.Create("protobuf", MediaRange.FromString(Constants.ProtoBufContentType)) };
    //        }
    //    }

    //    public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
    //    {
    //        if (IsWildcard(requestedMediaRange) || requestedMediaRange.Matches(Constants.ProtoBufContentType))
    //        {
    //            return new ProcessorMatch
    //            {
    //                ModelResult = MatchResult.DontCare,
    //                RequestedContentTypeResult = MatchResult.ExactMatch
    //            };
    //        }

    //        return new ProcessorMatch
    //        {
    //            ModelResult = MatchResult.DontCare,
    //            RequestedContentTypeResult = MatchResult.NoMatch
    //        };
    //    }

    //    public Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
    //    {
    //        return new ProtoBufResponse(model);
    //    }

    //    private static bool IsWildcard(MediaRange requestedMediaRange)
    //    {
    //        return requestedMediaRange.Type.IsWildcard && requestedMediaRange.Subtype.IsWildcard;
    //    }
    //}
}