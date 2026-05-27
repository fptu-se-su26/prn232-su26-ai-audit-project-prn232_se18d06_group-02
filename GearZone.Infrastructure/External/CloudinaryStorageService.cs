using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GearZone.Application.Abstractions.External;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace GearZone.Infrastructure.External
{
    public class CloudinaryStorageService : IFileStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryStorageService(IConfiguration configuration)
        {
            var cloudName = configuration["CLOUDINARY_CLOUD_NAME"];
            var apiKey = configuration["CLOUDINARY_API_KEY"];
            var apiSecret = configuration["CLOUDINARY_API_SECRET"];

            if (string.IsNullOrEmpty(cloudName) ||
                string.IsNullOrEmpty(apiKey) ||
                string.IsNullOrEmpty(apiSecret))
            {
                throw new Exception("Cloudinary environment variables (CLOUDINARY_CLOUD_NAME, CLOUDINARY_API_KEY, CLOUDINARY_API_SECRET) are missing.");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<List<string>> UploadAsync(List<IFormFile> files, string folder = "GearZone/images")
        {
            var imageUrls = new List<string>();
            var targetFolder = string.IsNullOrWhiteSpace(folder) ? "GearZone/images" : folder;

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                await using var stream = file.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = targetFolder,
                    Transformation = new Transformation()
                        .Quality("auto")
                        .FetchFormat("auto")
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Failed to upload image to Cloudinary.");
                }

                imageUrls.Add(result.SecureUrl.ToString());
            }

            return imageUrls;
        }
        public async Task DeleteAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            try
            {
                // Extract public_id from cloudinary URL
                // Example: https://res.cloudinary.com/cloud_name/image/upload/v12345/FUNewsManagement/images/name.jpg
                // Public ID would be: FUNewsManagement/images/name

                var uri = new Uri(fileUrl);
                var segments = uri.Segments;

                // Find where the folder starts
                int uploadIndex = Array.FindIndex(segments, s => s.Equals("upload/", StringComparison.OrdinalIgnoreCase));
                if (uploadIndex != -1 && segments.Length > uploadIndex + 2)
                {
                    // Everything after the version (v12345/) is the public_id
                    var publicIdWithExtension = string.Join("", segments.Skip(uploadIndex + 2)).TrimEnd('/');
                    var publicId = Path.ChangeExtension(publicIdWithExtension, null);

                    var deletionParams = new DeletionParams(publicId);
                    await _cloudinary.DestroyAsync(deletionParams);
                }
            }
            catch (Exception ex)
            {
                // Log error if needed, but don't fail the whole process
                Console.WriteLine($"Error deleting image from Cloudinary: {ex.Message}");
            }
        }
    }
}
