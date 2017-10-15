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
using ERNI.PhotoDatabase.Server.Utils.Image;
using Microsoft.AspNetCore.Http;

namespace ERNI.PhotoDatabase.Server.Controllers
{

    [Route("api/[controller]")]
    public class PhotoController : Controller
    {
        private readonly IPhotoRepository repository;
        private readonly ImageStore imageStore;
        private readonly IUnitOfWork unitOfwork;

        public PhotoController(IPhotoRepository repository, ImageStore imageStore, IUnitOfWork unitOfwork)
        {
            this.repository = repository;
            this.imageStore = imageStore;
            this.unitOfwork = unitOfwork;
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var photos = await this.repository.GetPhotos(cancellationToken);
            return Ok(photos);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var photo = await this.repository.GetPhoto(id, cancellationToken);

            return Ok(photo);
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
        {
            var photo = await this.repository.GetPhoto(id, cancellationToken);

            if (photo == null)
            {
                return NotFound();
            }

            var image = await this.imageStore.GetImageBlobAsync(photo.FullSizeImageId, cancellationToken);

            return File(image.Content, "image/jpeg");
        }

        [HttpGet("{id}/thumbnail")]
        public async Task<IActionResult> Thumbnail(int id, CancellationToken cancellationToken)
        {
            var photo = await this.repository.GetPhoto(id, cancellationToken);

            if (photo == null)
            {
                return NotFound();
            }

            var image = await this.imageStore.GetImageBlobAsync(photo.ThumbnailImageId, cancellationToken);

            return File(image.Content, "image/jpeg");
        }
        
        [HttpGet("search/tag/{tag}")]
        public async Task<IActionResult> GetImagesByTag(string tag, CancellationToken cancellationToken)
        {
            var photos = await this.repository.GetPhotosByTag(tag, cancellationToken);

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

                    var thumbnailData = ImageManipulation.CreateThumbnailFrom(data);

                    var fullSizeBlob = new ImageBlob {Content = data, Id = Guid.NewGuid()};
                    var thumbnailBlob = new ImageBlob {Content = thumbnailData, Id = Guid.NewGuid()};
                    await this.imageStore.SaveImageBlobAsync(fullSizeBlob, cancellationToken);
                    await this.imageStore.SaveImageBlobAsync(thumbnailBlob, cancellationToken);

                    var photo = this.repository.StorePhoto(formFile.FileName, fullSizeBlob.Id, thumbnailBlob.Id);

                    photos.Add(photo);
                }
            }

            await this.unitOfwork.SaveChanges(cancellationToken);

            return RedirectToAction("Index", "Tag", new {files = photos.Select(_ => _.Id).ToList()});
        }
    }
}