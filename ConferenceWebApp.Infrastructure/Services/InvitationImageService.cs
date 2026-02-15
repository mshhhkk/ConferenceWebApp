using ConferenceWebApp.Application;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkiaSharp;

public sealed class InvitationImageService : IInvitationImageService
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<InvitationImageService> _log;
    private readonly IPersonalAccountService _pa;
    private readonly IReportsRepository _reports;
    private readonly IUserProfileRepository _profiles;

    public InvitationImageService(
        IConfiguration cfg,
        ILogger<InvitationImageService> log,
        IPersonalAccountService pa,
        IReportsRepository reports,
        IUserProfileRepository profiles)
    {
        _cfg = cfg; _log = log; _pa = pa; _reports = reports; _profiles = profiles;
    }

    public async Task<Result<InvitationPng>> BuildForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _profiles.GetByUserIdAsync(userId);
        if (profile is null)
            return Result<InvitationPng>.Failure("Профиль не найден");

        var reports = await _reports.GetReportsByUserIdAsync(userId);
        var accepted = reports.Where(r => r.Status == ReportStatus.ExtendedThesisApproved).ToList();
        var isPaid = profile.Status == ParticipantStatus.ParticipationConfirmed;

        var bytes = RenderPng(profile, accepted, isPaid);
        return Result<InvitationPng>.Success(new InvitationPng(bytes));
    }

    private byte[] RenderPng(UserProfile profile, List<Reports> accepted, bool isPaid)
    {

        // A4 @300dpi ~ 2480x3508
        const int WIDTH = 2480;
        const int HEIGHT = 3508;

        using var bmp = new SKBitmap(WIDTH, HEIGHT, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(SKColors.White);

        // Шрифты — максимально нейтральные
        var tfRegular = SKTypeface.Default;
        var tfBold = SKTypeface.FromFamilyName(null, SKFontStyle.Bold);

        using var h1 = new SKPaint { Color = SKColors.Black, IsAntialias = true, TextSize = 60, Typeface = tfBold };
        using var h2 = new SKPaint { Color = SKColors.Black, IsAntialias = true, TextSize = 42, Typeface = tfBold };
        using var body = new SKPaint { Color = SKColors.Black, IsAntialias = true, TextSize = 36, Typeface = tfRegular };
        using var bodyItalic = new SKPaint { Color = SKColors.Black, IsAntialias = true, TextSize = 36, Typeface = SKTypeface.FromFamilyName(null, SKFontStyle.Italic) };
        using var small = new SKPaint { Color = SKColors.Black, IsAntialias = true, TextSize = 32, Typeface = tfRegular };

        var cfg = _cfg.GetSection("Invitation");
        string confTitle = cfg["ConferenceTitle"] ?? "";
        string confSubtitle = cfg["ConferenceSubtitle"] ?? "";
        string dates = cfg["DatesLine"] ?? "";
        string venue = cfg["VenueLine"] ?? "";
        string city = cfg["CityCountryLine"] ?? "";
        string feeLine = cfg["FeeLine"] ?? "";
        string bankTitle = cfg["BankTitle"] ?? "Банковские реквизиты";
        var bankLines = (cfg.GetSection("BankLines").Get<string[]>() ?? Array.Empty<string>()).ToList();
        string chairman = cfg["ChairmanFullName"] ?? "";
        string chairPost1 = cfg["ChairmanPostLine1"] ?? "";
        string chairPost2 = cfg["ChairmanPostLine2"] ?? "";

        // Верх без фото, просто текстовая «шапка»
        float margin = 160f;              // поля
        float left = margin;
        float right = WIDTH - margin;
        float maxWidth = right - left;
        float y = 220f;

        y = DrawWrap(canvas, confTitle, h1, left, y, maxWidth, 70);
        y = DrawWrap(canvas, confSubtitle, body, left, y + 6, maxWidth, 46);
        y = DrawWrap(canvas, dates, small, left, y + 2, maxWidth, 42);
        y = DrawWrap(canvas, city, small, left, y + 2, maxWidth, 42);

        // Тонкая разделительная линия (не «дизайн», а просто разграничение)
        using (var pen = new SKPaint { Color = new SKColor(200, 200, 200), StrokeWidth = 1, IsAntialias = true })
        {
            canvas.DrawLine(left, y + 24, right, y + 24, pen);
        }
        y += 60;

        // Обращение
        var fullName = BuildSalutation(profile);
        y = DrawWrap(canvas, fullName, h2, left, y, maxWidth, 56);

        // Вступление
        var intro = "По поручению Оргкомитета конференции сообщаем, что Ваш(и) доклад(ы):";
        y = DrawWrap(canvas, intro, body, left, y + 20, maxWidth);

        // Список принятых
        int n = 1;
        foreach (var r in accepted)
        {
            var typeRu = (r.WorkType == WorkType.Доклад) ? "Принят в качестве устного" : "Принят в качестве стендового";
            var section = EnumDescriptionGetter.Handle(r.Section);
            var line1 = $"{n}. {r.ReportTheme}";
            var line2 = $"{typeRu} в секции: {section}";
            y = DrawWrap(canvas, line1, bodyItalic, left, y + 18, maxWidth);
            y = DrawWrap(canvas, line2, small, left, y + 2, maxWidth);
            n++;
        }

        // Место проведения
        y = DrawWrap(canvas, "Место проведения конференции:", h2, left, y + 26, maxWidth, 50);
        y = DrawWrap(canvas, venue, small, left, y + 6, maxWidth);

        // Оргвзнос
        if (isPaid)
        {
            y = DrawWrap(canvas, "Организационный взнос:", h2, left, y + 26, maxWidth, 50);
            y = DrawWrap(canvas, "Оргвзнос получен. Спасибо!", small, left, y + 6, maxWidth);
        }
        else
        {
            y = DrawWrap(canvas, "Организационный взнос:", h2, left, y + 26, maxWidth, 50);
            y = DrawWrap(canvas, feeLine, small, left, y + 6, maxWidth);

            y = DrawWrap(canvas, bankTitle, h2, left, y + 18, maxWidth, 50);
            foreach (var l in bankLines)
                y = DrawWrap(canvas, l, small, left, y + 2, maxWidth);
        }

        // Завершение и подпись
        y = DrawWrap(canvas, "С уважением,", body, left, y + 28, maxWidth);
        y = DrawWrap(canvas, chairPost1, small, left, y + 6, maxWidth);
        y = DrawWrap(canvas, chairPost2, small, left, y + 2, maxWidth);

        var signPath = ResolveSignaturePath();
        if (signPath is not null)
        {
            using var data = SKData.Create(signPath);
            using var img = SKImage.FromEncodedData(data);
            float signH = 110f;
            float signW = img.Width * (signH / img.Height);
            var dst = new SKRect(left, y + 8, left + signW, y + 8 + signH);
            canvas.DrawImage(img, dst);
            y += signH + 8;
        }
        else
        {
            _log.LogWarning("signature.png не найден ни в Invitation:SignaturePath, ни в стандартных путях.");
        }

        y = DrawWrap(canvas, chairman, small, left, y + 10, maxWidth);

        using var image = SKImage.FromBitmap(bmp);
        using var d = image.Encode(SKEncodedImageFormat.Png, 100);
        return d.ToArray();
    }

    private string? ResolveSignaturePath()
    {
        // 1) Явно задано в конфиге (ENV или appsettings)
        var raw = _cfg["Invitation:SignaturePath"];
        if (!string.IsNullOrWhiteSpace(raw))
        {
            var configured = Path.IsPathRooted(raw)
                ? raw
                : Path.Combine(Directory.GetCurrentDirectory(), raw); // относительный -> от ContentRoot
            if (File.Exists(configured)) return configured;

            _log.LogWarning("Invitation:SignaturePath задан, но файл не найден: {Path}", configured);
        }

        // 2) Типовые места
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "signature.png"),
            Path.Combine(Directory.GetCurrentDirectory(), "assets", "signature.png"),
            Path.Combine(AppContext.BaseDirectory, "signature.png"),
        };

        foreach (var p in candidates)
            if (File.Exists(p)) return p;

        return null; // не нашли — вернём null, просто не нарисуем подпись
    }

    private static string BuildSalutation(UserProfile p)
    {
        string fio =
            !string.IsNullOrWhiteSpace(p.FirstName) && !string.IsNullOrWhiteSpace(p.MiddleName)
            ? $"{p.FirstName} {p.MiddleName} {p.LastName}".Trim()
            : $"{p.FirstName} {p.LastName}".Trim();

        return $"Уважаемый(ая) {fio}!";
    }

    private static float DrawWrap(SKCanvas c, string text, SKPaint paint, float x, float y, float maxWidth, float lineStep = 44)
    {
        foreach (var line in Wrap(text ?? string.Empty, paint, maxWidth))
        {
            c.DrawText(line, x, y, paint);
            y += lineStep;
        }
        return y;
    }

    private static IEnumerable<string> Wrap(string text, SKPaint p, float maxWidth)
    {
        var words = (text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var sb = new System.Text.StringBuilder();
        foreach (var w in words)
        {
            var probe = (sb.Length == 0) ? w : (sb.ToString() + " " + w);
            if (p.MeasureText(probe) <= maxWidth)
            {
                if (sb.Length == 0) sb.Append(w);
                else { sb.Append(' '); sb.Append(w); }
            }
            else
            {
                if (sb.Length > 0) { yield return sb.ToString(); sb.Clear(); }
                if (p.MeasureText(w) > maxWidth) yield return w; // слишком длинное слово — отдельной строкой
                else sb.Append(w);
            }
        }
        if (sb.Length > 0) yield return sb.ToString();
    }
}
