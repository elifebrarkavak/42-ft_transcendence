using System;
using TeamFlow.Api.Dtos;
using TeamFlow.Api.Models;

namespace TeamFlow.Api.Services;

public class InMemoryTaskService : ITaskService
{
    private readonly List<TaskItem> _tasks = new()
    {
        new TaskItem { Id = 1, Title = "Login Page", CreatedAt = DateTime.UtcNow },
        new TaskItem { Id = 2, Title = "Database Schema", Status = "IN_PROGRESS", CreatedAt = DateTime.UtcNow },
    };

    private readonly Lock _lock = new();
    public List<TaskItem> GetAll()
    {
        lock(_lock)
        {
            return _tasks.ToList();
        }
    }
    public TaskItem? GetById(int id)
    {
        lock(_lock)
        {
            return _tasks.FirstOrDefault(t => t.Id == id);
        }
    }
    public TaskItem Create(CreateTaskRequest request)
    {
                lock (_lock)
        {
            var newId = _tasks.Count == 0 ? 1 : _tasks.Max(t => t.Id) + 1;

            var task = new TaskItem
            {
                Id = newId,
                Title = request.Title,
                CreatedAt = DateTime.UtcNow
            };

            _tasks.Add(task);
            return task;
        }
    }

    public TaskItem? Update(int id, UpdateTaskRequest request)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return null;

            task.Title = request.Title;
            task.Status = request.Status;
            return task;
        }
    }
    public bool Delete(int id)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return false;

            _tasks.Remove(task);
            return true;
        }
    }
}
