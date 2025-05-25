using Microsoft.EntityFrameworkCore;
using Weather.Domain.Entities;
using Weather.Infrastructure.Abstractions.DataAccess;

namespace Weather.Infrastructure.DataAccess;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Measurement> Measurements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("weather");

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}