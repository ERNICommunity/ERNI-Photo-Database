using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Microsoft.ML;
using ERNI.PhotoDatabase.Annotator.YoloParser;
using ERNI.PhotoDatabase.Annotator.DataStructures;
using ERNI.PhotoDatabase.Annotator.Utils;

namespace ERNI.PhotoDatabase.Annotator
{
    public class AnnotationPredictor
    {
        public Dictionary<string, string[]> MakePredictions()
        {
            MLContext mlContext = new MLContext();
            Dictionary<string, string[]> imagesTags = new Dictionary<string, string[]>();

            try
            {
                IEnumerable<ImageNetData> images = ImageNetData.ReadFromFile(FileUtils.ImagesFolder);
                IDataView imageDataView = mlContext.Data.LoadFromEnumerable(images);

                var modelScorer = new OnnxModelScorer(FileUtils.ImagesFolder, FileUtils.ModelFilePath, mlContext);

                // Use model to score data
                IEnumerable<float[]> probabilities = modelScorer.Score(imageDataView);

                YoloOutputParser parser = new YoloOutputParser();

                var boundingBoxes =
                    probabilities
                    .Select(probability => parser.ParseOutputs(probability))
                    .Select(boxes => parser.FilterBoundingBoxes(boxes, 5, .5F));

                for (var i = 0; i < images.Count(); i++)
                {
                    string imageFileName = images.ElementAt(i).Label;
                    IList<YoloBoundingBox> detectedObjects = boundingBoxes.ElementAt(i);

                    imagesTags.Add(imageFileName, detectedObjects.Select(_ => _.Label).ToArray());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return imagesTags;
        }

        public string[] MakePrediction(Bitmap bmp)
        {
            MLContext mlContext = new MLContext();
            Dictionary<string, string[]> imagesTags = new Dictionary<string, string[]>();
            List<string> tags = new List<string>();
            try
            {
                var modelScorer = new OnnxModelScorer2(FileUtils.ImagesFolder, FileUtils.ModelFilePath, mlContext);

                // Use model to score data
                float[] probability = modelScorer.Score(bmp);

                YoloOutputParser parser = new YoloOutputParser();

                var boxes = parser.ParseOutputs(probability);
                var boundingBoxes = parser.FilterBoundingBoxes(boxes, 5, .5F);

                string imageFileName = "imageName";
                IList<YoloBoundingBox> detectedObjects = boundingBoxes;

                imagesTags.Add(imageFileName, detectedObjects.Select(_ => _.Label).ToArray());
                tags = detectedObjects.Select(_ => _.Label).Distinct().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return tags.ToArray();
        }

        private static void DrawBoundingBox(string inputImageLocation, string outputImageLocation, string imageName, IList<YoloBoundingBox> filteredBoundingBoxes)
        {
            Image image = Image.FromFile(Path.Combine(inputImageLocation, imageName));

            var originalImageHeight = image.Height;
            var originalImageWidth = image.Width;

            foreach (var box in filteredBoundingBoxes)
            {
                var x = (uint)Math.Max(box.Dimensions.X, 0);
                var y = (uint)Math.Max(box.Dimensions.Y, 0);
                var width = (uint)Math.Min(originalImageWidth - x, box.Dimensions.Width);
                var height = (uint)Math.Min(originalImageHeight - y, box.Dimensions.Height);

                x = (uint)originalImageWidth * x / OnnxModelScorer.ImageNetSettings.imageWidth;
                y = (uint)originalImageHeight * y / OnnxModelScorer.ImageNetSettings.imageHeight;
                width = (uint)originalImageWidth * width / OnnxModelScorer.ImageNetSettings.imageWidth;
                height = (uint)originalImageHeight * height / OnnxModelScorer.ImageNetSettings.imageHeight;

                string text = $"{box.Label} ({(box.Confidence * 100).ToString("0")}%)";

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

                    if (!Directory.Exists(outputImageLocation))
                    {
                        Directory.CreateDirectory(outputImageLocation);
                    }

                    image.Save(Path.Combine(outputImageLocation, imageName));
                }
            }
        }

        private static void LogDetectedObjects(string imageName, IList<YoloBoundingBox> boundingBoxes)
        {
            Console.WriteLine($".....The objects in the image {imageName} are detected as below....");

            foreach (var box in boundingBoxes)
            {
                Console.WriteLine($"{box.Label} and its Confidence score: {box.Confidence}");
            }

            Console.WriteLine("");
        }
    }
}
