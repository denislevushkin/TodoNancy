using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ConfigurationManager = System.Configuration.ConfigurationManager;
using Nancy;
using Nancy.Testing;
using ProtoBuf;
using ProtoBuf.Meta;
using TodoNancy.Infrastructure;
using TodoNancy.Model;
using TodoNancy.Protobuf;

namespace TodoNancy.Tests
{
    [TestClass]
    public class TodoModuleTests
    {
        private readonly Browser _sut;
        private readonly Todo _aTodo;
        private readonly Todo _anEditedTodo;

        public TodoModuleTests()
        {
            var connectionString = ConfigurationManager.AppSettings.Get("MONGOLAB_URI");
            var database = new MongoDataBase(connectionString);
            database.Drop();

            if (!RuntimeTypeModel.Default.CanSerializeContractType(typeof(Todo)))
            {
                RuntimeTypeModel.Default.Add(typeof(Todo), false).Add("Id", "Title", "Order", "Completed");
                RuntimeTypeModel.Default.CompileInPlace();
            }

            _sut = new Browser(new Bootstrapper());
            _aTodo = new Todo()
            {
                Title = "Task 1",
                Order = 0,
                Completed = false
            };
            _anEditedTodo = new Todo()
            {
                Id = 42,
                Title = "Edited name",
                Order = 0,
                Completed = false
            };
        }

        [TestMethod]
        public void Should_Return_Empty_Line_On_Get_When_No_Todos_Have_Been_Posted()
        {
            //Arrange
            //Act
            var actual = _sut.Get("/todos/", with => with.Accept("application/json"));
            //Assert
            Assert.AreEqual(HttpStatusCode.OK, actual.StatusCode);
            Assert.IsTrue(actual.Body.DeserializeJson<Todo[]>().Length == 0);
        }

        [TestMethod]
        public void Should_Return_201_Create_When_Todos_Is_Posted()
        {
            //Arrange
            //Act
            var actual = _sut.Post("/todos/", with =>
            {
                with.JsonBody(_aTodo);
                with.Accept("application/json");
            });
            //Assert
            Assert.AreEqual(HttpStatusCode.Created, actual.StatusCode);
        }

        [TestMethod]
        public void Should_Not_Accept_Posting_With_Duplicated_Id()
        {
            //Arrange
            //Act
            var actual = _sut.Post("/todos/", with => with.JsonBody(_anEditedTodo))
                .Then
                .Post("/todos/", with => with.JsonBody(_anEditedTodo));
            //Assert
            Assert.AreEqual(HttpStatusCode.NotAcceptable, actual.StatusCode);
        }

        [TestMethod]
        public void Should_Be_Able_To_Get_Posted_Todo()
        {
            //Arrange
            //Act
            var actual = _sut.Post("/todos/", with => with.JsonBody(_aTodo))
                .Then
                .Get("/todos/", with => with.Accept("application/json"));
            var actualBody = actual.Body.DeserializeJson<Todo[]>();
            //Assert
            Assert.AreEqual(1, actualBody.Length);
            AssertAreSame(_aTodo, actualBody[0]);
        }

        [TestMethod]
        public void Should_Be_Able_To_Edit_Todo_With_Put()
        {
            //Arrange
            //Act
            var actual = _sut.Post("/todos/", with => with.JsonBody(_aTodo))
                .Then
                .Put("/todos/1", with => with.JsonBody(_anEditedTodo))
                .Then
                .Get("/todos/", with => with.Accept("application/json"));
            var actualBody = actual.Body.DeserializeJson<Todo[]>();
            //Assert
            Assert.AreEqual(1, actualBody.Length);
            AssertAreSame(_anEditedTodo, actualBody[0]);
        }

        [TestMethod]
        public void Should_Be_Able_To_Delete_Todo_With_Delete()
        {
            //Arrange
            //Act
            var actual = _sut.Post("/todos/", with => with.Body(_aTodo.ToJson()))
                .Then
                .Delete("/todos/1")
                .Then
                .Get("/todos/", with => with.Accept("application/json"));
            //Assert
            Assert.AreEqual(HttpStatusCode.OK, actual.StatusCode);
            Assert.IsTrue(actual.Body.DeserializeJson<Todo[]>().Length == 0);
        }

