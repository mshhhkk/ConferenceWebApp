using Microsoft.AspNetCore.Mvc.RazorPages;

public class ArchiveModel : PageModel
{
    public List<ArchiveYear> ArchiveYears { get; set; } = new List<ArchiveYear>();

    public void OnGet()
    {
        ArchiveYears = new List<ArchiveYear>
        {
            new ArchiveYear
            {
                Year = 2024,
                ProgramUrl = "/files/2024_program.pdf",
                Volume1Url = "/files/2024_volume1.pdf",
                Volume2Url = "/files/2024_volume2.pdf",
                PhotosUrl = "/files/2024_photos.zip"
            },
            new ArchiveYear
            {
                Year = 2023,
                ProgramUrl = "/files/2023_program.pdf",
                Volume1Url = "/files/2023_volume1.pdf",
                Volume2Url = "/files/2023_volume2.pdf",
                PhotosUrl = "/files/2023_photos.zip"
            }
        };
    }
}

public class ArchiveYear
{
    public int Year { get; set; }
    public string ProgramUrl { get; set; } = string.Empty;
    public string Volume1Url { get; set; } = string.Empty;
    public string Volume2Url { get; set; } = string.Empty;
    public string PhotosUrl { get; set; } = string.Empty;
}
