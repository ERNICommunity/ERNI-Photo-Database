using ERNI.PhotoDatabase.Annotator.DataStructures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ERNI.PhotoDatabase.Annotator.YoloParser
{
    public class YoloOutputParser
    {
        // https://github.com/hunglc007/tensorflow-yolov4-tflite/blob/master/data/anchors/yolov4_anchors.txt
        static readonly float[][][] ANCHORS = new float[][][]
        {
            new float[][] { new float[] { 12, 16 }, new float[] { 19, 36 }, new float[] { 40, 28 } },
            new float[][] { new float[] { 36, 75 }, new float[] { 76, 55 }, new float[] { 72, 146 } },
            new float[][] { new float[] { 142, 110 }, new float[] { 192, 243 }, new float[] { 459, 401 } }
        };
        static readonly float[] STRIDES = new float[] { 8, 16, 32 };
        static readonly float[] XYSCALE = new float[] { 1.2f, 1.1f, 1.05f };
        static readonly int[] shapes = new int[] { 52, 26, 13 };
        const int anchorsCount = 3;

        private string[] labels = new string[] {
            "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train",
            "truck", "boat", "traffic light", "fire hydrant", "stop sign",
            "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep",
            "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella",
            "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard",
            "sports ball", "kite", "baseball bat", "baseball glove", "skateboard",
            "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork",
            "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange",
            "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair",
            "sofa", "pottedplant", "bed", "diningtable", "toilet", "tvmonitor",
            "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave",
            "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase",
            "scissors", "teddy bear", "hair drier", "toothbrush" };

        private static Color[] classColors = new Color[]
        {
            Color.Khaki,
            Color.Fuchsia,
            Color.Silver,
            Color.RoyalBlue,
            Color.Green,
            Color.DarkOrange,
            Color.Purple,
            Color.Gold,
            Color.Red,
            Color.Aquamarine,
            Color.Lime,
            Color.AliceBlue,
            Color.Sienna,
            Color.Orchid,
            Color.Tan,
            Color.LightPink,
            Color.Yellow,
            Color.HotPink,
            Color.OliveDrab,
            Color.SandyBrown,
            Color.DarkTurquoise
        };

        private Dimensions ExtractBoundingBoxDimensions(float[] offsetModelOutput, int row, int column, float xyScale, float stride, float[] anchor)
        {
            var rawDx = offsetModelOutput[0];
            var rawDy = offsetModelOutput[1];
            var rawDw = offsetModelOutput[2];
            var rawDh = offsetModelOutput[3];

            float predX = ((Sigmoid(rawDx) * xyScale) - 0.5f * (xyScale - 1) + row) * stride;
            float predY = ((Sigmoid(rawDy) * xyScale) - 0.5f * (xyScale - 1) + column) * stride;
            float predW = (float)Math.Exp(rawDw) * anchor[0];
            float predH = (float)Math.Exp(rawDh) * anchor[1];

            // postprocess_boxes
            // (1) (x, y, w, h) --> (xmin, ymin, xmax, ymax)
            float predX1 = predX - predW * 0.5f;
            float predY1 = predY - predH * 0.5f;
            float predX2 = predX + predW * 0.5f;
            float predY2 = predY + predH * 0.5f;

            // (2) (xmin, ymin, xmax, ymax) -> (xmin_org, ymin_org, xmax_org, ymax_org)
            float org_h = Yolov4ModelSettings.ImageSettings.imageHeight;
            float org_w = Yolov4ModelSettings.ImageSettings.imageWidth;

            float inputSize = 416f;
            float resizeRatio = Math.Min(inputSize / org_w, inputSize / org_h);
            float dw = (inputSize - resizeRatio * org_w) / 2f;
            float dh = (inputSize - resizeRatio * org_h) / 2f;

            var orgX1 = 1f * (predX1 - dw) / resizeRatio; // left
            var orgX2 = 1f * (predX2 - dw) / resizeRatio; // right
            var orgY1 = 1f * (predY1 - dh) / resizeRatio; // top
            var orgY2 = 1f * (predY2 - dh) / resizeRatio; // bottom

            // (3) clip some boxes that are out of range
            orgX1 = Math.Max(orgX1, 0);
            orgY1 = Math.Max(orgY1, 0);
            orgX2 = Math.Min(orgX2, org_w - 1);
            orgY2 = Math.Min(orgY2, org_h - 1);

            return new Dimensions
            {
                X1 = orgX1,
                X2 = orgX2,
                Y1 = orgY1,
                Y2 = orgY2
            };
        }

        private float IntersectionOverUnion(RectangleF boundingBoxA, RectangleF boundingBoxB)
        {
            var areaA = boundingBoxA.Width * boundingBoxA.Height;
            var areaB = boundingBoxB.Width * boundingBoxB.Height;

            if (areaA <= 0 || areaB <= 0)
                return 0;

            var minX = Math.Max(boundingBoxA.Left, boundingBoxB.Left);
            var minY = Math.Max(boundingBoxA.Top, boundingBoxB.Top);
            var maxX = Math.Min(boundingBoxA.Right, boundingBoxB.Right);
            var maxY = Math.Min(boundingBoxA.Bottom, boundingBoxB.Bottom);

            var intersectionArea = Math.Max(maxY - minY, 0) * Math.Max(maxX - minX, 0);

            return intersectionArea / (areaA + areaB - intersectionArea);
        }

        public IList<YoloBoundingBox> ParseOutputs(ImageNetPrediction prediction, float threshold = .3F)
        {
            // ported from https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov4#postprocessing-steps
            List<float[]> postProcesssedResults = new List<float[]>();
            int classesCount = labels.Length;
            // YOLOv4 outputs from 3 different levels (different object scales) 
            var results = new[] { prediction.Output_1, prediction.Output_2, prediction.Output_3 };

            var boxes = new List<YoloBoundingBox>();
            for (int i = 0; i < results.Length; i++)
            {
                var result = results[i];
                var outputSize = shapes[i];

                for (int boxY = 0; boxY < outputSize; boxY++)
                {
                    for (int boxX = 0; boxX < outputSize; boxX++)
                    {
                        for (int a = 0; a < anchorsCount; a++)
                        {
                            var offset = (boxY * outputSize * (classesCount + 5) * anchorsCount)
                                        + (boxX * (classesCount + 5) * anchorsCount)
                                        + a * (classesCount + 5);
                            var offsetPredictions = result.Skip(offset).Take(classesCount + 5).ToArray();

                            var confidence = offsetPredictions[4];
                            var predClasses = offsetPredictions.Skip(5).ToArray();
                            var boundingBox = ExtractBoundingBoxDimensions(offsetPredictions, boxX, boxY,
                                XYSCALE[i], STRIDES[i],
                                ANCHORS[i][a]);
                            if (boundingBox.X1 > boundingBox.X2
                                || boundingBox.Y1 > boundingBox.Y2)
                            {
                                continue;
                            }

                            var scores = predClasses.Select(p => p * confidence).ToList();
                            float topScore = scores.Max();
                            if (topScore < threshold)
                                continue;

                            boxes.Add(new YoloBoundingBox()
                            {
                                Dimensions = boundingBox,
                                Confidence = topScore,
                                Label = labels[scores.IndexOf(topScore)],
                                BoxColor = classColors[scores.IndexOf(topScore) % (classColors.Length - 1)]
                            });
                        }
                    }
                }
            }

            return boxes;
        }
        
        private float Sigmoid(float value)
        {
            var k = (float)Math.Exp(value);
            return k / (1.0f + k);
        }

        public IList<YoloBoundingBox> NonMaxSuppression(IList<YoloBoundingBox> boxes, int limit, float threshold)
        {
            var activeCount = boxes.Count;
            var isActiveBoxes = new bool[boxes.Count];

            for (int i = 0; i < isActiveBoxes.Length; i++)
                isActiveBoxes[i] = true;

            var sortedBoxes = boxes.Select((b, i) => new { Box = b, Index = i })
                    .OrderByDescending(b => b.Box.Confidence)
                    .ToList();

            var results = new List<YoloBoundingBox>();

            for (int i = 0; i < boxes.Count; i++)
            {
                if (isActiveBoxes[i])
                {
                    var boxA = sortedBoxes[i].Box;
                    results.Add(boxA);

                    if (results.Count >= limit)
                        break;

                    for (var j = i + 1; j < boxes.Count; j++)
                    {
                        if (isActiveBoxes[j])
                        {
                            var boxB = sortedBoxes[j].Box;

                            if (IntersectionOverUnion(boxA.Rect, boxB.Rect) > threshold)
                            {
                                isActiveBoxes[j] = false;
                                activeCount--;

                                if (activeCount <= 0)
                                    break;
                            }
                        }
                    }
                    if (activeCount <= 0)
                        break;
                }
            }

            return results;
        }
    }
}
