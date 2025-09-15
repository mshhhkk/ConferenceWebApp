using ConferenceWebApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace ConferenceWebApp.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _rootPath;

    public FileService(string rootPath)
    {
        _rootPath = rootPath;
    }

    public async Task<string> SaveFileAsync(
        IFormFile file,
        string destinationFolder,
        string[] allowedContentTypes,
        long maxFileSize)
    {

        if (file == null || file.Length == 0)
            throw new InvalidOperationException("Файл не предоставлен.");

        if (!allowedContentTypes.Contains(file.ContentType))
            throw new InvalidOperationException($"Разрешены только файлы: {string.Join(", ", allowedContentTypes)}.");

        if (file.Length > maxFileSize)
            throw new InvalidOperationException($"Размер файла не должен превышать {maxFileSize / (1024 * 1024)} МБ.");

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var relativePath = Path.Combine(destinationFolder, fileName).Replace("\\", "/");
        var fullPath = Path.Combine(_rootPath, relativePath);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/{relativePath}";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Ошибка при сохранении файла.", ex);
        }
    }

    public void DeleteFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || filePath.Contains("default"))
            return;

        var fullPath = Path.Combine(_rootPath, filePath.TrimStart('/'));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    public async Task<string> UpdateFileAsync(
        IFormFile newFile,
        string oldFilePath,
        string destinationFolder,
        string[] allowedContentTypes,
        long maxFileSize)
    {
        DeleteFile(oldFilePath);
        return await SaveFileAsync(newFile, destinationFolder, allowedContentTypes, maxFileSize);
    }

    public async Task<(Stream FileStream, string ContentType, string FileName)> GetFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new InvalidOperationException("Путь к файлу не указан.");

        var fullPath = Path.Combine(_rootPath, filePath.TrimStart('/'));
        if (!File.Exists(fullPath))
            throw new InvalidOperationException("Файл не найден.");

        var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        var fileName = Path.GetFileName(filePath);
        var fileExtension = Path.GetExtension(filePath).ToLower();
        var contentType = fileExtension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };

        return (fileStream, contentType, fileName);
    }

    public async Task<(bool Exists, string FileName, DateTime UploadDate)?> TryGetFileMetadataAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return null;

        var fullPath = Path.Combine(_rootPath, filePath.TrimStart('/'));
        if (!File.Exists(fullPath))
            return null;

        return (true, Path.GetFileName(filePath), File.GetCreationTime(fullPath));
    }
}
