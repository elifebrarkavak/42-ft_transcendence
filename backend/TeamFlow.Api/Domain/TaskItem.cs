
namespace TeamFlow.Api.Domain;

public class TaskItem
{
    public Guid Id {get; set; } = Guid.NewGuid(); //create new id
    public string Title { get; set;} = string.Empty; 
    public string Status {get; set;} = "TODO";
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
}