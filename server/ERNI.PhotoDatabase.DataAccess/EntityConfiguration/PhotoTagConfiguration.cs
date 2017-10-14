using ERNI.PhotoDatabase.DataAccess.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERNI.PhotoDatabase.DataAccess.EntityConfiguration
{
    public class PhotoTagConfiguration : EntityTypeConfiguration<PhotoTag>
    {
        public override void Configure(EntityTypeBuilder<PhotoTag> builder)
        {
            builder.ToTable("PhotoTag");

            builder.HasKey(bc => new { bc.PhotoId, bc.TagId });

            builder.HasOne(bc => bc.Photo).WithMany(b => b.PhotoTag).HasForeignKey(bc => bc.PhotoId);
            builder.HasOne(bc => bc.Tag).WithMany(c => c.PhotoTag).HasForeignKey(bc => bc.TagId);
        }
    }
}
