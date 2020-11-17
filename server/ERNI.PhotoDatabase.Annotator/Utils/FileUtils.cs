using System.IO;

namespace ERNI.PhotoDatabase.Annotator.Utils
{
    public class FileUtils
    {
        public static string AssetsRelativePath = @"../../../../ERNI.PhotoDatabase.Annotator/assets";
        public static string AssetsPath = GetAbsolutePath(AssetsRelativePath);
        public static string ModelFilePath = Path.Combine(AssetsPath, "Model", "TinyYolo2_model.onnx");
        public static string ImagesFolder = Path.Combine(AssetsPath, "images");
        public static string OutputFolder = Path.Combine(AssetsPath, "images", "output");

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(AnnotationPredictor).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
