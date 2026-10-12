using InternshipProject.Models;
using Microsoft.EntityFrameworkCore;

namespace InternshipProject.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().ToTable("Users");
        modelBuilder.Entity<AppUser>().Property(u => u.Username).UseCollation("NOCASE");
        modelBuilder.Entity<AppUser>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<AppUser>().HasOne(u => u.Role).WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Role>().HasIndex(r => r.Name).IsUnique();
        modelBuilder.Entity<Role>().ToTable("Roles", table =>
            table.HasCheckConstraint("CK_Roles_Name", "Name IN ('Write', 'Read')"));
        modelBuilder.Entity<Product>().ToTable("Products", table =>
            table.HasCheckConstraint("CK_Products_Price", "CAST(Price AS REAL) > 0"));
    }
}
