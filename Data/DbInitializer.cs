using InternshipProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InternshipProject.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IWebHostEnvironment environment)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var connection = new SqliteConnectionStringBuilder(db.Database.GetConnectionString());
        if (connection.DataSource != ":memory:")
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(connection.DataSource));
            if (directory is not null) Directory.CreateDirectory(directory);
        }

        // The assignment has a fixed schema: create its database on first run.
        await db.Database.EnsureCreatedAsync();
        foreach (var name in new[] { Role.Write, Role.Read })
        {
            if (!await db.Roles.AnyAsync(r => r.Name == name))
                db.Roles.Add(new Role { Name = name });
        }
        await db.SaveChangesAsync();

        if (environment.IsDevelopment() && !await db.Users.AnyAsync())
        {
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
            var roles = await db.Roles.ToDictionaryAsync(r => r.Name, r => r.Id);
            foreach (var demo in new[]
            {
                (Username: "writer", Name: "Write User", Role: Role.Write, Password: "WriteUser!123"),
                (Username: "reader", Name: "Read User", Role: Role.Read, Password: "ReadUser!123")
            })
            {
                var user = new AppUser { Username = demo.Username, FullName = demo.Name, RoleId = roles[demo.Role] };
                user.PasswordHash = hasher.HashPassword(user, demo.Password);
                db.Users.Add(user);
            }
            await db.SaveChangesAsync();
        }
    }
}
