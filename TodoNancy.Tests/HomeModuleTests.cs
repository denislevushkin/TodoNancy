using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nancy;
using Nancy.Testing;
using TodoNancy.Infrastructure;

namespace TodoNancy.Tests
{
    [TestClass]
    public class HomeModuleTests
    {
        [TestMethod]
        public void Should_Get_Root_Rout()
        {
            //Arrange
            var sut = new Browser(new Bootstrapper());
            //Act
            var actual = sut.Get("/").Then.Get("/");
            actual.Then.Get("/");
            //Assert
            Assert.AreEqual(HttpStatusCode.OK, actual.StatusCode);
        }
    }
}
