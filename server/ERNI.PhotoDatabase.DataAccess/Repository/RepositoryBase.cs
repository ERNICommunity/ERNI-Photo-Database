using System;

namespace ERNI.PhotoDatabase.DataAccess.Repository
{
    internal abstract class RepositoryBase : IDisposable
    {
        protected RepositoryBase(DatabaseContext dbContext)
        {
            if(dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            this.DbContext = dbContext;
        }

        protected DatabaseContext DbContext { get; }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}
