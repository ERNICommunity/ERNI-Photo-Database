using System.Collections.Generic;
using System.Linq;
using ERNI.PhotoDatabase.Server.Utils.Image;
using Newtonsoft.Json;

namespace ERNI.PhotoDatabase.Server.Obsolete
{
    public class DataProvider
    {
        public DataProvider()
        {
            var json = System.IO.File.ReadAllText(@"data/data.json");

            Images = JsonConvert.DeserializeObject<DataRoot>(json).Data.ToList();

            foreach (var image in Images)
            {
                image.Content = System.IO.File.ReadAllBytes(@"data/" + image.File);
                image.Thumbnail = ImageManipulation.CreateThumbnailFrom(image.Content);
            }
        }

        public ICollection<Image> Images { get; }
    }
}
