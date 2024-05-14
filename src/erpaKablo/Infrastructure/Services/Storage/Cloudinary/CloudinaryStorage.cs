using Application.Storage.Cloudinary;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain;
using Infrastructure.Adapters.Image.Cloudinary;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Storage.Cloudinary;

public class CloudinaryStorage : ICloudinaryStorage
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;
    
    public CloudinaryStorage(IConfiguration configuration)
    {
        var cloudinaryAccountSettings = configuration.GetSection("CloudinaryAccount").Get<CloudinarySettings>();

        var account = new Account(
            cloudinaryAccountSettings?.CloudName,
            cloudinaryAccountSettings?.ApiKeyName,
            cloudinaryAccountSettings?.ApiSecretName
        );

        _cloudinary = new CloudinaryDotNet.Cloudinary(account);
    }

    /*public async Task<List<(string fileName, string path, string containerName)>> UploadAsync(string category,
        string path, List<IFormFile> files)
    {
        var datas = new List<(string fileName, string path, string containerName)>();
        foreach (IFormFile file in files)
        {
            ImageUploadParams imageUploadParams = new()
            {
                File = new FileDescription(file.FileName, stream: file.OpenReadStream()),
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = false
            };
            
            
            
            imageUploadParams.Folder = category;
            imageUploadParams.File.FileName = file.FileName;

            ImageUploadResult imageUploadResult = await _cloudinary.UploadAsync(imageUploadParams);
            datas.Add((file.FileName, imageUploadResult.Url.ToString(), path));
            
        }
        return datas;
        
    }*/


    public async Task<List<(string fileName, string path, string containerName)>> UploadFileToStorage(string category,
        string path, string fileName, MemoryStream fileStream)
    {
        var datas = new List<(string fileName, string path, string containerName)>();
        ImageUploadParams imageUploadParams = new()
        {
            File = new FileDescription(fileName, stream: fileStream),
            UseFilename = true,
            UniqueFilename = false,
            Overwrite = false
        };
        
        imageUploadParams.Folder = category + "/" + path;
        imageUploadParams.File.FileName = fileName;
        
        await _cloudinary.UploadAsync(imageUploadParams);
        
        
        return null;
        
    }

    public async Task DeleteAsync(string path)
    {
        var publicId = GetPublicId(path);
        var deletionParams = new DeletionParams(publicId);
        await _cloudinary.DestroyAsync(deletionParams);
    }

    public Task<List<T>?> GetFiles<T>(string employeeId) where T : ImageFile, new() => throw new NotImplementedException();

    public Task<List<T>> GetFiles<T>(string category, string path) where T : ImageFile, new() => throw new NotImplementedException();

    public async Task<List<string?>> GetFiles(string path, string category)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        var files = directoryInfo.GetFiles();
        List<string> datas = new();
        foreach (var file in files)
        {
            datas.Add(file.Name);
        }
        return datas;
        
    }

    public bool HasFile(string path, string fileName) 
        => File.Exists(Path.Combine(path, fileName));

    

    private string GetPublicId(string imageUrl)
    {
        // Cloudinary URL'sinden '/image/upload/' dizgisinden sonraki kısmı çıkarmak için
        var uploadSegment = "/image/upload/";
        var startIndex = imageUrl.IndexOf(uploadSegment) + uploadSegment.Length;
        if(startIndex > uploadSegment.Length - 1)
        {
            // '/image/upload/' segmentinden sonraki kısmı alır
            var pathWithVersion = imageUrl.Substring(startIndex);
        
            // 'version' kısmını geçmek için ikinci '/' karakterinden sonrasını alır
            var pathStartIndex = pathWithVersion.IndexOf('/', 1);
            if(pathStartIndex > -1)
            {
                var publicId = pathWithVersion.Substring(pathStartIndex + 1);

                // Eğer varsa dosya uzantısını kaldırır
                var endIndex = publicId.LastIndexOf('.');
                if(endIndex > -1)
                {
                    publicId = publicId.Substring(0, endIndex);
                }

                return publicId;
            }
        }

        // Eğer URL beklenen formatta değilse, boş bir string dön
        return string.Empty;
    }
    

    
}