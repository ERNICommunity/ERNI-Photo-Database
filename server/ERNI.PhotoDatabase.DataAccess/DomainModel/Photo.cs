using System;
using System.Collections.Generic;

namespace ERNI.PhotoDatabase.DataAccess.DomainModel
{
    public class Photo
    {
        public int Id { get; set;}

        public string Name { get; set; }

        public Guid FullSizeImageId { get; set; }

        public Guid ThumbnailImageId { get; set; }

        public List<PhotoTag> PhotoTags { get; set; }

        public string Mime { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
