using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ERNI.PhotoDatabase.DataAccess.DomainModel;

namespace ERNI.PhotoDatabase.DataAccess.Repository
{
    public interface IPhotoRepository
    {
        Task<Photo[]> GetPhotosByTag(string[] tags, CancellationToken cancellationToken);

        Task<Photo[]> SearchPhotos(string[] expressions, CancellationToken cancellationToken);

        Task<List<Photo>> GetAllPhotos(CancellationToken cancellationToken);

        Task<List<Photo>> GetPhotos(IEnumerable<int> ids, CancellationToken cancellationToken);

        Task<Photo> GetPhoto(int id, CancellationToken cancellationToken);

        Photo StorePhoto(string fileName, Guid fullSizeBlobId, Guid thumbnailBlobId, string mime, int width, int height);

        void DeletePhoto(Photo photo);
    }
}
