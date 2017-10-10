using Newtonsoft.Json;
using System.Collections.Generic;

namespace ERNI.PhotoDatabase.Server.Controllers
{
    public class DataProvider
    {
        public DataProvider()
        {
            var json = System.IO.File.ReadAllText(@"..\..\data\data.json");

            Images = JsonConvert.DeserializeObject<DataRoot>(json).Data;
        }

        public ICollection<Image> Images { get; }
    }
}
