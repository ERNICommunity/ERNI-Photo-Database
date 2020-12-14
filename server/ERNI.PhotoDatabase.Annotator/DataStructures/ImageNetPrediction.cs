using Microsoft.ML.Data;

namespace ERNI.PhotoDatabase.Annotator.DataStructures
{
    public class ImageNetPrediction
    {
        [VectorType(1, 52, 52, 3, 85)]
        [ColumnName(Yolov4ModelSettings.Output_1)]
        public float[] Output_1;

        [VectorType(1, 26, 26, 3, 85)]
        [ColumnName(Yolov4ModelSettings.Output_2)]
        public float[] Output_2;

        [VectorType(1, 52, 52, 3, 85)]
        [ColumnName(Yolov4ModelSettings.Output_3)]
        public float[] Output_3;

        [ColumnName("width")]
        public float ImageWidth { get; set; }

        [ColumnName("height")]
        public float ImageHeight { get; set; }
    }
}
