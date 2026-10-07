using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.Api.Dtos;
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
            new TaskItem {Id = 2, Title = "Database Schema", Status = "IN_PROGRESS", CreatedAt = DateTime.UtcNow},
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

        [HttpPost]
        public IActionResult Create(CreateTaskRequest request)
        {
            var newId = _tasks.Count == 0 ? 1 : _tasks.Max(t => t.Id) + 1;

            var task = new TaskItem
            {
                Id = newId,
                Title = request.Title,
                CreatedAt = DateTime.UtcNow  
            };
            _tasks.Add(task);
            return CreatedAtAction(nameof(GetById), new {id = task.Id}, task);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, UpdateTaskRequest request)
        {
            var task = _tasks.FirstOrDefault( t => t.Id == id);
            if(task == null)
                return NotFound();
            task.Title = request.Title;
            task.Status =request.Status;
            return Ok(task);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var task = _tasks.FirstOrDefault( t => t.Id == id);
            if(task == null)
                return NotFound();
            _tasks.Remove(task);
            return NoContent();
        }
    }

}

