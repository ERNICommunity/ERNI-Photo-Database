using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using ERNI.PhotoDatabase.DataAccess.DomainModel;
using ERNI.PhotoDatabase.DataAccess.Images;
using ERNI.PhotoDatabase.DataAccess.Repository;
using ERNI.PhotoDatabase.DataAccess.UnitOfWork;
using ERNI.PhotoDatabase.Server.Configuration;
using ERNI.PhotoDatabase.Server.Utils.Image;
using ERNI.PhotoDatabase.Annotator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

namespace ERNI.PhotoDatabase.Server.Controllers
{

    [Route("api/[controller]")]
    [Authorize]
    public class PhotoController : Controller
    {
        private readonly Lazy<IPhotoRepository> repository;
        private readonly Lazy<ITagRepository> tagRepository;
        private readonly Lazy<ImageStore> imageStore;
        private readonly Lazy<IUnitOfWork> unitOfWork;
        private readonly Lazy<IImageManipulation> imageTools;
        private readonly Lazy<PhotoAnnotator> photoAnnotator;
        private readonly IOptions<ImageSizesSettings> settings;

        public PhotoController(Lazy<IPhotoRepository> repository,
            Lazy<ITagRepository> tagRepository,
            Lazy<ImageStore> imageStore, 
            Lazy<IUnitOfWork> unitOfWork, 
            Lazy<IImageManipulation> imageTools, 
            IOptions<ImageSizesSettings> settings,
            Lazy<PhotoAnnotator> photoAnnotator)
        {
            this.repository = repository;
            this.tagRepository = tagRepository;
            this.imageStore = imageStore;
            this.unitOfWork = unitOfWork;
            this.imageTools = imageTools;
            this.settings = settings;
            this.photoAnnotator = photoAnnotator;
        }

        private IPhotoRepository Repository => repository.Value;

        private ITagRepository TagRepository => tagRepository.Value;

        private ImageStore ImageStore => imageStore.Value;

        private IUnitOfWork UnitOfWork => unitOfWork.Value;

        private IImageManipulation ImageTools => imageTools.Value;

        private PhotoAnnotator PhotoAnnotator => photoAnnotator.Value;

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var photos = await this.Repository.GetAllPhotos(cancellationToken);
            return Ok(photos);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var photo = await this.Repository.GetPhoto(id, cancellationToken);

            return Ok(photo);
        }

        [HttpGet("{id}/download/{size?}")]
        public async Task<IActionResult> Download(int id, int? size, CancellationToken cancellationToken)
        {
            var photo = await this.Repository.GetPhoto(id, cancellationToken);

            if (photo == null)
            {
                return NotFound();
            }

            var image = await this.ImageStore.GetImageBlobAsync(photo.FullSizeImageId, cancellationToken);

            var data = image.Content;

            if (size != null)
            {
                data = ImageTools.ResizeTo(image.Content, size.Value);
            }

            return File(data, photo.Mime, photo.Name);
        }

        [HttpGet("{id}/thumbnail")]
        public async Task<IActionResult> Thumbnail(int id, CancellationToken cancellationToken)
        {
            var photo = await this.Repository.GetPhoto(id, cancellationToken);

            if (photo == null)
            {
                return NotFound();
            }

            var image = await this.ImageStore.GetImageBlobAsync(photo.ThumbnailImageId, cancellationToken);
            
            return File(image.Content, "image/jpeg");
        }
        
        [HttpGet("search/tag/{tag}")]
        public async Task<IActionResult> GetImagesByTag(string tag, CancellationToken cancellationToken)
        {
            var photos = await this.Repository.GetPhotosByTag(tag.ToLowerInvariant().Split(" "), cancellationToken);

            return Ok(photos);
        }
        
        // GET api/values
        /// <summary>
        /// Uploads the specified files.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "uploader")]
        public async Task<IActionResult> Upload(List<IFormFile> files, CancellationToken cancellationToken)
        {
            Dictionary<Photo, string[]> taggedPhotos = new Dictionary<Photo, string[]>();

            foreach (var formFile in files)
            {
                using (var openReadStream = formFile.OpenReadStream())
                {
                    var data = new byte[formFile.Length];
                    openReadStream.Read(data, 0, data.Length);

                    var thumbnailData = ImageTools.ResizeTo(data, this.settings.Value.Thumbnail);
                    var (width, height) = ImageTools.GetSize(data);

                    var fullSizeBlob = new ImageBlob {Content = data, Id = Guid.NewGuid()};
                    var thumbnailBlob = new ImageBlob {Content = thumbnailData, Id = Guid.NewGuid()};
                    await ImageStore.SaveImageBlobAsync(fullSizeBlob, cancellationToken);
                    await ImageStore.SaveImageBlobAsync(thumbnailBlob, cancellationToken);

                    var tags = PhotoAnnotator.AnnotatePhoto(data);

                    var photo = Repository.StorePhoto(formFile.FileName, fullSizeBlob.Id, thumbnailBlob.Id, formFile.ContentType, width, height);

                    taggedPhotos.Add(photo, tags);
                }
            }

            await this.UnitOfWork.SaveChanges(cancellationToken);
            using (var t = await this.UnitOfWork.BeginTransaction(cancellationToken))
            {
                foreach (var tag in taggedPhotos)
                {
                    await TagRepository.SetTagsForImage(tag.Key.Id, tag.Value, cancellationToken);
                    await this.UnitOfWork.SaveChanges(cancellationToken);
                }
                t.Commit();
            }

            return RedirectToAction("Index", "Tag", new {fileIds = taggedPhotos.Select(_ => _.Key.Id).ToList()});
        }

        // DELETE api/values
        /// <summary>
        /// Deletes the specified photo.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Produces("application/json")]
        [Authorize(Roles = "uploader")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var photo = await Repository.GetPhoto(id, cancellationToken);

            if (photo == null)
            {
                return NotFound();
            }

            await ImageStore.DeleteImageBlobAsync(photo.FullSizeImageId, cancellationToken);
            await ImageStore.DeleteImageBlobAsync(photo.ThumbnailImageId, cancellationToken);

            Repository.DeletePhoto(photo);

            await UnitOfWork.SaveChanges(cancellationToken);

            return Ok("Photo deleted");
        }
    }
}