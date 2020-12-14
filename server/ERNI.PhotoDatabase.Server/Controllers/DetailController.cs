using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERNI.PhotoDatabase.DataAccess.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;

namespace ERNI.PhotoDatabase.Server.Controllers
{
    [Authorize]
    public class DetailController : Controller
    {
        private readonly IPhotoRepository _repository;

        public DetailController(IPhotoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index(int id, CancellationToken cancellationToken)
        {
            var photo = await _repository.GetPhoto(id, cancellationToken);

            if (photo == null)
            {
                return NotFound();
            }

            return View(new PhotoModel
            {
                Id = photo.Id.ToString(),
                Name = photo.Name,
                Tags = photo.PhotoTags.Select(_ => _.Tag.Text).ToArray(),
                Width = photo.Width,
                Height = photo.Height,
                ThumbnailUrl = Url.Action("Thumbnail", "Photo", new { id = photo.Id }),
                HasAnnotationLayer = photo.TaggedThumbnailImageId != Guid.Empty,
                TaggedThumbnailUrl = Url.Action("Thumbnail", "Photo", new { id = photo.Id, withOverlay = true }),
                DetailUrl = Url.Action("Index", "Detail", new { id = photo.Id })
            });
        }
    }
}