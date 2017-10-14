using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ERNI.PhotoDatabase.DataAccess.Images
{
    public class ImageStore
    {
        private const string GuidFormat = "D";

        ImageStoreConfiguration _configuration;
        CloudStorageAccount _account;

        public ImageStore(ImageStoreConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _account = CloudStorageAccount.Parse(configuration.ConnectionString);
        }

        public Task Initialize(CancellationToken cancellationToken)
        {
            var client = _account.CreateCloudBlobClient();
            var container = GetContainerReference(client);
            return container.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, null, null, cancellationToken);
        }

        public async Task<ImageBlob> GetImageBlobAsync(Guid id, CancellationToken cancellationToken)
        {
            var client = _account.CreateCloudBlobClient();
            var container = GetContainerReference(client);
            var blob = container.GetBlockBlobReference(id.ToString(GuidFormat, CultureInfo.InvariantCulture));
            using (var bufferStream = new MemoryStream())
            {
                await blob.FetchAttributesAsync(null, null, null, cancellationToken);
                await blob.DownloadToStreamAsync(bufferStream, null, null, null, cancellationToken);
                return new ImageBlob
                {
                    Id = Guid.ParseExact(blob.Name, GuidFormat),
                    Content = bufferStream.ToArray()
                };
            }
        }

        public async Task SaveImageBlobAsync(ImageBlob image, CancellationToken cancellationToken)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var client = _account.CreateCloudBlobClient();
            var container = GetContainerReference(client);
            var blob = container.GetBlockBlobReference(image.Id.ToString(GuidFormat, CultureInfo.InvariantCulture));
            var buffer = image.Content;
            await blob.UploadFromByteArrayAsync(buffer, 0, buffer.Length, null, null, null, cancellationToken);
        }

        public async Task DeleteImageBlobAsync(Guid id, CancellationToken cancellationToken)
        {
            var client = _account.CreateCloudBlobClient();
            var container = GetContainerReference(client);
            var blob = container.GetBlockBlobReference(id.ToString(GuidFormat, CultureInfo.InvariantCulture));
            await blob.DeleteAsync(DeleteSnapshotsOption.None, null, null, null, cancellationToken);
        }

        private CloudBlobContainer GetContainerReference(CloudBlobClient client)
        {
            return client.GetContainerReference(_configuration.ContainerName);
        }
    }
}
