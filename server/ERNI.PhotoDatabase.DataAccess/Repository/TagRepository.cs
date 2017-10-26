using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERNI.PhotoDatabase.DataAccess.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace ERNI.PhotoDatabase.DataAccess.Repository
{
    internal class TagRepository : RepositoryBase, ITagRepository
    {
        public TagRepository(DatabaseContext dbContext)
            : base(dbContext)
        {
        }

        public Task<List<Tag>> GetMostUsedTags(CancellationToken cancellationToken)
        {
            var query = this.DbContext.Tags.Include(t => t.PhotoTags).OrderByDescending(_ => _.PhotoTags.Count);

            return query.ToListAsync(cancellationToken);
        }

        public Task<string[]> GetTagsForImage(int photoId, CancellationToken cancellationToken)
        {
            return this.DbContext.PhotoTag.Include(pt => pt.Tag).Where(_ => _.PhotoId == photoId).Select(_ => _.Tag.Text)
                .ToArrayAsync(cancellationToken);
        }

        public async Task SetTagsForImage(int photoId, string[] tags, CancellationToken cancellationToken)
        {
            tags = tags ?? Array.Empty<string>();

            var photo = await this.DbContext.Photos.Include(p => p.PhotoTags).ThenInclude(pt => pt.Tag)
                .SingleOrDefaultAsync(_ => _.Id == photoId, cancellationToken);

            foreach (var photoTag in photo.PhotoTags.ToList())
            {
                if (!tags.Contains(photoTag.Tag.Text))
                {
                    photo.PhotoTags.Remove(photoTag);
                }
            }

            var existingTags = new HashSet<string>(photo.PhotoTags.Select(_ => _.Tag.Text));
            var addedTags = tags.Except(existingTags);

            var allTags = await this.DbContext.Tags.ToDictionaryAsync(_ => _.Text, cancellationToken);

            foreach (var tag in addedTags)
            {
                if (!allTags.TryGetValue(tag, out var existing))
                {
                    existing = new Tag {Text = tag};
                    allTags.Add(tag, existing);
                    this.DbContext.Tags.Add(existing);
                }

                photo.PhotoTags.Add(new PhotoTag {Tag = existing});
            }
        }
    }
}