using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace ERNI.PhotoDatabase.DataAccess.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task SaveChanges(CancellationToken cancellationToken);

        Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken);
    }
}
