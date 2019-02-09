using System;
using System.IO;
using System.Linq;
using SkiaSharp;

namespace ERNI.PhotoDatabase.Server.Utils.Image
{
    public class ImageManipulation : IImageManipulation
    {
        private SKBitmap RotateAndFlip(SKBitmap original, SKEncodedOrigin origin)
        {
            // these are the origins that represent a 90 degree turn in some fashion
            var differentOrientations = new[]
            {
                SKEncodedOrigin.LeftBottom,
                SKEncodedOrigin.LeftTop,
                SKEncodedOrigin.RightBottom,
                SKEncodedOrigin.RightTop
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
                case SKEncodedOrigin.LeftBottom:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(y, original.Width - 1 - x, original.GetPixel(x, y));
                    break;

                case SKEncodedOrigin.RightTop:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(original.Height - 1 - y, x, original.GetPixel(x, y));
                    break;

                case SKEncodedOrigin.RightBottom:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(original.Height - 1 - y, original.Width - 1 - x, original.GetPixel(x, y));

                    break;

                case SKEncodedOrigin.LeftTop:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(y, x, original.GetPixel(x, y));
                    break;

                case SKEncodedOrigin.BottomLeft:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(x, original.Height - 1 - y, original.GetPixel(x, y));
                    break;

                case SKEncodedOrigin.BottomRight:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(original.Width - 1 - x, original.Height - 1 - y, original.GetPixel(x, y));
                    break;

                case SKEncodedOrigin.TopRight:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(original.Width - 1 - x, y, original.GetPixel(x, y));
                    break;

            }

            original.Dispose();

            return bitmap;
        }

        public byte[] ResizeTo(byte[] sourceImageData, int size)
        {
            using (var ms = new MemoryStream(sourceImageData))
            using (var inputStream = new SKManagedStream(ms, false))
            using (var codec = SKCodec.Create(inputStream))
            {
                var sourceBitmap = SKBitmap.Decode(codec);

                if (codec.EncodedOrigin != SKEncodedOrigin.TopLeft)
                {
                    sourceBitmap = RotateAndFlip(sourceBitmap, codec.EncodedOrigin);
                }

                var sourceSize = Math.Max(sourceBitmap.Width, sourceBitmap.Height);
                var ratio = size / (double)sourceSize;

                using (var targetBitmap = new SKBitmap(new SKImageInfo((int)(sourceBitmap.Width * ratio), (int)(sourceBitmap.Height * ratio))))
                using (var targetMemoryStream = new MemoryStream())
                using (var thumbnailStream = new SKManagedWStream(targetMemoryStream))
                {
                    sourceBitmap.ScalePixels(targetBitmap, SKFilterQuality.High);
                    
                    SKPixmap.Encode(thumbnailStream, targetBitmap, SKEncodedImageFormat.Jpeg, 85);
                    thumbnailStream.Flush();

                    sourceBitmap.Dispose();
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
