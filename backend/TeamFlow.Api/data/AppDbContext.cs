
using Microsoft.EntityFrameworkCore;
using TeamFlow.Api.Domain;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) 
        :base(options)
    {
        
    }
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
}