using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERNI.PhotoDatabase.DataAccess.Repository;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var data = await this.tagRepository.GetMostUsedTags(cancellationToken);

            return View(data.Select(_ => (_.Text, _.PhotoTag.Count)));
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query, CancellationToken cancellationToken)
        {
            var images = await this.photoRepository.GetPhotosByTag(query, cancellationToken);

            return View(images.Select(_ =>
                    new SearchResult {Id = _.Id.ToString(), Name = _.Name, Tags = _.PhotoTag.Select(__ => __.Tag.Text).ToArray()})
                .ToArray());
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }
    }
}