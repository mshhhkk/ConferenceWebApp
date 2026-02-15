using System.Text.RegularExpressions;
using ConferenceWebApp.Application.DTOs;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ConferenceWebApp.Infrastructure.Services;

public sealed class ReportLinkingService : IReportLinkingService
{
    private readonly IReportCsvReader _csv;
    private readonly IReportsRepository _reports;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IProcessedFilesRegistry _processed;
    private readonly ILogger<ReportLinkingService> _log;

    // Приходит из Program.cs (НЕ тянем IHostEnvironment внутрь сервиса)
    private readonly string _uploadsDir;       // физический путь, если когда-нибудь нужно будет класть файлы
    private readonly string _uploadsUrlPrefix; // веб-префикс для ссылок в БД (например "/uploads")

    public ReportLinkingService(
        IReportCsvReader csv,
        IReportsRepository reports,
        IUserProfileRepository userProfileRepository,
        IProcessedFilesRegistry processed,
        string uploadsDir,
        string uploadsUrlPrefix,
        ILogger<ReportLinkingService> log)
    {
        _csv = csv;
        _reports = reports;
        _userProfileRepository = userProfileRepository;
        _processed = processed;
        _uploadsDir = uploadsDir;
        _uploadsUrlPrefix = uploadsUrlPrefix.StartsWith("/") ? uploadsUrlPrefix : "/" + uploadsUrlPrefix;
        _log = log;

        Directory.CreateDirectory(_uploadsDir);
    }

    public async Task<LinkingResult> BindByEmailAsync(Guid userId, string email, CancellationToken ct = default)
    {
        var result = new LinkingResult();
        if (string.IsNullOrWhiteSpace(email))
        {
            result.Messages.Add("E-mail пустой — пропуск.");
            return result;
        }

        var rows = await _csv.ReadAsync(ct);
        var matched = rows.Where(r => EmailMatches(r.EmailsRaw, email)).ToList();

        if (matched.Count == 0)
        {
            result.Messages.Add($"В CSV не найдено записей для email: {email}");
            return result;
        }

        await AddReportsForRows(userId, matched, result, ct, true);
       
        return result;
    }

    /// <summary>
    /// Матчит CSV (где могут быть «Фамилия И.» или «Фамилия И. О.») против ПОЛНОГО ФИО пользователя,
    /// переданного в параметры (surname, firstName, middleName).
    /// </summary>
    public async Task<LinkingResult> BindByNameAsync(
        Guid userId, string surname, string? firstName, string? middleName, CancellationToken ct = default)
    {
        var result = new LinkingResult();
        if (string.IsNullOrWhiteSpace(surname))
        {
            result.Messages.Add("Фамилия пуста — пропуск.");
            return result;
        }

        var profSurname = NormalizeName(surname);
        var profFirst = NormalizeName(firstName);
        var profMiddle = NormalizeName(middleName);

        var rows = await _csv.ReadAsync(ct);

        bool match(ReportCsvRowDTO r)
        {
            var rowSurname = NormalizeName(r.Surname);
            var rowFirst = NormalizeName(r.FirstName);
            var rowMiddle = NormalizeName(r.MiddleName);

            // 1) Фамилия — строгое совпадение
            if (!NameEquals(rowSurname, profSurname))
                return false;

            // 2) Имя — допускаем полное совпадение ИЛИ совпадение по первой букве/инициалу
            if (!NameTokenMatches(rowFirst, profFirst))
                return false;

            if (!string.IsNullOrEmpty(rowMiddle) && !string.IsNullOrEmpty(profMiddle))
                return NameTokenMatches(rowMiddle, profMiddle);

            // все прочие варианты считаем совпадением
            return true;
        }

        var matched = rows.Where(match).ToList();

        if (matched.Count == 0)
        {
            var shown = string.Join(' ', new[] { surname, firstName, middleName }.Where(s => !string.IsNullOrWhiteSpace(s)));
            result.Messages.Add($"В CSV не найдено записей для ФИО: {shown}");
            return result;
        }

        await AddReportsForRows(userId, matched, result, ct, false);
       
       
        return result;
    }


    private static string NormalizeName(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        var t = s.Trim().ToLowerInvariant()
                 .Replace('ё', 'е')
                 .Replace(".", "")
                 .Replace("-", " ");
        t = Regex.Replace(t, @"\s+", " ");
        return t;
    }

    private static bool NameEquals(string a, string b) =>
        string.Equals(a, b, StringComparison.Ordinal);

    /// <summary>
    /// Совпадение токена (имя/отчество), допускающее инициалы.
    /// - rowToken: из CSV (может быть "", "и", "и.", "иван")
    /// - inputToken: полная форма из профиля (может быть также "и"/"и.")
    /// </summary>
    private static bool NameTokenMatches(string rowToken, string inputToken)
    {
        if (string.IsNullOrEmpty(inputToken))
            return string.IsNullOrEmpty(rowToken);

        if (string.IsNullOrEmpty(rowToken))
            return false;

        // Полное совпадение после нормализации
        if (NameEquals(rowToken, inputToken)) return true;

        // Сравнение по инициалам: первая буква у обоих
        var inputInitial = FirstLetter(inputToken);
        var rowInitial = FirstLetter(rowToken);
        return inputInitial != '\0' && inputInitial == rowInitial;
    }

