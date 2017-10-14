using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using ERNI.PhotoDatabase.Server.Obsolete;
using ERNI.PhotoDatabase.Server.Utils.Image;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using SkiaSharp;

namespace ERNI.PhotoDatabase.Server.Controllers
{

    [Route("api/[controller]")]
    public class PhotoController : Controller
    {
        private readonly DataProvider _dataProvider;

        public PhotoController(DataProvider dataProvider)
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
            return File(_dataProvider.Images.Single(i => i.File == id).Thumbnail, "image/jpeg");
        }

        [HttpGet("{id}/thumbnail")]
        public IActionResult Thumbnail(string id)
        {
            return File(_dataProvider.Images.Single(i => i.File == id).Thumbnail, "image/jpeg");
        }

        // GET api/values/5
        [HttpGet("search/tag/{tag}")]
        public IActionResult GetImagesByTag(string tag)
        {
            return Ok(_dataProvider.Images.Where(i => i.Tags.Contains(tag)));
        }
        
        // GET api/values
        /// <summary>
        /// Uploads the specified files.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Upload(List<IFormFile> files)
        {
            foreach (var formFile in files)
            {
                using (var openReadStream = formFile.OpenReadStream())
                {
                    var data = new byte[formFile.Length];
                    openReadStream.Read(data, 0, data.Length);

                    var thumbnailData = ImageManipulation.CreateThumbnailFrom(data);
                    
                    _dataProvider.Images.Add(new Image
                    {
                        File = formFile.FileName,
                        Content = data,
                        Tags = new[] { "office" },
                        Thumbnail = thumbnailData
                    });
                }
            }

            return RedirectToAction("Index", "Tag", new {files = files.Select(_ => _.FileName).ToArray()});
        }
    }
}

