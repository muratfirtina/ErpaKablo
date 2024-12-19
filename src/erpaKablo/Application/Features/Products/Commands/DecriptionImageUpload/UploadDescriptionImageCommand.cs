using Application.Dtos.Image;
using Application.Services;
using Application.Storage;
using Core.CrossCuttingConcerns.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace Application.Features.Products.Commands.DecriptionImageUpload;

public class UploadDescriptionImageCommand : IRequest<UploadedDescriptionImageResponse>
{
    public IFormFile Image { get; set; }
    public string? AltText { get; set; }
    public string? Title { get; set; }

    public UploadDescriptionImageCommand(IFormFile image, string? altText = null, string? title = null)
    {
        Image = image;
        AltText = altText;
        Title = title;
    }
    
    public class UploadDescriptionImageCommandHandler : IRequestHandler<UploadDescriptionImageCommand, UploadedDescriptionImageResponse>
    {
        private readonly IStorageService _storageService;
        private readonly IImageSeoService _imageSeoService;

        public UploadDescriptionImageCommandHandler(
            IStorageService storageService,
            IImageSeoService imageSeoService)
        {
            _storageService = storageService;
            _imageSeoService = imageSeoService;
        }

        public async Task<UploadedDescriptionImageResponse> Handle(UploadDescriptionImageCommand request, CancellationToken cancellationToken)
        {
            if (request.Image == null || request.Image.Length == 0)
                throw new BusinessException("No file uploaded");
            
            string folderId = Guid.NewGuid().ToString();

            // Görsel optimizasyonu yap
            using var stream = request.Image.OpenReadStream();
            var processedImage = await _imageSeoService.ProcessAndOptimizeImage(
                stream,
                request.Image.FileName,
                new ImageProcessingOptionsDto
                {
                    AltText = request.AltText ?? Path.GetFileNameWithoutExtension(request.Image.FileName),
                    Title = request.Title,
                    Path = folderId,
                    EntityType = "description-images"
                });

            // En uygun versiyonu seç (medium size genellikle içerik görselleri için uygundur)
            var selectedVersion = processedImage.Versions
                .FirstOrDefault(v => v.Size == "medium" && v.Format == "webp") ?? 
                processedImage.Versions.First();

            using var versionStream = new MemoryStream();
            await selectedVersion.Stream.CopyToAsync(versionStream);
            versionStream.Position = 0;

            var versionFileName = $"{Path.GetFileNameWithoutExtension(request.Image.FileName)}-{selectedVersion.Size}.{selectedVersion.Format}";
            var formFile = new FormFile(
                versionStream,
                0,
                versionStream.Length,
                "image",
                versionFileName);

            var uploadedFiles = await _storageService.UploadAsync(
                "description-images", 
                folderId, 
                new List<IFormFile> { formFile });

            var result = uploadedFiles.FirstOrDefault();
            if (!string.IsNullOrEmpty(result.url))
            {
                // HTML içinde kullanılacak img tag'ini SEO dostu şekilde oluştur
                string imgHtml = $@"<img src=""{result.url}"" 
                    alt=""{processedImage.SeoMetadata.AltText}"" 
                    title=""{processedImage.SeoMetadata.Title ?? ""}"" 
                    width=""{selectedVersion.Width}"" 
                    height=""{selectedVersion.Height}"" 
                    loading=""lazy"" 
                    decoding=""async"">";

                return new UploadedDescriptionImageResponse 
                { 
                    Url = result.url,
                    ImageHtml = imgHtml,
                    Width = selectedVersion.Width,
                    Height = selectedVersion.Height,
                    Alt = processedImage.SeoMetadata.AltText,
                    Title = processedImage.SeoMetadata.Title
                };
            }

            throw new BusinessException("Upload failed");
        }
    }
}
