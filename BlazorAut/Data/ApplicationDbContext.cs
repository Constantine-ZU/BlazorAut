using Microsoft.EntityFrameworkCore;

namespace BlazorAut.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AppSetting> AppSettings { get; set; }
    }

}
