using System;
using System.IO;
using System.Linq;
using SkiaSharp;

namespace ERNI.PhotoDatabase.Server.Utils.Image
{
    public class ImageManipulation : IImageManipulation
    {
        private SKBitmap RotateAndFlip(SKBitmap original, SKCodecOrigin origin)
        {
            // these are the origins that represent a 90 degree turn in some fashion
            var differentOrientations = new[]
            {
                SKCodecOrigin.LeftBottom,
                SKCodecOrigin.LeftTop,
                SKCodecOrigin.RightBottom,
                SKCodecOrigin.RightTop
            };

            // check if we need to turn the image
            var isDifferentOrientation = differentOrientations.Any(o => o == origin);

            // define new width/height
            var width = isDifferentOrientation ? original.Height : original.Width;
            var height = isDifferentOrientation ? original.Width : original.Height;

            var bitmap = new SKBitmap(width, height, original.AlphaType == SKAlphaType.Opaque);

            // todo: the stuff in this switch statement should be rewritten to use pointers
            switch (origin)
            {
                case SKCodecOrigin.LeftBottom:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(y, original.Width - 1 - x, original.GetPixel(x, y));
                    break;

                case SKCodecOrigin.RightTop:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(original.Height - 1 - y, x, original.GetPixel(x, y));
                    break;

                case SKCodecOrigin.RightBottom:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(original.Height - 1 - y, original.Width - 1 - x, original.GetPixel(x, y));

                    break;

                case SKCodecOrigin.LeftTop:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(y, x, original.GetPixel(x, y));
                    break;

                case SKCodecOrigin.BottomLeft:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(x, original.Height - 1 - y, original.GetPixel(x, y));
                    break;

                case SKCodecOrigin.BottomRight:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(original.Width - 1 - x, original.Height - 1 - y, original.GetPixel(x, y));
                    break;

                case SKCodecOrigin.TopRight:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(original.Width - 1 - x, y, original.GetPixel(x, y));
                    break;

            }

            return bitmap;
        }

        public byte[] ResizeTo(byte[] sourceImageData, int size)
        {
            using (var ms = new MemoryStream(sourceImageData))
            using (var inputStream = new SKManagedStream(ms, false))
            using (var codec = SKCodec.Create(inputStream))
            using (var bmp = SKBitmap.Decode(codec))
            using (var sourceBitmap = RotateAndFlip(bmp, codec.Origin))
            {
                var sourceSize = Math.Max(sourceBitmap.Width, sourceBitmap.Height);
                var ratio = size / (double)sourceSize;

                using (var targetBitmap = new SKBitmap(new SKImageInfo((int)(sourceBitmap.Width * ratio), (int)(sourceBitmap.Height * ratio))))
                using (var targetMemoryStream = new MemoryStream())
                using (var thumbnailStream = new SKManagedWStream(targetMemoryStream))
                {
                    sourceBitmap.Resize(targetBitmap, SKBitmapResizeMethod.Lanczos3);
                    targetBitmap.Encode(thumbnailStream, SKEncodedImageFormat.Jpeg, 85);

                    thumbnailStream.Flush();
                    return targetMemoryStream.ToArray();
                }
            }
        }

        public (int Width, int Height) GetSize(byte[] imageData)
        {
            using (var ms = new MemoryStream(imageData))
            using (var inputStream = new SKManagedStream(ms, false))
            using (var sourceBitmap = SKBitmap.Decode(inputStream))
            {
                return (sourceBitmap.Width, sourceBitmap.Height);
            }
        }
    }
}