    private static char FirstLetter(string s)
    {
        foreach (var c in s)
        {
            if (char.IsLetter(c)) return char.ToLowerInvariant(c);
        }
        return '\0';
    }

    private static string NormalizeFileName(string? s) =>
        string.IsNullOrWhiteSpace(s) ? string.Empty : Path.GetFileName(s).Trim().ToLowerInvariant();

    private async Task AddReportsForRows(Guid userId, List<ReportCsvRowDTO> rows, LinkingResult result, CancellationToken ct, bool bindEmail)
    {
        var existing = await _reports.GetReportsByUserIdAsync(userId);
        var existingTitles = new HashSet<string>(
            existing.Select(x => (x.ReportTheme ?? "").Trim()),
            StringComparer.OrdinalIgnoreCase);

        foreach (var r in rows)
        {
            ct.ThrowIfCancellationRequested();

            if (!TryParseSection(r.SectionKey, out var section))
            { result.SkippedInvalid++; result.Messages.Add($"Пропуск: секция '{r.SectionKey}' у «{r.Title}»."); continue; }

            if (!TryParseWorkType(r.WorkTypeKey, out var workType))
            { result.SkippedInvalid++; result.Messages.Add($"Пропуск: тип '{r.WorkTypeKey}' у «{r.Title}»."); continue; }

            var normalized = NormalizeFileName(r.FileName);
            if (string.IsNullOrEmpty(normalized))
            { result.SkippedInvalid++; result.Messages.Add($"Пропуск: пустое имя файла у «{r.Title}»."); continue; }

            // Файл уже занят?
            var owner = await _processed.GetOwnerAsync(normalized, ct);
            if (owner.HasValue && owner.Value != userId)
            {
                result.SkippedInvalid++;
                result.Messages.Add($"Файл '{r.FileName}' уже привязан к другому пользователю — пропуск.");
                continue;
            }

            // Для того же пользователя — считаем дублем
            if (owner.HasValue && owner.Value == userId)
            { result.SkippedAlreadyExists++; continue; }

            // Дубликат по названию
            if (existingTitles.Contains((r.Title ?? "").Trim()))
            { result.SkippedAlreadyExists++; continue; }

            // Зафиксируем владение файлом (персистентная «булевая матрица»)
            var marked = await _processed.TryMarkOwnedAsync(normalized, userId, ct);
            if (!marked)
            {
                result.SkippedInvalid++;
                result.Messages.Add($"Не удалось отметить владение файлом '{r.FileName}' — пропуск.");
                continue;
            }


            var virtualPath = $"{_uploadsUrlPrefix}/{normalized}".Replace("//", "/");

            var report = new Reports
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AuthorId = userId,
                ReportTheme = r.Title,
                Section = section,
                WorkType = workType,
                Status = ReportStatus.ExtendedThesisApproved,
                FilePath = virtualPath,
                UploadedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                IsAuthor = true
            };
            var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (bindEmail)
            {
                
                userProfile.FirstName = r.FirstName;
                userProfile.LastName = r.Surname;
                userProfile.MiddleName = r.MiddleName;
               
            }

            userProfile.ApprovalStatus = UserApprovalStatus.ExtendedThesisApproved;
            if (userProfile.Position != Position.Student)
            {
                userProfile.Status = ParticipantStatus.ProfileCompleted;
            }
            await _userProfileRepository.UpdateAsync(userProfile);

            await _reports.AddReportAsync(report);

            existingTitles.Add((r.Title ?? "").Trim());
            result.Added++;
        }
    }

    private static bool EmailMatches(string emailsRaw, string targetEmail)
    {
        if (string.IsNullOrWhiteSpace(emailsRaw) || string.IsNullOrWhiteSpace(targetEmail))
            return false;

        var tokens = Regex.Split(emailsRaw, @"[;,]\s*")
                          .Select(x => x.Trim())
                          .Where(x => !string.IsNullOrWhiteSpace(x));

        return tokens.Any(e => string.Equals(e, targetEmail, StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryParseSection(string key, out SectionTopic section) =>
        Enum.TryParse(key, ignoreCase: true, out section);

    private static bool TryParseWorkType(string key, out WorkType workType)
    {
        if (Enum.TryParse(key, ignoreCase: true, out workType)) return true;

        var normalized = key.Trim().ToLowerInvariant();
        return normalized switch
        {
            "стендовый" => (workType = WorkType.Стендовый) is WorkType,
            "доклад" => (workType = WorkType.Доклад) is WorkType,
            _ => false
        };
    }
}
