using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ERNI.PhotoDatabase.DataAccess.DomainModel;

namespace ERNI.PhotoDatabase.DataAccess.Repository
{
    public interface ITagRepository
    {
        Task<List<Tag>> GetMostUsedTags(CancellationToken token);
        Task<List<Tag>> GetAllTags(CancellationToken token);
        Task<string[]> GetTagsForImage(int photoId, CancellationToken cancellationToken);
        Task SetTagsForImage(int photoId, string[] tags, CancellationToken cancellationToken);
    }
}