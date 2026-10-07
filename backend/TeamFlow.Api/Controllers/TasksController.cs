using Microsoft.AspNetCore.Mvc;
using TeamFlow.Api.Dtos;
using TeamFlow.Api.Services;

namespace TeamFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_taskService.GetAll());
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var task = _taskService.GetById(id);
        if (task == null)
            return NotFound();

        return Ok(task);
    }

    [HttpPost]
    public IActionResult Create(CreateTaskRequest request)
    {
        var task = _taskService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, UpdateTaskRequest request)
    {
        var task = _taskService.Update(id, request);
        if (task == null)
            return NotFound();

        return Ok(task);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        if (!_taskService.Delete(id))
            return NotFound();

        return NoContent();
    }
}