        private static void AssertAreSame(Todo expected, Todo actual)
        {
            Assert.AreEqual(expected.Title, actual.Title);
            Assert.AreEqual(expected.Order, actual.Order);
            Assert.AreEqual(expected.Completed, actual.Completed);
        }

        [TestMethod]
        public void Should_Be_Able_To_Get_Posted_Xml_Todo()
        {
            //Arrange
            //Act
            var actual = _sut.Post("/todos/", with =>
            {
                with.XMLBody(_aTodo);
                with.Accept("application/xml");
            })
            .Then.Get("/todos/", with => with.Accept("application/json"));
            var actualBody = actual.Body.DeserializeJson<Todo[]>();
            //Assert
            Assert.AreEqual(1, actualBody.Length);
            AssertAreSame(_aTodo, actualBody[0]);
        }

        [TestMethod]
        public void Should_Be_Able_To_Get_Posted_Todo_As_Xml()
        {
            //Arrange
            //Act
            var actual = _sut.Post("/todos/", with =>
            {
                with.JsonBody(_aTodo);
                with.Accept("application/xml");
            })
            .Then.Get("/todos/", with => with.Accept("application/xml"));
            var actualBody = actual.Body.DeserializeXml<Todo[]>();
            //Assert
            Assert.AreEqual(1, actualBody.Length);
            AssertAreSame(_aTodo, actualBody[0]);
        }

        [TestMethod]
        public void Should_Be_Able_To_Get_Posted_Todo_As_Protobuf()
        {
            //Arrange
            //Act
            var actual = _sut.Post("/todos/", with =>
            {
                var stream = new MemoryStream();
                Serializer.Serialize(stream, _aTodo);
                with.Body(stream, Constants.ProtoBufContentType);
                with.Accept("application/xml");
            })
            .Then.Get("/todos/", with => with.Accept(Constants.ProtoBufContentType));
            var actualBody = Serializer.Deserialize<Todo[]>(actual.Body.AsStream());
            //Assert
            Assert.AreEqual(1, actualBody.Length);
            AssertAreSame(_aTodo, actualBody[0]);
        }

        [TestMethod]
        public void Should_Be_Able_To_Get_View_With_Posted_Todo()
        {
            //Arrange
            //Act
            var actual = _sut.Post("/todos/", with =>
            {
                with.JsonBody(_aTodo);
                with.Accept("application/json");
            })
            .Then.Get("/todos/", with => with.Accept("text/html"));
            //Assert
            actual.Body["title"].AllShouldContain("Todos");
            actual.Body["table[id='Table1'] tr[id='1'] td:first-child"]
                .ShouldExistOnce()
                .And
                .ShouldContain(_aTodo.Title);
        }

        [TestMethod]
        public void Should_return_view_with_a_form_on_get_when_accepting_html()
        {
            var actual = _sut.Get("/todos/", with => with.Accept("text/html"));

            actual.Body["form"].ShouldExistOnce();
            actual.Body["form input[type='text' name='Title']"].ShouldExistOnce();
            actual.Body["form input[type='number' name='Order']"].ShouldExistOnce();
            actual.Body["form input[type='checkbox' name='Completed']"].ShouldExistOnce();
        }

        [TestMethod]
        public void Should_Be_Able_To_Post_View_With_Posted_Todo()
        {
            //Arrange
            //Act
            var actual = _sut.Post("/todos/", with =>
            {
                with.JsonBody(_aTodo);
                with.Accept("text/html");
            });
            //Assert
            actual.Body["title"].AllShouldContain("Todo");
        }

        [TestMethod]
        public void Should_Give_Access_To_Overview_Documentation()
        {
            //Arrange
            //Act
            var actual = _sut.Get("/docs/overview.htm", with =>
                with.Accept("text/html"));
            //Assert
            Assert.AreEqual(HttpStatusCode.OK, actual.StatusCode);
        }
    }
}
