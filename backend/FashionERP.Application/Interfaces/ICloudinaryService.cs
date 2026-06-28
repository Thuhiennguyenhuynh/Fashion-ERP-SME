namespace FashionERP.Application.Interfaces
{
    using System.IO;
    using System.Threading.Tasks;

    public class CloudinaryUploadResult
    {
        public string Url { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
    }

    public interface ICloudinaryService
    {
        Task<CloudinaryUploadResult> UploadImageAsync(Stream imageStream, string folder, string? publicId = null);
        Task DeleteImageAsync(string publicId);
    }
}

