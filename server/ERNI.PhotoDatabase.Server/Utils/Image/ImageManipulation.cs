using System;
using System.IO;
using SkiaSharp;

namespace ERNI.PhotoDatabase.Server.Utils.Image
{
    public class ImageManipulation : IImageManipulation
    {
        public byte[] ResizeTo(byte[] sourceImageData, int size)
        {
            using (var ms = new MemoryStream(sourceImageData))
            using (var inputStream = new SKManagedStream(ms, false))
            using (var sourceBitmap = SKBitmap.Decode(inputStream))
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
    }
}
