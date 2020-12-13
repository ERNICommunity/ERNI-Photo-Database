namespace ERNI.PhotoDatabase.Annotator.YoloParser
{
    public class Dimensions
    {
        public float X1 { get; set; }
        public float Y1 { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }

        public float Width 
        {
            get => X2 - X1;
        }

        public float Height
        {
            get => Y2 - Y1;
        }
    }
}
