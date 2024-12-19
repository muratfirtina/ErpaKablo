using Application.Services;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services.Seo;

public class ImageOptimizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IImageSeoService _imageSeoService;

    public ImageOptimizationMiddleware(RequestDelegate next, IImageSeoService imageSeoService)
    {
        _next = next;
        _imageSeoService = imageSeoService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Eğer istek bir görsel için ise
        if (IsImageRequest(context.Request.Path))
        {
            var acceptHeader = context.Request.Headers["Accept"].ToString();
            string bestFormat = DetermineBestFormat(acceptHeader);

            // URL'i format bazlı versiyon için güncelle
            ModifyRequestForFormat(context, bestFormat);
        }

        await _next(context);
    }

    private bool IsImageRequest(PathString path)
    {
        return path.StartsWithSegments("/images") ||
               path.Value?.EndsWith(".jpg") == true ||
               path.Value?.EndsWith(".png") == true ||
               path.Value?.EndsWith(".webp") == true||
               path.Value?.EndsWith(".avif") == true;
    }

    private string DetermineBestFormat(string acceptHeader)
    {
        if (acceptHeader.Contains("image/avif")) return "avif";
        if (acceptHeader.Contains("image/webp")) return "webp";
        return "original";
    }

    private void ModifyRequestForFormat(HttpContext context, string format)
    {
        var originalPath = context.Request.Path.Value;
        var extension = Path.GetExtension(originalPath);
        
        if (format != "original")
        {
            context.Request.Path = new PathString(
                originalPath.Replace(extension, $".{format}"));
        }
    }
}