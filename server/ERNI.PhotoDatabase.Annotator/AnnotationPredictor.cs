using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Microsoft.ML;
using ERNI.PhotoDatabase.Annotator.YoloParser;
using ERNI.PhotoDatabase.Annotator.Utils;
using System.Drawing.Imaging;

namespace ERNI.PhotoDatabase.Annotator
{
    public class AnnotationPredictor
    {
        public (string[], byte[]) MakePrediction(Bitmap bmp)
        {
            MLContext mlContext = new MLContext();
            List<string> tags = new List<string>();
            var bmpWithBoxes = bmp.Clone(new RectangleF(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
            try
            {
                var modelScorer = new OnnxModelScorer(FileUtils.ModelFilePath, mlContext);
                var prediction = modelScorer.Score(bmp);

                YoloOutputParser parser = new YoloOutputParser();
                var boxes = parser.ParseOutputs(prediction);

                IList<YoloBoundingBox> detectedObjects = parser.NonMaxSuppression(boxes, 10, .5F); ;
                DrawBoundingBox(ref bmpWithBoxes, detectedObjects);

                tags = detectedObjects.Select(_ => _.Label).Distinct().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return (tags.ToArray(), bmpWithBoxes.ToByteArray(ImageFormat.Jpeg));
        }

        private static void DrawBoundingBox(ref Bitmap image, 
                                            IList<YoloBoundingBox> filteredBoundingBoxes)
        {
            var originalImageHeight = image.Height;
            var originalImageWidth = image.Width;

            foreach (var box in filteredBoundingBoxes)
            {
                var x = (uint)Math.Max(box.Dimensions.X1, 0);
                var y = (uint)Math.Max(box.Dimensions.Y1, 0);
                var width = (uint)Math.Min(originalImageWidth - x, box.Dimensions.Width);
                var height = (uint)Math.Min(originalImageHeight - y, box.Dimensions.Height);

                x = (uint)originalImageWidth * x / Yolov4ModelSettings.ImageSettings.imageWidth;
                y = (uint)originalImageHeight * y / Yolov4ModelSettings.ImageSettings.imageHeight;
                width = (uint)originalImageWidth * width / Yolov4ModelSettings.ImageSettings.imageWidth;
                height = (uint)originalImageHeight * height / Yolov4ModelSettings.ImageSettings.imageHeight;

                string text = $"{box.Label} ({box.Confidence * 100:0}%)";

                using (Graphics thumbnailGraphic = Graphics.FromImage(image))
                {
                    thumbnailGraphic.CompositingQuality = CompositingQuality.HighQuality;
                    thumbnailGraphic.SmoothingMode = SmoothingMode.HighQuality;
                    thumbnailGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    // Define Text Options
                    Font drawFont = new Font("Arial", 12, FontStyle.Bold);
                    SizeF size = thumbnailGraphic.MeasureString(text, drawFont);
                    SolidBrush fontBrush = new SolidBrush(Color.Black);
                    Point atPoint = new Point((int)x, (int)y - (int)size.Height - 1);

                    // Define BoundingBox options
                    Pen pen = new Pen(box.BoxColor, 3.2f);
                    SolidBrush colorBrush = new SolidBrush(box.BoxColor);

                    thumbnailGraphic.FillRectangle(colorBrush, (int)x, (int)(y - size.Height - 1), (int)size.Width, (int)size.Height);

                    thumbnailGraphic.DrawString(text, drawFont, fontBrush, atPoint);

                    // Draw bounding box on image
                    thumbnailGraphic.DrawRectangle(pen, x, y, width, height);
                }
            }
        }
    }
}
