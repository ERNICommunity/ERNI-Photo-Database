using System;
using System.Linq;
using System.Threading;
using Autofac.Extras.Moq;
using ERNI.PhotoDatabase.DataAccess;
using ERNI.PhotoDatabase.DataAccess.DomainModel;
using ERNI.PhotoDatabase.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace ERNI.PhotoDatabase.Server.Test
{
    public class TagRepositoryTest
    {
        [Fact]
        public async void GetMostUsedTags_ResultIsOrdered()
        {
            using (var mock = AutoMock.GetLoose())
            {
                MockDatabase(mock);
                var sut = mock.Create<TagRepository>();

                var result = sut.GetMostUsedTags(CancellationToken.None);

                result.ShouldBe(result.OrderByDescending(_ => _.PhotoTags.Count));
            }
        }

        [Fact]
        public async void SetTagsForImage_AddNonExistingTag_CreatesEntity()
        {
            using (var mock = AutoMock.GetLoose())
            {
                MockDatabase(mock);

                var db = mock.Create<DatabaseContext>();
                var photo = db.Photos.Include(p => p.PhotoTags).ThenInclude(pt => pt.Tag).First();

                var tags = photo.PhotoTags.Select(_ => _.Tag.Text).ToList();

                const string tagName = "Taggggg";
                tags.Add(tagName);

                var sut = mock.Create<TagRepository>();
                await sut.SetTagsForImage(photo.Id, tags.ToArray(), CancellationToken.None);

                var newTagEntity = photo.PhotoTags.SingleOrDefault(_ => _.Tag.Text == tagName)?.Tag;

                newTagEntity.ShouldNotBeNull();
                db.Entry(newTagEntity).State.ShouldBe(EntityState.Added);
            }
        }

        [Fact]
        public async void SetTagsForImage_AddExistingTag_ReusesEntity()
        {
            using (var mock = AutoMock.GetLoose())
            {
                MockDatabase(mock);

                var db = mock.Create<DatabaseContext>();
                var photo = db.Photos.Include(p => p.PhotoTags).ThenInclude(pt => pt.Tag).First();

                var allTags = db.Tags.Select(_ => _.Text).ToList();
                var presentTags = photo.PhotoTags.Select(_ => _.Tag.Text).ToList();
                var unusedTags = allTags.Except(presentTags);
                var addedTag = unusedTags.First();
                presentTags.Add(addedTag);

                var sut = mock.Create<TagRepository>();
                await sut.SetTagsForImage(photo.Id, presentTags.ToArray(), CancellationToken.None);

                var newTagEntity = photo.PhotoTags.SingleOrDefault(_ => _.Tag.Text == addedTag)?.Tag;

                newTagEntity.ShouldNotBeNull();
                db.Entry(newTagEntity).State.ShouldBe(EntityState.Unchanged);
            }
        }

        [Fact]
        public async void SetTagsForImage_RemoveExistingTag_RemovesLink()
        {
            using (var mock = AutoMock.GetLoose())
            {
                MockDatabase(mock);

                var db = mock.Create<DatabaseContext>();
                var photo = db.Photos.Include(p => p.PhotoTags).ThenInclude(pt => pt.Tag).First();
                
                var presentTags = photo.PhotoTags.Select(_ => _.Tag.Text).ToList();
                var removedTag = presentTags.First();
                presentTags.Remove(removedTag);

                var sut = mock.Create<TagRepository>();
                await sut.SetTagsForImage(photo.Id, presentTags.ToArray(), CancellationToken.None);

                db.SaveChanges();

                var linkEntity = photo.PhotoTags.SingleOrDefault(_ => _.Tag.Text == removedTag);
                var tagEntity = db.Tags.SingleOrDefault(_ => _.Text == removedTag);

                linkEntity.ShouldBeNull();
                tagEntity.ShouldNotBeNull();
            }
        }


        private void MockDatabase(AutoMock mock)
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new DatabaseContext(options);

            foreach (var i in Enumerable.Range(1, 20))
            {
                context.Tags.Add(new Tag { Text = $"Tag {i}" });
            }

            context.SaveChanges();

            foreach (var i in Enumerable.Range(1, 10))
            {
                var photo = new Photo
                {
                    FullSizeImageId = Guid.NewGuid(),
                    ThumbnailImageId = Guid.NewGuid(),
                    Name = $"Image {i}.jpg"
                };
                
                context.Photos.Add(photo);

                foreach (var tag in context.Tags.Take(i))
                {
                    context.PhotoTag.Add(new PhotoTag { Tag = tag, Photo = photo });
                }
            }

            context.SaveChanges();

            mock.Provide(context);
        }
    }
}
