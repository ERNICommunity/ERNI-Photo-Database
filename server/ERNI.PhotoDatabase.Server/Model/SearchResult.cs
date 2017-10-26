namespace ERNI.PhotoDatabase.Server.Controllers
{
    public class SearchResult
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string[] Tags { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}