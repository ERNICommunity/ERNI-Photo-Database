using ERNI.PhotoDatabase.DataAccess.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERNI.PhotoDatabase.DataAccess.EntityConfiguration
{
    public class TagConfiguration:EntityTypeConfiguration<Tag>
    {
        public override void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("Tag");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Text).IsRequired();

            builder.HasIndex(p => p.Text).IsUnique();
        }
    }
}
