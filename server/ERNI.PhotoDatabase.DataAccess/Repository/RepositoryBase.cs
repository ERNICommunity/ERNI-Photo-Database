namespace ERNI.PhotoDatabase.DataAccess.Repository
{
    public class RepositoryBase
    {
        protected RepositoryBase(DatabaseContext dbContext)
        {
            this.DbContext = dbContext;
        }

        protected DatabaseContext DbContext { get; }
    }
}
