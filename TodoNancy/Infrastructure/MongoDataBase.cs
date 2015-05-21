﻿using System.Collections.Generic;
using System.Linq;
using TodoNancy.Abstract;
using TodoNancy.Model;
using TodoNancy.NancyModules;

namespace TodoNancy.Infrastructure
{
    public class MongoDataBase : IDataStore
    {
        private string _dbConn;

        public MongoDataBase(string conn)
        {
            _dbConn = conn;
        }

        public void Drop()
        {
            TodosModule.Store.Clear();
        }

        public IEnumerable<Todo> GetAll()
        {
            return TodosModule.Store.Values.ToArray();
        }

        public long Count
        {
            get { return TodosModule.Store.Count; }
        }

        public bool TryAdd(Todo todo)
        {
            if (TodosModule.Store.ContainsKey(todo.Id))
            {
                return false;
            }
            TodosModule.Store.Add(todo.Id, todo);
            return true;
        }

        public bool TryRemove(int id)
        {
            return TodosModule.Store.ContainsKey(id) 
                && TodosModule.Store.Remove(id);
        }

        public bool TryUpdate(Todo todo)
        {
            if (!TodosModule.Store.ContainsKey(todo.Id))
            {
                return false;
            }
            TodosModule.Store.Remove(todo.Id);
            TodosModule.Store.Add(todo.Id, todo);
            return true;
        }
    }
}