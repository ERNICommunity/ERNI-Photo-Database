namespace ERNI.PhotoDatabase.DataAccess.Repository
{
    public class PhotoRepository : RepositoryBase, IPhotoRepository
    {
        public PhotoRepository(DatabaseContext dbContext) 
            : base(dbContext)
        {
        }
    }
}
