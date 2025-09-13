using Microsoft.AspNetCore.Http;

namespace ConferenceWebApp.Application.Interfaces.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string destinationPath, string[] allowedContentTypes, long maxFileSize);

        void DeleteFile(string filePath);

        Task<string> UpdateFileAsync(IFormFile newFile, string oldFilePath, string destinationPath, string[] allowedContentTypes, long maxFileSize);

        Task<(Stream FileStream, string ContentType, string FileName)> GetFileAsync(string filePath);

        Task<(bool Exists, string FileName, DateTime UploadDate)?> TryGetFileMetadataAsync(string filePath);
    }
}