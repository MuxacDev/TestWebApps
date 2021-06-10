using Microsoft.EntityFrameworkCore;

namespace TaskManagerDMD.Models
{
    public class TaskContext : DbContext
    {        
        public DbSet<TmTask> Tasks { get; set; }
        public TaskContext(DbContextOptions<TaskContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
