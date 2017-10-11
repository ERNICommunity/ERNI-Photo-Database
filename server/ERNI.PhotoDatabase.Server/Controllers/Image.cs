namespace ERNI.PhotoDatabase.Server.Controllers
{
    public class Image
    {
        public string File { get; set; }

        public string[] Tags { get; set; }

        public byte[] Content { get; set; }

        public byte[] Thumbnail { get; set; }
    }
}
