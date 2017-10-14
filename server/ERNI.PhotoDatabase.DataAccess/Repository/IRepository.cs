namespace ERNI.PhotoDatabase.DataAccess.Repository
{
    public interface IRepository
    {
        IPhotoRepository PhotoRepository { get; }
    }
}
