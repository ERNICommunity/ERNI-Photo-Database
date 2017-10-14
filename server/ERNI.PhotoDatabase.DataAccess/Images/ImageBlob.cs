using System;
using System.Collections.Generic;
using System.Text;

namespace ERNI.PhotoDatabase.DataAccess.Images
{
    public class ImageBlob
    {
        public string Id { get; set; }

        public byte[] Content { get; set; }
    }
}
