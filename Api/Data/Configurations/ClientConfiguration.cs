using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data.Configurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Name)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
