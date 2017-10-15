namespace ERNI.PhotoDatabase.DataAccess.Images
{
    public class ImageStoreConfiguration
    {
        public string ConnectionString { get; set; } = "UseDevelopmentStorage=true";

        public string ContainerName { get; set; } = "photos";
    }
}
