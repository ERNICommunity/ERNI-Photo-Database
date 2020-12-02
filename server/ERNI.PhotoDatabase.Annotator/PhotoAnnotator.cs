using System.Drawing;
using System.IO;

namespace ERNI.PhotoDatabase.Annotator
{
    public class PhotoAnnotator
    {
        public (string[], byte[]) AnnotatePhoto(byte[] photoData, string fileName)
        {
            Bitmap bmp;
            string[] tags;
            byte[] pictureData;

            using (var ms = new MemoryStream(photoData))
            {
                bmp = new Bitmap(ms);
                var predictor = new AnnotationPredictor();
                (tags, pictureData) = predictor.MakePrediction(bmp, fileName);
            }
            return (tags, pictureData);
        }
    }
}
