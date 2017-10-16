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

        public Task<List<Photo>> GetPhotosByTag(string tag, CancellationToken cancellationToken)
        {
            var query = this.DbContext.Photos.Include(p => p.PhotoTags).ThenInclude(pt => pt.Tag)
                .Where(_ => _.PhotoTags.Any(__ => __.Tag.Text == tag));

            return query.ToListAsync(cancellationToken);
        }

        public Task<List<Photo>> GetAllPhotos(CancellationToken cancellationToken)
        {
            return this.DbContext.Photos.Include(p => p.PhotoTags).ThenInclude(pt => pt.Tag).ToListAsync(cancellationToken);
        }

        public Task<List<Photo>> GetPhotos(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            return this.DbContext.Photos.Include(p => p.PhotoTags).ThenInclude(pt => pt.Tag)
                .Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);
        }

        public Task<Photo> GetPhoto(int id, CancellationToken cancellationToken)
        {
            return this.DbContext.Photos.Include(p => p.PhotoTags).ThenInclude(pt => pt.Tag)
                .SingleOrDefaultAsync(_ => _.Id == id, cancellationToken);
        }

        public Photo StorePhoto(string fileName, Guid fullSizeBlobId, Guid thumbnailBlobId)
        {
            var photo = new Photo
            {
                Name = fileName,
                FullSizeImageId = fullSizeBlobId,
                ThumbnailImageId = thumbnailBlobId
            };

            this.DbContext.Photos.Add(photo);

            return photo;
        }
    }
}
