using ERNI.PhotoDatabase.Annotator.DataStructures;
using ERNI.PhotoDatabase.Annotator.Utils;
using ERNI.PhotoDatabase.DataAccess.Images;
using ERNI.PhotoDatabase.DataAccess.Repository;
using ERNI.PhotoDatabase.DataAccess.UnitOfWork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERNI.PhotoDatabase.Annotator
{
    public class PhotoAnnotator
    {
        private readonly Lazy<IPhotoRepository> photoRepository;
        private readonly Lazy<ITagRepository> tagRepository;
        private readonly Lazy<ImageStore> imageStore;
        private readonly Lazy<IUnitOfWork> unitOfWork;

        public PhotoAnnotator(Lazy<IPhotoRepository> photoRepository,
            Lazy<ITagRepository> tagRepository,
            Lazy<ImageStore> imageStore,
            Lazy<IUnitOfWork> unitOfWork)
        {
            this.photoRepository = photoRepository;
            this.tagRepository = tagRepository;
            this.imageStore = imageStore;
            this.unitOfWork = unitOfWork;
        }

        private IPhotoRepository PhotoRepository => photoRepository.Value;

        private ITagRepository TagRepository => tagRepository.Value;

        private ImageStore ImageStore => imageStore.Value;

        private IUnitOfWork UnitOfWork => unitOfWork.Value;

        public async Task AnnotatePhotos(IEnumerable<int> photoIds)
        {
            await DownloadPhotos(photoIds, CancellationToken.None);
            var predictor = new AnnotationPredictor();
            var results = predictor.MakePredictions();

            var processedImages = results.Select(r => new ProcessedImage
            {
                Id = int.Parse(r.Key.Split("-")[0]),
                Name = r.Key.Split("-")[1],
                Tags = r.Value
            });

            await SaveTags(processedImages.ToList(), CancellationToken.None);

            //await HouseKeeping();
        }

        private async Task DownloadPhotos(IEnumerable<int> photoIds, CancellationToken cancellationToken)
        {
            foreach (var photoId in photoIds)
            {
                var photo = await PhotoRepository.GetPhoto(photoId, cancellationToken);

                if (photo == null)
                {
                    return;
                }

                var image = await ImageStore.GetImageBlobAsync(photo.FullSizeImageId, cancellationToken);

                var data = image.Content;

                File.WriteAllBytes(Path.Combine(FileUtils.ImagesFolder, $"{photoId}-{photo.Name}"), data);
            }
        }

        private async Task SaveTags(List<ProcessedImage> images, CancellationToken cancellationToken)
        {
            using (var t = await UnitOfWork.BeginTransaction(cancellationToken))
            {
                foreach (var image in images)
                {
                    var photo = await PhotoRepository.GetPhoto(image.Id, cancellationToken);

                    photo.Name = image.Name;

                    await TagRepository.SetTagsForImage(image.Id, image.Tags, cancellationToken);

                    await UnitOfWork.SaveChanges(cancellationToken);
                }
                t.Commit();
            }
        }
    }
}
