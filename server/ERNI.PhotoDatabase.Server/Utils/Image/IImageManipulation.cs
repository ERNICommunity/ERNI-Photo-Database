namespace ERNI.PhotoDatabase.Server.Utils.Image
{
    public interface IImageManipulation
    {
        byte[] ResizeTo(byte[] sourceImageData, int size);
        (int Width, int Height) GetSize(byte[] imageData);
    }
}