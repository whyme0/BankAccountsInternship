using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasIndex(t => new { t.AccountId, t.Date });
        builder.HasIndex(t => t.Date)
            .HasMethod("GiST");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(t => t.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.Date)
            .IsRequired();
    }
}