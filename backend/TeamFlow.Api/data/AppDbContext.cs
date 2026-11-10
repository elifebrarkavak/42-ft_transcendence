
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Api.Domain;
namespace TeamFlow.Api.Data;


// public class AppDbContext : DbContext
// {
//     public AppDbContext(DbContextOptions<AppDbContext> options) 
//         :base(options)
//     {
        
//     }
//     public DbSet<TaskItem> Tasks => Set<TaskItem>();
// }

public class AppDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
}