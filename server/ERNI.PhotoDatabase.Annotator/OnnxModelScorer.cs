using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;

using ERNI.PhotoDatabase.Annotator.DataStructures;
using System.Drawing;

namespace ERNI.PhotoDatabase.Annotator
{
    public class OnnxModelScorer
    {
        private readonly string modelLocation;
        private readonly MLContext mlContext;

        public OnnxModelScorer(string modelLocation, MLContext mlContext)
        {
            this.modelLocation = modelLocation;
            this.mlContext = mlContext;
        }

        private ITransformer LoadModel(string modelLocation)
        {
            var data = mlContext.Data.LoadFromEnumerable(new List<InputPicture>());

            var pipeline = mlContext.Transforms.ResizeImages(outputColumnName: Yolov4ModelSettings.Input,
                                                            imageWidth: Yolov4ModelSettings.ImageSettings.imageWidth,
                                                            imageHeight: Yolov4ModelSettings.ImageSettings.imageHeight,
                                                            inputColumnName: "bitmap",
                                                            resizing: Microsoft.ML.Transforms.Image.ImageResizingEstimator.ResizingKind.IsoPad)
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: Yolov4ModelSettings.Input,
                                                            scaleImage: 1f / 255f, interleavePixelColors: true))
                .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: modelLocation,
                                                            outputColumnNames: Yolov4ModelSettings.ModelOutputNames,
                                                            inputColumnNames: Yolov4ModelSettings.ModelInputName,
                                                            shapeDictionary: Yolov4ModelSettings.Shape));

            var model = pipeline.Fit(data);

            return model;
        }

        private ImageNetPrediction PredictDataUsingModel(Bitmap picture, ITransformer model)
        {
            var predictionEngine = mlContext.Model.CreatePredictionEngine<InputPicture, ImageNetPrediction>(model);
            var prediction = predictionEngine.Predict(new InputPicture { Image = picture });

            return prediction;
        }

        public ImageNetPrediction Score(Bitmap picture)
        {
            var model = LoadModel(modelLocation);

            return PredictDataUsingModel(picture, model);
        }
    }
}
