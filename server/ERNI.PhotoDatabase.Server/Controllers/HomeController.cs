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
        public IActionResult Index()
        {
            var data = _dataProvider.Images
                .SelectMany(i => i.Tags)
                .GroupBy(t => t)
                .Select(grp => (Tag: grp.Key, Count: grp.Count()))
                .ToList();

            return View(data);
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            var images = _dataProvider.Images.Where(i => i.Tags.Contains(query))
                .Select(i => new SearchResult {ImagePath = i.File, Tags = i.Tags})
                .ToArray();

            return View(images);
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }
    }
}