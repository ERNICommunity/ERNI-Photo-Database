using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERNI.PhotoDatabase.DataAccess.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace ERNI.PhotoDatabase.DataAccess.Repository
{
    internal class PhotoRepository : RepositoryBase, IPhotoRepository
    {
        public PhotoRepository(DatabaseContext dbContext) 
            : base(dbContext)
        {
        }

        public Task<Photo[]> GetPhotosByTag(string[] tags, CancellationToken cancellationToken)
        {
            var photos = DbContext.Photos.Where(_ => _.PhotoTags.Any(t => tags.Contains(t.Tag.Text))).ToArray();

            var photoIds = photos.Select(_ => _.Id).ToArray();

            // This query loads the tags of the photos.
            // These objects get assigned to their photos automatically by Entity framework.
            DbContext.PhotoTag.Include(_ => _.Tag).Where(_ => photoIds.Contains(_.PhotoId)).ToArray();

            var oderedPhotos = photos
                .Select(_ => new { Photo = _, Count = _.PhotoTags.Count(t => tags.Contains(t.Tag.Text)) })
                .OrderByDescending(_ => _.Count)
                .Select(_ => _.Photo)
                .ToArray();

            return Task.FromResult(oderedPhotos);
        }

        public Task<List<Photo>> GetAllPhotos(CancellationToken cancellationToken)
        {
            return DbContext.Photos.Include(p => p.PhotoTags).ThenInclude(pt => pt.Tag).ToListAsync(cancellationToken);
        }

        public Task<List<Photo>> GetPhotos(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            return DbContext.Photos.Include(p => p.PhotoTags).ThenInclude(pt => pt.Tag)
                .Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);
        }

        public Task<Photo> GetPhoto(int id, CancellationToken cancellationToken)
        {
            return DbContext.Photos.Include(p => p.PhotoTags).ThenInclude(pt => pt.Tag)
                .SingleOrDefaultAsync(_ => _.Id == id, cancellationToken);
        }

        public Photo StorePhoto(string fileName, Guid fullSizeBlobId, Guid thumbnailBlobId, string mime, int width, int height)
        {
            var photo = new Photo
            {
                Name = fileName,
                FullSizeImageId = fullSizeBlobId,
                ThumbnailImageId = thumbnailBlobId,
                Mime = mime,
                Width = width,
                Height = height
            };

            DbContext.Photos.Add(photo);

            return photo;
        }

        public void DeletePhoto(Photo photo)
        {
            DbContext.Photos.Remove(photo);
        }
    }
}
