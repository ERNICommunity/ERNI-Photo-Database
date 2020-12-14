using System.Drawing;

namespace ERNI.PhotoDatabase.Annotator.YoloParser
{
    public class YoloBoundingBox
    {
        public Dimensions Dimensions { get; set; }

        public string Label { get; set; }

        public float Confidence { get; set; }

        public RectangleF Rect
        {
            get { return new RectangleF(Dimensions.X1, Dimensions.Y1, Dimensions.Width, Dimensions.Height); }
        }

        public Color BoxColor { get; set; }
    }
}
