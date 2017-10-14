using System.Collections.Generic;

namespace ERNI.PhotoDatabase.DataAccess.DomainModel
{
    public class Photo
    {
        public int Id { get; set;}
        public string Name { get; set; }

        public List<PhotoTag> PhotoTag { get; set; }
    }
}
