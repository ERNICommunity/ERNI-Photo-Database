namespace ERNI.PhotoDatabase.Server.Controllers
{
    public class PhotoModel
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public string[] Tags { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string DetailUrl { get; set; }

        public string ThumbnailUrl { get; set; }
        
        public string TaggedThumbnailUrl { get; set; }
    }
}