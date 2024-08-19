using DockerToDoTest.Models;
using Microsoft.EntityFrameworkCore;

namespace DockerToDoTest.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Todo> Todos { get; set; }
}
