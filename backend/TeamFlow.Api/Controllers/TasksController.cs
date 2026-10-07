using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.Api.Models;

namespace TeamFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private static readonly List<TaskItem> _tasks = new()
        {
            new TaskItem {Id = 1, Title = "Login Page", CreatedAt = DateTime.UtcNow},
            new TaskItem {Id = 2, Title = "Database Shame", Status = "IN_PROGRESS", CreatedAt = DateTime.UtcNow},
        };

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_tasks);
        }
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if(task == null)
                return NotFound();
            return Ok(task);
        }
    }

}

