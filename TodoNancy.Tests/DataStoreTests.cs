using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nancy.Testing;
using TodoNancy.Abstract;
using TodoNancy.Model;
using TodoNancy.NancyModules;

namespace TodoNancy.Tests
{
    [TestClass]
    public class DataStoreTests
    {
        private readonly IDataStore _fakeDataStore;
        private readonly Browser _sut;
        private readonly Todo _aTodo;

        public DataStoreTests()
        {
            //TODO: Install-Package FakeItEasy -project TodoNancy.Tests
            _fakeDataStore = A.Fake<IDataStore>();
            _sut = new Browser(with =>
            {
                with.Dependency(_fakeDataStore);
                with.Module<TodosModule>();
            });
            _aTodo = new Todo()
            {
                Id = 5,
                Completed = true,
                Order = 100,
                Title = "Task 10"
            };
        }

        [TestMethod]
        public void Should_Store_Posted_Todods_In_DataStore()
        {
            //Arrange
            //Act
            _sut.Post("/todos/", with => with.JsonBody(_aTodo));
            //Assert
            AssertCalledTryAddOnDataStoreWith(_aTodo);
        }

        private void AssertCalledTryAddOnDataStoreWith(Todo expected)
        {
            A.CallTo(() =>
                _fakeDataStore.TryAdd(A<Todo>
                    .That.Matches(actual => AssertAreSame(expected, actual)
                )))
                .MustHaveHappened();
        }

        private static bool AssertAreSame(Todo expected, Todo actual)
        {
            Assert.AreEqual(expected.Title, actual.Title);
            Assert.AreEqual(expected.Order, actual.Order);
            Assert.AreEqual(expected.Completed, actual.Completed);
            return true;
        }
    }
}
