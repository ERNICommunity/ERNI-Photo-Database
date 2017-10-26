using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ERNI.PhotoDatabase.DataAccess.DomainModel;

namespace ERNI.PhotoDatabase.DataAccess.Repository
{
    public interface IPhotoRepository
    {
        Task<List<Photo>> GetPhotosByTag(string tag, CancellationToken cancellationToken);
        Task<List<Photo>> GetAllPhotos(CancellationToken cancellationToken);
        Task<List<Photo>> GetPhotos(IEnumerable<int> ids, CancellationToken cancellationToken);
        Task<Photo> GetPhoto(int id, CancellationToken cancellationToken);
        Photo StorePhoto(string fileName, Guid fullSizeBlobId, Guid thumbnailBlobId, string mime);
    }
}
