namespace ServiceLayer.Services
{
    public interface IImageStorageService
    {
        Task<string?> SaveImageAsync(IFormFile file, string folder);
        void DeleteImage(string? relativePath);
    }
}
