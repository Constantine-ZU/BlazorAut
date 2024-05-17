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
        public DbSet<AuthCode> AuthCodes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserToken>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.Tokens)
                .HasForeignKey(ut => ut.UserId);
        }
    }

}
