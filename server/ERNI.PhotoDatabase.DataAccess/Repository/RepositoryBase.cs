namespace ERNI.PhotoDatabase.DataAccess.Repository
{
    internal abstract class RepositoryBase
    {
        protected RepositoryBase(DatabaseContext dbContext)
        {
            this.DbContext = dbContext;
        }

        protected DatabaseContext DbContext { get; }
    }
}
