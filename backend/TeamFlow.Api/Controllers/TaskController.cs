using Microsoft.AspNetCore.Mvc;
using TeamFlow.Api.Data;
using TeamFlow.Api.Domain;
using TeamFlow.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace TeamFlow.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    private readonly AppDbContext _db;

    public TaskController(AppDbContext db)
    {
        _db = db;
    }
    //private static readonly List<TaskItem> Tasks = new(); //veri bellekte tutuluyor
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTaskRequest request)
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
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        //Tasks.Add(task);

        // Görev bilgilerini cevap olarak gönderiyoruz.
        // return Ok(new
        // {
        //     id = task.Id,
        //     title = task.Title,
        //     status = task.Status,
        //     createdAt = task.CreatedAt
        // });
    return CreatedAtAction(
    nameof(GetById),
    new { id = task.Id },
    new
    {
        id = task.Id,
        title = task.Title,
        status = task.Status,
        createdAt = task.CreatedAt
    });
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await _db.Tasks
            .AsNoTracking()
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var task = await _db.Tasks
        .AsNoTracking()
        .FirstOrDefaultAsync(item => item.Id == id);

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
    public async Task<IActionResult> Update(
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
        var task = await _db.Tasks
            .FirstOrDefaultAsync(item => item.Id == id);

        if(task is null)
        {
            return NotFound(new
            {
                code = "TASK_NOT_FOUND"
            });
        }
        task.Title = request.Title.Trim();

        await _db.SaveChangesAsync();
        return Ok(task);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var task = await _db.Tasks
            .FirstOrDefaultAsync( item => item.Id == id);
        
        if(task is null)
        {
            return NotFound(new
            {
                code = "TASK_NOT_FOUND"
            });
        }
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
