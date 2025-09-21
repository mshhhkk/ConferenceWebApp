using ConferenceWebApp.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace ConferenceWebApp.Application.DTOs.Admin;

public class AdminEditReportDTO
{
    // Если Id == Guid.Empty → создаём новый, иначе редактируем существующий
    public Guid Id { get; set; } = Guid.Empty;

    // Базовые поля
    public string ReportTheme { get; set; } = string.Empty;
    public string? ExtThesis { get; set; }

    // Текущие сохранённые пути (для отображения/сохранения без замены)
    public string? ExtThesisFilePath { get; set; }
    public string? FilePath { get; set; }

    // Новые файлы (если загружаются – заменим соответствующие *FilePath)
    public IFormFile? ExtThesisFile { get; set; }
    public IFormFile? File { get; set; }

    // Enum-поля
    public SectionTopic Section { get; set; }
    public WorkType WorkType { get; set; }

    // Владелец/связь
    public Guid UserId { get; set; }

    // Авторство/адресация
    public bool IsAuthor { get; set; }
    public Guid AuthorId { get; set; } = Guid.Empty;
    public Guid TargetUserId { get; set; } = Guid.Empty;

    // Статусы
    public ReportStatus Status { get; set; }
    public ReportTransferStatus TransferStatus { get; set; }

    public string? RejectionComment { get; set; }

    // Даты — админ может править (ты просил «изменить все поля сущности»)
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    // Удобные флаги для очистки файлов без загрузки нового
    public bool RemoveMainFile { get; set; } = false;
    public bool RemoveExtThesisFile { get; set; } = false;
}
