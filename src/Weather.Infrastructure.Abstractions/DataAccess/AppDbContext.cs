using Microsoft.EntityFrameworkCore;
using Weather.Domain.Entities;

namespace Weather.Infrastructure.Abstractions.DataAccess;

public interface IAppDbContext
{
    DbSet<Measurement> Measurements { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}