using System;
using TeamFlow.Api.Dtos;
using TeamFlow.Api.Models;

namespace TeamFlow.Api.Services;

public interface ITaskService
{
    List<TaskItem> GetAll();
    TaskItem? GetById(int id);
    TaskItem Create(CreateTaskRequest request);
    TaskItem? Update(int id, UpdateTaskRequest request);
    bool Delete(int id);
}
