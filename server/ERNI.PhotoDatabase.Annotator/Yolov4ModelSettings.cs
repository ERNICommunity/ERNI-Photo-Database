using System.Collections.Generic;

namespace ERNI.PhotoDatabase.Annotator
{
    public class Yolov4ModelSettings
    {
        public const string Input = "input_1:0";

        public const string Output_1 = "Identity:0";
        public const string Output_2 = "Identity_1:0";
        public const string Output_3 = "Identity_2:0";

        // input tensor name
        public static readonly string[] ModelInputName = new[] { Input };

        // output tensors name
        public static readonly string[] ModelOutputNames = new[]
        {
            Output_1,
            Output_2,
            Output_3
        };

        public static readonly Dictionary<string, int[]> Shape = new Dictionary<string, int[]>()
        {
            { Input, new[] { 1, 416, 416, 3 } },
            { Output_1, new[] { 1, 52, 52, 3, 85 } },
            { Output_2, new[] { 1, 26, 26, 3, 85 } },
            { Output_3, new[] { 1, 13, 13, 3, 85 } },
        };

        public struct ImageSettings
        {
            public const int imageHeight = 416;
            public const int imageWidth = 416;
        }
    }
}
