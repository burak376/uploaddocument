namespace DocumentManagementAPI.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, int companyId, string ownerKey, string documentTypeKey);
        Task<bool> DeleteFileAsync(string filePath);
        Task<(byte[] fileBytes, string contentType, string fileName)> GetFileAsync(string filePath);
        bool IsValidFileExtension(string fileName, List<string> allowedExtensions);
        bool IsValidFileSize(long fileSize, int maxFileSizeMB);
    }
}