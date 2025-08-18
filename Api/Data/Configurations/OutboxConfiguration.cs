using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class OutboxConfiguration : IEntityTypeConfiguration<Outbox>
{
    public void Configure(EntityTypeBuilder<Outbox> builder)
    {
        builder.ToTable("outbox");
        builder.Property(x => x.Payload).HasColumnType("jsonb");
    }
}