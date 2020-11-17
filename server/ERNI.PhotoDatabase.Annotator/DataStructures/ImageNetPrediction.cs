using Microsoft.ML.Data;

namespace ERNI.PhotoDatabase.Annotator.DataStructures
{
    public class ImageNetPrediction
    {
        [ColumnName("grid")]
        public float[] PredictedLabels;
    }
}
