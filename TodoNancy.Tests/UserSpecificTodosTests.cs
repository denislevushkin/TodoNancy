using System.Linq;
using FakeItEasy;
using Nancy;
using Nancy.Testing;
using TodoNancy.Abstract;
using TodoNancy.Model;
using TodoNancy.NancyModules;
using Xunit;

namespace TodoNancy.Tests
{
    public class UserSpecificTodosTests
  {
    private const string UserName = "Alice";
    private readonly Browser _sut;
    private readonly IDataStore _fakeDataStore;

    public UserSpecificTodosTests()
    {
      _fakeDataStore = A.Fake<IDataStore>();
      _sut = new Browser(with =>
      {
        with.Module<TodosModule>();
        with.ApplicationStartup((container, pipelines) =>
        {
          container.Register(_fakeDataStore);
          pipelines.BeforeRequest += ctx =>
          {
            ctx.CurrentUser = new User { UserName = UserName };
            return null;
          };
        });
      });
    }

    [Theory]
    [InlineData(0, 0, 0)] [InlineData(0, 10, 0)] [InlineData(0, 0, 10)] [InlineData(0, 10, 10)]
    [InlineData(1, 0, 0)] [InlineData(1, 10, 0)] [InlineData(1, 0, 10)] [InlineData(1, 10, 10)]
    [InlineData(42, 0, 0)] [InlineData(42, 10, 0)] [InlineData(42, 0, 10)] [InlineData(42, 10, 10)]
    public void Should_only_get_user_own_todos(int nofTodosForUser, int nofTodosForAnonynousUser, int nofTodosForOtherUser)
    {
      var todosForUser = Enumerable.Range(0, nofTodosForUser).Select(i => new Todo { Id = i, UserName =  UserName });
      var todosForAnonymousUser = Enumerable.Range(0, nofTodosForAnonynousUser).Select(i => new Todo { Id = i });
      var todosForOtherUser = Enumerable.Range(0, nofTodosForOtherUser).Select(i => new Todo { Id = i, UserName = "Bob" });

      A.CallTo(() => _fakeDataStore.GetAll())
       .Returns(todosForUser.Concat(todosForAnonymousUser).Concat(todosForOtherUser));

      var actual = _sut.Get("/todos/", with => with.Accept("application/json"));

      var actualBody = actual.Body.DeserializeJson<Todo[]>();
      Assert.Equal(nofTodosForUser, actualBody.Length);
    }

    [Fact]
    public void Should_store_posted_todo_for_user()
    {
      A.CallTo(() => _fakeDataStore.TryAdd(A<Todo>._)).Returns(true);
      var expected = new Todo { Id = 1001, UserName = UserName };

      var actual = _sut.Post("/todos/", with =>
      {
        with.JsonBody(expected);
        with.Accept("application/json");
      });

      Assert.Equal(HttpStatusCode.Created, actual.StatusCode);
      A.CallTo(() => 
        _fakeDataStore.TryAdd(
                        A<Todo>.That.Matches(actualTodo => 
                                             actualTodo.Id == expected.Id && 
                                             actualTodo.UserName == expected.UserName)))
        .MustHaveHappened();
    }

    [Fact]
    public void Should_should_use_current_user_when_trying_to_update_todo()
    {
      var todo = new Todo { Id = 5 };

      _sut.Put("/todos/5", with =>
      {
        with.JsonBody(new Todo { Id = todo.Id, UserName = todo.UserName, Title = "new titke"});
        with.Accept("application/json");
      });

      A.CallTo(() => _fakeDataStore.TryUpdate(A<Todo>._, UserName));
    }

    [Fact]
    public void Should_should_use_current_user_when_trying_to_delete_todo()
    {
      _sut.Delete("/todos/5");

      A.CallTo(() => _fakeDataStore.TryUpdate(A<Todo>._, UserName));
    }
  }
}