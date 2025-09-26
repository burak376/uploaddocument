namespace DocumentManagementAPI.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> SaveFileAsync(IFormFile file, int companyId, string ownerKey, string documentTypeKey)
        {
            try
            {
                var now = DateTime.Now;
                var guid = Guid.NewGuid();
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{guid}_{Path.GetFileNameWithoutExtension(file.FileName)}{extension}";

                var relativePath = Path.Combine(
                    "uploads",
                    companyId.ToString(),
                    ownerKey,
                    documentTypeKey,
                    now.Year.ToString(),
                    now.Month.ToString("00"),
                    fileName
                );

                var fullPath = Path.Combine(_environment.ContentRootPath, relativePath);
                var directory = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return relativePath.Replace('\\', '/');
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file: {FileName}", file.FileName);
                throw;
            }
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.ContentRootPath, filePath.Replace('/', '\\'));
                
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return Task.FromResult(true);
                }
                
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                return Task.FromResult(false);
            }
        }

        public async Task<(byte[] fileBytes, string contentType, string fileName)> GetFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.ContentRootPath, filePath.Replace('/', '\\'));
                
                _logger.LogInformation("Attempting to read file from path: {FullPath}", fullPath);
                
                if (!File.Exists(fullPath))
                {
                    _logger.LogError("File does not exist at path: {FullPath}", fullPath);
                    throw new FileNotFoundException("File not found");
                }

                var fileBytes = await File.ReadAllBytesAsync(fullPath);
                var fileName = Path.GetFileName(fullPath);
                var contentType = GetContentType(Path.GetExtension(fileName));
                
                _logger.LogInformation("Successfully read file {FileName} with {FileSize} bytes", fileName, fileBytes.Length);

                return (fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file: {FilePath}", filePath);
                throw;
            }
        }

        public bool IsValidFileExtension(string fileName, List<string> allowedExtensions)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return allowedExtensions.Any(ext => ext.ToLowerInvariant() == extension);
        }

        public bool IsValidFileSize(long fileSize, int maxFileSizeMB)
        {
            var maxFileSizeBytes = maxFileSizeMB * 1024 * 1024;
            return fileSize <= maxFileSizeBytes;
        }

        private string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }
}