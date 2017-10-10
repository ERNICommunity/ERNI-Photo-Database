using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace ERNI.PhotoDatabase.Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataProvider _dataProvider;

        public HomeController(DataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        [HttpGet]
        public IActionResult Index(string query)
        {
            var images = _dataProvider.Images.Where(i => i.Tags.Contains(query))
                .Select(i => new SearchResult {ImagePath = i.File})
                .ToArray();

            return View(images);
        }
    }
}