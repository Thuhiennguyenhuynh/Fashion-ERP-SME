using System.IO;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FashionERP.Application.Common;
using FashionERP.Application.Interfaces;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(Cloudinary cloudinary)
    {
        _cloudinary = cloudinary;
    }

    // ─── UPLOAD ───────────────────────────────────────────
    public async Task<CloudinaryUploadResult> UploadImageAsync(
        Stream imageStream, string folder, string? publicId = null)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(publicId ?? Guid.NewGuid().ToString(), imageStream),
            Folder = folder,
            PublicId = publicId,
            Overwrite = true,
            Transformation = new Transformation()
                .Quality("auto")
                .FetchFormat("auto")
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new AppException($"Lỗi upload ảnh: {result.Error.Message}", 500);

        return new CloudinaryUploadResult
        {
            Url = result.SecureUrl.ToString(),
            PublicId = result.PublicId
        };
    }

    // ─── DELETE ───────────────────────────────────────────
    public async Task DeleteImageAsync(string publicId)
    {
        if (string.IsNullOrEmpty(publicId)) return;

        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);

        if (result.Error != null)
            throw new AppException($"Lỗi xóa ảnh trên Cloudinary: {result.Error.Message}", 500);
    }
}

 
 
