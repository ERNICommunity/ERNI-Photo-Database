namespace ERNI.PhotoDatabase.DataAccess.Repository
{
    public class RepositoryBase
    {
        protected RepositoryBase(DatabaseContext dbContext)
        {
            DbContext = dbContext;
        }

        protected DatabaseContext DbContext { get; set; }
    }
}
