using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERNI.PhotoDatabase.DataAccess.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace ERNI.PhotoDatabase.Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPhotoRepository photoRepository;
        private readonly ITagRepository tagRepository;

        public HomeController(IPhotoRepository photoRepository, ITagRepository tagRepository)
        {
            this.photoRepository = photoRepository;
            this.tagRepository = tagRepository;
        }

        [HttpGet]
        public IActionResult Index(CancellationToken cancellationToken)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(HomeController.Search), "Home");
            }

            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(CancellationToken cancellationToken)
        {
            var data = await tagRepository.GetMostUsedTags(cancellationToken);

            return View(data.Select(_ => (_.Text, _.PhotoTags.Count)));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SearchResult(string query, CancellationToken cancellationToken)
        {
            var images = await photoRepository.GetPhotosByTag(query, cancellationToken);

            return View(images.Select(_ => new SearchResult
            {
                Id = _.Id.ToString(),
                Name = _.Name,
                Tags = _.PhotoTags.Select(__ => __.Tag.Text).ToArray(),
                Width = _.Width,
                Height = _.Height
            }).ToArray());
        }

        [HttpGet]
        [Authorize]
        public IActionResult Upload()
        {
            return View();
        }
    }
}