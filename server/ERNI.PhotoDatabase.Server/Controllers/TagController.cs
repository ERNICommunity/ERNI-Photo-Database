﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ERNI.PhotoDatabase.DataAccess.Repository;
using ERNI.PhotoDatabase.DataAccess.UnitOfWork;
using ERNI.PhotoDatabase.Server.Model;
using Microsoft.AspNetCore.Mvc;

namespace ERNI.PhotoDatabase.Server.Controllers
{
    public class TagController : Controller
    {
        private readonly ITagRepository tagRepository;
        private readonly IPhotoRepository phootRepository;
        private readonly IUnitOfWork unitOfWork;

        public TagController(ITagRepository tagRepository, IPhotoRepository photoRepository, IUnitOfWork unitOfWork)
        {
            this.tagRepository = tagRepository;
            this.phootRepository = photoRepository;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int[] fileIds, CancellationToken cancellationToken)
        {
            var uploadedFiles = await this.phootRepository.GetPhotos(fileIds, cancellationToken);

            return this.View(new UploadResult {Images = uploadedFiles.Select(p => new ImageDescription {
                Id = p.Id,
                Name = p.Name,
                Tags = string.Join(", ", p.PhotoTags.Select(t => t.Tag.Text))
            }).ToArray()});
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromForm]UploadResult taggedResults, CancellationToken cancellationToken)
        {
            await SaveTags(taggedResults, cancellationToken);

            return this.RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> SaveOne([FromForm]UploadResult taggedResults, CancellationToken cancellationToken)
        {
            if (taggedResults.Images.Length != 1)
            {
                return StatusCode((int)HttpStatusCode.BadRequest);
            }

            await SaveTags(taggedResults, cancellationToken);

            return this.RedirectToAction("Index", "Detail", new { id = taggedResults.Images.Single().Id });
        }

        private async Task SaveTags(UploadResult taggedResults, CancellationToken cancellationToken)
        {
            using (var t = await this.unitOfWork.BeginTransaction(cancellationToken))
            {
                foreach (var image in taggedResults.Images)
                {
                    var tags = image.Tags?.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    await this.tagRepository.SetTagsForImage(image.Id, tags, cancellationToken);

                    await this.unitOfWork.SaveChanges(cancellationToken);
                }

                t.Commit();
            }
        }
    }
}