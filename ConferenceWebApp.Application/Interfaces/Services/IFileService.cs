using Microsoft.AspNetCore.Http;

namespace ConferenceWebApp.Application.Interfaces.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string destinationPath, string[] allowedContentTypes, long maxFileSize);

        void DeleteFile(string filePath);

        Task<string> UpdateFileAsync(IFormFile newFile, string oldFilePath, string destinationPath, string[] allowedContentTypes, long maxFileSize);

        (FileStream Stream, string ContentType, string FileName) GetFile(string filePath);

        (bool Exists, string FileName, DateTime UploadDate)? TryGetFileMetadata(string filePath);
    }
}