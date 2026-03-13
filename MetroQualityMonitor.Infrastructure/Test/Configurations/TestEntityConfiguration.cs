using MetroQualityMonitor.Domain.Test.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetroQualityMonitor.Infrastructure.Test.Configurations;

public class TestEntityConfiguration : IEntityTypeConfiguration<TestEntity>
{
    public void Configure(EntityTypeBuilder<TestEntity> builder)
    {
        builder.ToTable("TestEntities");

        builder.HasKey(x => x.Id);

        /*builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CreateDateTimeUtc)
            .IsRequired();*/
    }
}