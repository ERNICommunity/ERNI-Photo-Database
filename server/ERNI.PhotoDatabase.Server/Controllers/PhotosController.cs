using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;

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

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
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
    }
}
