using System.Collections.Generic;

namespace ERNI.PhotoDatabase.DataAccess.DomainModel
{
    public class Tag
    {
        public int Id { get; set; }
        public string Text { get; set; }

        public List<PhotoTag> PhotoTags { get; set; }
    }
}
