# Internship Project

A C# ASP.NET Core MVC application with Users, Roles and Products.

## Requirements

- A .NET SDK that supports .NET 8.
- SQLite is used locally; no database server is required.

## Run

```powershell
dotnet build InternshipProject.sln -c Release
dotnet run --project InternshipProject.csproj
```

Open http://localhost:5080, or open `InternshipProject.sln` in Visual Studio and run the project.

The database is created automatically at `Data/internship.db`. Its data persists after restarting the application. Database files are excluded from Git; the C# files in `Data/` are included.

## Development accounts

On the first run in Development, these accounts are created if the Users table is empty:

| Username | Password | Role |
| --- | --- | --- |
| writer | WriteUser!123 | Write |
| reader | ReadUser!123 | Read |

The default launch profile uses Development. These accounts are for a local demonstration.

## Permissions

| Operation | Write | Read |
| --- | --- | --- |
| Products list and details | Allowed | Allowed |
| Products create, edit and delete | Allowed | Denied |
| Users full CRUD | Allowed | Denied |
| Roles list and details | Allowed | Allowed |
| Roles create, edit and delete | Unavailable | Unavailable |

Every user is assigned one of the two roles. User management requires Write so Read users cannot change their own permissions. The final Write user cannot be deleted or demoted.

## Implementation

- Entity Framework Core defines the tables and the user-to-role foreign key.
- Passwords are stored as hashes; login uses cookie authentication.
- Controllers enforce role restrictions on both GET and POST actions.
- Forms validate input and require antiforgery tokens for POST requests.
- Role changes take effect on the next request; password changes and deletion invalidate previous sessions.
- The fixed database schema is created with `EnsureCreated`; migrations are not included.
