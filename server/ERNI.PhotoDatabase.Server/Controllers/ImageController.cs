using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace ERNI.PhotoDatabase.Server.Controllers
{

    [Route("api/[controller]")]
    public class ImageController : Controller
    {
        private readonly DataProvider _dataProvider;

        public ImageController(DataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_dataProvider.Images);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            return Ok(_dataProvider.Images.Single(i => i.File == id));
        }

        [HttpGet("{id}/download")]
        public IActionResult Download(string id)
        {
            return File(_dataProvider.Images.Single(i => i.File == id).Content, "image/jpeg");
        }

        // GET api/values/5
        [HttpGet("search/tag/{tag}")]
        public IActionResult GetImagesByTag(string tag)
        {
            return Ok(_dataProvider.Images.Where(i => i.Tags.Contains(tag)));
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        // GET api/values
        [HttpPost]
        public IActionResult Upload(List<IFormFile> files)
        {
            foreach (var formFile in files)
            {
                using (var openReadStream = formFile.OpenReadStream())
                {
                    var data = new byte[formFile.Length];
                    openReadStream.Read(data, 0, data.Length);
                    _dataProvider.Images.Add(new Image
                    {
                        File = formFile.Name,
                        Content = data,
                        Tags = new[] {"office"}
                    });
                }
            }
            
            return RedirectToAction("Index", "Home");
        }
    }
}
