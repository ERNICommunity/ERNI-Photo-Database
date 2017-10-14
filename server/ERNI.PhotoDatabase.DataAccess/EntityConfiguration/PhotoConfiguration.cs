using ERNI.PhotoDatabase.DataAccess.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERNI.PhotoDatabase.DataAccess.EntityConfiguration
{
    public class PhotoConfiguration : EntityTypeConfiguration<Photo>
    {
        public override void Configure(EntityTypeBuilder<Photo> builder)
        {
            builder.ToTable("Photo");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired();
            builder.Property(p => p.FullSizeImageId).IsRequired();
            builder.Property(p => p.ThumbnailImageId).IsRequired();

            builder.HasIndex(p => p.FullSizeImageId).IsUnique();
            builder.HasIndex(p => p.ThumbnailImageId).IsUnique();
        }
    }
}
