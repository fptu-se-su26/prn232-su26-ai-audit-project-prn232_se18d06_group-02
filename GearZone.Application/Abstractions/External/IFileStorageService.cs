using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.External
{
    public interface IFileStorageService
    {
        Task<List<string>> UploadAsync(List<IFormFile> files, string folder = "GearZone/images");
        /// <summary>Uploads images from remote URLs (Cloudinary fetches each URL) and returns the hosted URLs. Bad URLs are skipped.</summary>
        Task<List<string>> UploadFromUrlsAsync(List<string> urls, string folder = "GearZone/images");
        Task DeleteAsync(string fileUrl);
    }
}
