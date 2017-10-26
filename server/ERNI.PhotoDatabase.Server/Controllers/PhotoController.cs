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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ERNI.PhotoDatabase.Server.Controllers
{

    [Route("api/[controller]")]
    public class PhotoController : Controller
    {
        private readonly Lazy<IPhotoRepository> repository;
        private readonly Lazy<ImageStore> imageStore;
        private readonly Lazy<IUnitOfWork> unitOfwork;
        private readonly Lazy<IImageManipulation> imageTools;
        private readonly IOptions<ImageSizesSettings> settings;

        public PhotoController(Lazy<IPhotoRepository> repository, Lazy<ImageStore> imageStore, Lazy<IUnitOfWork> unitOfwork, Lazy<IImageManipulation> imageTools, IOptions<ImageSizesSettings> settings)
        {
            this.repository = repository;
            this.imageStore = imageStore;
            this.unitOfwork = unitOfwork;
            this.imageTools = imageTools;
            this.settings = settings;
        }
        
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var photos = await this.repository.Value.GetAllPhotos(cancellationToken);
            return Ok(photos);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var photo = await this.repository.Value.GetPhoto(id, cancellationToken);

            return Ok(photo);
        }

        [HttpGet("{id}/download/{size?}")]
        public async Task<IActionResult> Download(int id, int? size, CancellationToken cancellationToken)
        {
            var photo = await this.repository.Value.GetPhoto(id, cancellationToken);

            if (photo == null)
            {
                return NotFound();
            }

            var image = await this.imageStore.Value.GetImageBlobAsync(photo.FullSizeImageId, cancellationToken);

            var data = image.Content;

            if (size != null)
            {
                data = imageTools.Value.ResizeTo(image.Content, size.Value);
            }

            return File(data, photo.Mime, photo.Name);
        }

        [HttpGet("{id}/thumbnail")]
        public async Task<IActionResult> Thumbnail(int id, CancellationToken cancellationToken)
        {
            var photo = await this.repository.Value.GetPhoto(id, cancellationToken);

            if (photo == null)
            {
                return NotFound();
            }

            var image = await this.imageStore.Value.GetImageBlobAsync(photo.ThumbnailImageId, cancellationToken);

            return File(image.Content, "image/jpeg");
        }
        
        [HttpGet("search/tag/{tag}")]
        public async Task<IActionResult> GetImagesByTag(string tag, CancellationToken cancellationToken)
        {
            var photos = await this.repository.Value.GetPhotosByTag(tag, cancellationToken);

            return Ok(photos);
        }
        
        // GET api/values
        /// <summary>
        /// Uploads the specified files.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Upload(List<IFormFile> files, CancellationToken cancellationToken)
        {
            var photos = new List<Photo>();

            foreach (var formFile in files)
            {
                using (var openReadStream = formFile.OpenReadStream())
                {
                    var data = new byte[formFile.Length];
                    openReadStream.Read(data, 0, data.Length);

                    var thumbnailData = imageTools.Value.ResizeTo(data, this.settings.Value.Thumbnail);
                    var (width, height) = imageTools.Value.GetSize(data);

                    var fullSizeBlob = new ImageBlob {Content = data, Id = Guid.NewGuid()};
                    var thumbnailBlob = new ImageBlob {Content = thumbnailData, Id = Guid.NewGuid()};
                    await this.imageStore.Value.SaveImageBlobAsync(fullSizeBlob, cancellationToken);
                    await this.imageStore.Value.SaveImageBlobAsync(thumbnailBlob, cancellationToken);

                    var photo = this.repository.Value.StorePhoto(formFile.FileName, fullSizeBlob.Id, thumbnailBlob.Id, formFile.ContentType, width, height);

                    photos.Add(photo);
                }
            }

            await this.unitOfwork.Value.SaveChanges(cancellationToken);

            return RedirectToAction("Index", "Tag", new {fileIds = photos.Select(_ => _.Id).ToList()});
        }
    }
}