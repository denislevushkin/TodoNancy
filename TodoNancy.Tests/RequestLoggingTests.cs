using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using Nancy;
using Nancy.Testing;
using TodoNancy.Infrastructure;
using Xunit;

namespace TodoNancy.Tests
{
    public class RequestLoggingTests
    {
        private readonly Browser _sut;
        private readonly MemoryTarget _actualLog;
        private const string InfoRequestlogger = "|INFO|RequestLogger";
        private const string ErrorRequestlogger = "|ERROR|RequestLogger";

        public RequestLoggingTests()
        {
            _sut = new Browser(new Bootstrapper());
            _actualLog = new MemoryTarget
            {
                Layout = NLog.Layouts.Layout.FromString(
                    "${longdate}|${level:uppercase=true}|${logger}|${message}|${exception}")
            };
            SimpleConfigurator.ConfigureForTargetLogging(_actualLog, LogLevel.Info);
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/todos/")]
        [InlineData("/shouldnotbefound/")]
        public void ShouldLogIncomingRequests(string path)
        {
            _sut.Get(path);
            Assert.True(TryFindExptedInfoLog(_actualLog, "Handling request GET \"" + path + "\""));
        }

        [Theory]
        [InlineData("/", HttpStatusCode.OK)]
        [InlineData("/todos/", HttpStatusCode.OK)]
        [InlineData("/shouldnotbefound/", HttpStatusCode.NotFound)]
        public void ShouldLogStatusCodeOffResponses(string path, HttpStatusCode expectedStatusCode)
        {
            _sut.Get(path);

            Assert.True(TryFindExptedInfoLog(_actualLog, "Responding " + expectedStatusCode + " to GET \"" + path + "\""));
        }


        [Fact]
        public void ShouldLogMethod_post()
        {
            _sut.Post("/");

            Assert.True(TryFindExptedInfoLog(_actualLog, "POST"));
        }

        [Fact]
        public void ShouldLogMethod_put()
        {
            _sut.Put("/");

            Assert.True(TryFindExptedInfoLog(_actualLog, "PUT"));
        }

        [Fact]
        public void ShouldLogMethod_delete()
        {
            _sut.Delete("/");

            Assert.True(TryFindExptedInfoLog(_actualLog, "DELETE"));
        }

        [Fact]
        public void ShouldLogErrorOnFailingRequest()
        {
            try
            {
                _sut.Delete("/todos/illegal_item_id");
            }
            catch
            {
                // ignored
            }
            finally
            {
                Assert.True(TryFindExptedErrorLog(_actualLog, "Input string was not in a correct format."));
            }
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/todos/")]
        public void ShouldNotLogErrorOnSuccessfulReqeust(string path)
        {
            _sut.Get(path);

            Assert.False(TryFindExptedErrorLog(_actualLog, ""));
        }

        private static bool TryFindExptedInfoLog(MemoryTarget actualLog, string expected)
        {
            return TryFindExptedLogAtExpectedLevel(actualLog, expected, InfoRequestlogger);
        }

        private static bool TryFindExptedErrorLog(MemoryTarget actualLog, string expected)
        {
            return TryFindExptedLogAtExpectedLevel(actualLog, expected, ErrorRequestlogger);
        }

        private static bool TryFindExptedLogAtExpectedLevel(MemoryTarget actualLog, string expected, string requestloggerLevel)
        {
            var tryFindExptedLog =
              actualLog.Logs
                .Where(s => s.Contains(requestloggerLevel))
                .FirstOrDefault(s => s.Contains(expected));
            if (tryFindExptedLog != null)
                return true;
            //Console.WriteLine("\"{0}\" not found in log filtered by \"{1}\":", expected, requestloggerLevel);
            //Console.WriteLine(actualLog.Logs.Aggregate("[\n\t{0}\n]", (acc, s1) => string.Format(acc, s1 + "\n\t{0}")));
            return false;
        }
    }
}
