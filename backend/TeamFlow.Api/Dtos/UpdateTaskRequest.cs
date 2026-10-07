using System;
using System.ComponentModel.DataAnnotations;

namespace TeamFlow.Api.Dtos;

public class UpdateTaskRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";

    [Required]
    [RegularExpression("^(TODO|IN_PROGRESS|DONE)$")]
    public string Status { get; set; }  = "";
}
