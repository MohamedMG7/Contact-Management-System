using Contact_Management_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Contact_Management_system.DbHelper
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // applicationUser Table
            modelBuilder.Entity<ApplicationUser>().HasKey(k => k.Id);
            modelBuilder.Entity<ApplicationUser>().HasMany(sc => sc.Contacts).WithOne(s => s.ApplicationUser)
                .HasForeignKey(s => s.ApplicationUserId).OnDelete(DeleteBehavior.Cascade);

            // Contact Table
            modelBuilder.Entity<Contact>().HasKey(k => k.Id);
            modelBuilder.Entity<Contact>().HasIndex(p => new  { p.ApplicationUserId, p.PhoneNumber }).IsUnique();
            
        }
        
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Contact> Contacts { get; set; }
    }
}
