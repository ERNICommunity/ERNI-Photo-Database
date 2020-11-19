using Microsoft.ML.Transforms.Image;
using System.Drawing;

namespace ERNI.PhotoDatabase.Annotator.DataStructures
{
    class InputPicture
    {
        [ImageType(ImageSettings.imageHeight, ImageSettings.imageWidth)]
        public Bitmap Image { get; set; }
    }
}
