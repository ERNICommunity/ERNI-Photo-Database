using System;
using System.Linq;
using ERNI.PhotoDatabase.Server.Model;
using ERNI.PhotoDatabase.Server.Obsolete;
using Microsoft.AspNetCore.Mvc;

namespace ERNI.PhotoDatabase.Server.Controllers
{
    public class TagController : Controller
    {
        private readonly DataProvider provider;

        public TagController(DataProvider provider)
        {
            this.provider = provider;
        }

        [HttpGet]
        public IActionResult Index(string[] files)
        {
            var f = this.provider.Images.Where(_ => files.Contains(_.File))
                .Select(_ => new ImageDescription {Filename = _.File}).ToArray();

            return View(new UploadResult {Images = f});
        }

        [HttpPost]
        public IActionResult Save([FromForm]UploadResult taggedResults)
        {
            foreach (var image in taggedResults.Images)
            {
                var match = this.provider.Images.SingleOrDefault(_ => _.File == image.Filename);

                if (match == null)
                {
                    continue;
                }

                match.Tags = image.Tags?.Split(new[] {' ', ',', ';'}, StringSplitOptions.RemoveEmptyEntries);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}