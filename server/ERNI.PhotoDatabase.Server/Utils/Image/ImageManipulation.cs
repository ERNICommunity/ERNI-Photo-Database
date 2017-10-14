using System;
using System.IO;
using SkiaSharp;

namespace ERNI.PhotoDatabase.Server.Utils.Image
{
    public static class ImageManipulation
    {
        public static byte[] CreateThumbnailFrom(byte[] sourceImageData)
        {
            using (var ms = new MemoryStream(sourceImageData))
            using (var inputStream = new SKManagedStream(ms, false))
            using (var sourceBitmap = SKBitmap.Decode(inputStream))
            {
                var sourceSize = Math.Max(sourceBitmap.Width, sourceBitmap.Height);
                const int thumbnailSize = 640;
                var ratio = thumbnailSize / (double)sourceSize;

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
