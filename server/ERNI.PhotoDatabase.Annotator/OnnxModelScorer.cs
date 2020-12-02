using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using ERNI.PhotoDatabase.Annotator.DataStructures;
using ERNI.PhotoDatabase.Annotator.YoloParser;
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

        public struct TinyYoloModelSettings
        {
            // input tensor name
            public const string ModelInput = "image";

            // output tensor name
            public const string ModelOutput = "grid";
        }

        private ITransformer LoadModel(string modelLocation)
        {
            var data = mlContext.Data.LoadFromEnumerable(new List<InputPicture>());

            var pipeline = mlContext.Transforms.ResizeImages(outputColumnName: "image", 
                                                            imageWidth: ImageSettings.imageWidth, 
                                                            imageHeight: ImageSettings.imageHeight, 
                                                            inputColumnName: nameof(InputPicture.Image))
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "image"))
                .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: modelLocation, 
                                                            outputColumnNames: new[] { TinyYoloModelSettings.ModelOutput }, 
                                                            inputColumnNames: new[] { TinyYoloModelSettings.ModelInput }));

            var model = pipeline.Fit(data);

            return model;
        }

        private float[] PredictDataUsingModel(Bitmap picture, ITransformer model)
        {
            var predictionEngine = mlContext.Model.CreatePredictionEngine<InputPicture, ImageNetPrediction>(model);
            var prediction = predictionEngine.Predict(new InputPicture { Image = picture });

            return prediction.PredictedLabels;
        }

        public float[] Score(Bitmap picture)
        {
            var model = LoadModel(modelLocation);

            return PredictDataUsingModel(picture, model);
        }
    }
}
