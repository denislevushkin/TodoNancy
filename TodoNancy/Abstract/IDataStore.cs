using System.Collections.Generic;
using TodoNancy.Model;

namespace TodoNancy.Abstract
{
    public interface IDataStore
    {
        IEnumerable<Todo> GetAll();
        long Count { get; }
        bool TryAdd(Todo todo);
        bool TryRemove(int id);
        bool TryUpdate(Todo todo);
    }
}