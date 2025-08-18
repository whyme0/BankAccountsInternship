using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class InboxDeadLettersConfiguration : IEntityTypeConfiguration<InboxConsumed>
{
    public void Configure(EntityTypeBuilder<InboxConsumed> builder)
    {
        builder.ToTable("inbox_dead_letters");
    }
}