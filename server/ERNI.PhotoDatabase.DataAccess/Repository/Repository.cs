using System;

namespace ERNI.PhotoDatabase.DataAccess.Repository
{
    internal class Repository : IRepository, IDisposable
    {
        private readonly Lazy<IPhotoRepository> _photoRepository;
        private readonly Lazy<ITagRepository> _tagRepository;

        private DatabaseContext _dbContext;
        private bool _disposed;

        public Repository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            _photoRepository = new Lazy<IPhotoRepository>(() => new PhotoRepository(this._dbContext));
            _tagRepository = new Lazy<ITagRepository>(() => new TagRepository(this._dbContext));
        }

        public IPhotoRepository PhotoRepository => this._photoRepository.Value;
        public ITagRepository TagRepository => this._tagRepository.Value;

        #region IDisposable

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _dbContext.Dispose();
                _dbContext = null;
            }

            _disposed = true;
        }

        #endregion
    }
}
