using System;

namespace TeamFlow.Api.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Status { get; set; } = "TODO";
    public DateTime CreatedAt { get; set; }
}
