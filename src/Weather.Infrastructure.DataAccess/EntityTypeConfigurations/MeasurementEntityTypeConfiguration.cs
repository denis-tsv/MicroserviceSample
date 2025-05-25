using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Weather.Domain.Entities;

namespace Weather.Infrastructure.DataAccess.EntityTypeConfigurations;

public class MeasurementEntityTypeConfiguration : IEntityTypeConfiguration<Measurement>
{
    public void Configure(EntityTypeBuilder<Measurement> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.City).HasMaxLength(128);

        builder.HasIndex(x => new {x.City, x.Date}).IsUnique();
    }
}