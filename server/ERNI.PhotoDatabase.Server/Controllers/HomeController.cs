﻿using System.Linq;
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
            var data = tagRepository.GetMostUsedTags(cancellationToken);

            return View(data.Select(_ => (Text: _.Text, Count: _.PhotoTags.Count)).OrderByDescending(_ => _.Count).Take(10));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SearchResult(string query, CancellationToken cancellationToken)
        {
            var images = await photoRepository.SearchPhotos(query.ToLowerInvariant().Split(" "), cancellationToken);

            return View(images.Select(_ => new PhotoModel
            {
                Id = _.Id.ToString(),
                Name = _.Name,
                Tags = _.PhotoTags.Select(__ => __.Tag.Text).ToArray(),
                Width = _.Width,
                Height = _.Height,
                ThumbnailUrl = Url.Action("Thumbnail", "Photo", new { id = _.Id }),
                DetailUrl = Url.Action("Index", "Detail", new { id = _.Id })
            }).ToArray());
        }

        [HttpGet("browse/{page?}")]
        public async Task<IActionResult> Browse(int? page, CancellationToken cancellationToken)
        {
            var photosPerPage = 15;

            page = page ?? 1;

            var photos = await photoRepository.GetPhotos(photosPerPage, (page.Value - 1) * photosPerPage, cancellationToken);

            ViewBag.Page = page;

            return View(photos.Select(_ => new PhotoModel
            {
                Id = _.Id.ToString(),
                Name = _.Name,
                Tags = _.PhotoTags.Select(__ => __.Tag.Text).ToArray(),
                Width = _.Width,
                Height = _.Height,
                ThumbnailUrl = Url.Action("Thumbnail", "Photo", new { id = _.Id }),
                DetailUrl = Url.Action("Index", "Detail", new { id = _.Id })
            }).ToArray());
        }

        [HttpGet]
        [Authorize(Roles = "uploader")]
        public IActionResult Upload()
        {
            return View();
        }
    }
}