using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace NPV.API.Data;

// This DbContext is used by ASP.NET Core Identity for user and role management.
public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Add DbSet<T> here for your own domain entities if needed.
}
