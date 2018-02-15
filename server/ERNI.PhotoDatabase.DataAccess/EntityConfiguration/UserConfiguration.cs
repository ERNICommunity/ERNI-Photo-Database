using ERNI.PhotoDatabase.DataAccess.DomainModel;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERNI.PhotoDatabase.DataAccess.EntityConfiguration
{
    public class UserConfiguration : EntityTypeConfiguration<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(bc => bc.Id);
        }
    }

}
