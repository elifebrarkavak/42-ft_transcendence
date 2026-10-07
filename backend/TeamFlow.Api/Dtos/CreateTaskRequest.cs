using System.ComponentModel.DataAnnotations;

namespace TeamFlow.Api.Dtos;

public class CreateTaskRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";
}
