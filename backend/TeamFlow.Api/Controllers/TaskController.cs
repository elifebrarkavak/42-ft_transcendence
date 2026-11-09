using Microsoft.AspNetCore.Mvc;
using TeamFlow.Api.Domain;
using TeamFlow.Api.Dtos;

namespace TeamFlow.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    private static readonly List<TaskItem> Tasks = new(); //veri bellekte tutuluyor
    [HttpPost]
    public IActionResult Create([FromBody] CreateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new
            {
                code = "VALIDATION_ERROR",
                errors = new
                {
                    title = new[] { "REQUIRED" }
                }
            });
        }

        var task = new TaskItem
        {
            Title = request.Title.Trim()
        };
        Tasks.Add(task);

        // Görev bilgilerini cevap olarak gönderiyoruz.
        return Ok(new
        {
            id = task.Id,
            title = task.Title,
            status = task.Status,
            createdAt = task.CreatedAt
        });
    }
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(Tasks);
    }
    [HttpGet("{id:guid}")]
    public IActionResult GetById([FromRoute] Guid id)
    {
        var task = Tasks.FirstOrDefault(item => item.Id == id);

        if(task is null)
        {
            return NotFound(new
            {
                code = "TASK_NOT_FOUND"
            });
        }
        return Ok(task);
    }

    [HttpPatch("{id:guid}")]
    public IActionResult Update(
        [FromRoute] Guid id,
        [FromBody] UpdateTaskRequest request)
    {
        if(string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new
            {
                code = "VALIDATION_ERROR",
                errors = new
                {
                    title = new[] { "REQUIRED" }
                }
            });
        }
        var task = Tasks.FirstOrDefault(item => item.Id == id);

        if(task is null)
        {
            return NotFound(new
            {
                code = "TASK_NOT_FOUND"
            });
        }
        task.Title = request.Title.Trim();
        return Ok(task);
    }
}
