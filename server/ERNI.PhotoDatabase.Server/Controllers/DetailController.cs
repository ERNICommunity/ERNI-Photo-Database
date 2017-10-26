using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERNI.PhotoDatabase.DataAccess.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ERNI.PhotoDatabase.Server.Controllers
{
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

            return View(new SearchResult
            {
                Id = photo.Id.ToString(),
                Name = photo.Name,
                Tags = photo.PhotoTags.Select(_ => _.Tag.Text).ToArray()
            });
        }
    }
}