using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;

namespace RaceDay.API.Clean.Services
{
    // Handles uploading, retrieving and deleting images in Azure Blob Storage.
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        // Sets the maximum allowed image size to 5 MB.
        private const long MaxFileSize = 5 * 1024 * 1024;

        // Connects the service to the Azure storage account.
        public BlobStorageService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        // Checks whether an uploaded file is a supported image.
        public bool IsValidImage(IFormFile file)
        {
            // Rejects empty files and files larger than 5 MB.
            if (file == null || file.Length == 0 || file.Length > MaxFileSize)
            {
                return false;
            }

            // Gets the file extension.
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            // Allows only supported image formats.
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

            // Checks whether the extension is supported.
            if (!allowedExtensions.Contains(extension))
            {
                return false;
            }

            // Checks whether the content type matches the file extension.
            string expectedContentType = extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => string.Empty
            };

            return file.ContentType.Equals(
                expectedContentType,
                StringComparison.OrdinalIgnoreCase);
        }

        // Uploads an image to the selected private Azure container.
        public async Task<string> UploadAsync(
            string containerName, IFormFile file)
        {
            // Rejects files that do not meet the image requirements.
            if (!IsValidImage(file))
            {
                throw new ArgumentException(
                    "Invalid image. Use JPG, JPEG, PNG or WEBP up to 5 MB.");
            }

            // Gets the selected Azure container.
            BlobContainerClient containerClient =
                _blobServiceClient.GetBlobContainerClient(containerName);

            // Creates the container if it does not already exist.
            await containerClient.CreateIfNotExistsAsync(
                PublicAccessType.None);

            // Generates a unique name for the uploaded image.
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string blobName = $"{Guid.NewGuid()}{extension}";

            // Gets a reference to the new blob.
            BlobClient blobClient =
                containerClient.GetBlobClient(blobName);

            // Opens the uploaded file.
            using Stream fileStream = file.OpenReadStream();

            // Uploads the image and records its content type.
            await blobClient.UploadAsync(
                fileStream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = file.ContentType
                    }
                });

            // Returns the unique filename for database storage.
            return blobName;
        }

        // Downloads an image from a private Azure container.
        public async Task<(byte[] Content, string ContentType)?> DownloadAsync(
            string containerName, string blobName)
        {
            // Gets the selected Azure container.
            BlobContainerClient containerClient =
                _blobServiceClient.GetBlobContainerClient(containerName);

            // Gets the requested image.
            BlobClient blobClient =
                containerClient.GetBlobClient(blobName);

            try
            {
                // Downloads the image content.
                var response = await blobClient.DownloadContentAsync();

                // Returns the image data and its content type.
                return (
                    response.Value.Content.ToArray(),
                    response.Value.Details.ContentType ?? "application/octet-stream"
                );
            }
            catch (RequestFailedException exception) when (exception.Status == 404)
            {
                // Returns null when the requested image does not exist.
                return null;
            }
        }

        // Deletes an image from a private Azure container.
        public async Task DeleteAsync(string containerName, string blobName)
        {
            // Stops if no image filename was supplied.
            if (string.IsNullOrWhiteSpace(blobName))
            {
                return;
            }

            // Gets the selected Azure container.
            BlobContainerClient containerClient =
                _blobServiceClient.GetBlobContainerClient(containerName);

            // Gets the image to delete.
            BlobClient blobClient =
                containerClient.GetBlobClient(blobName);

            // Deletes the image if it exists.
            await blobClient.DeleteIfExistsAsync();
        }
    }
}