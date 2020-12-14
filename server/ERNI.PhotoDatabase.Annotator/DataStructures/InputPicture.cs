using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;
using System.Drawing;

namespace ERNI.PhotoDatabase.Annotator.DataStructures
{
    public class InputPicture
    {
        [ColumnName("bitmap")]
        [ImageType(Yolov4ModelSettings.ImageSettings.imageHeight, Yolov4ModelSettings.ImageSettings.imageWidth)]
        public Bitmap Image { get; set; }

        [ColumnName("width")]
        public float ImageWidth => Image.Width;

        [ColumnName("height")]
        public float ImageHeight => Image.Height;
    }
}
