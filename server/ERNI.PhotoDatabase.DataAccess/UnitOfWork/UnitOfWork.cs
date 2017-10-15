using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace ERNI.PhotoDatabase.DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseContext context;

        public UnitOfWork(DatabaseContext context)
        {
            this.context = context;
        }

        public Task SaveChanges(CancellationToken cancellationToken)
        {
            return this.context.SaveChangesAsync(cancellationToken);
        }

        public Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken)
        {
            return this.context.Database.BeginTransactionAsync(cancellationToken);
        }
    }
}