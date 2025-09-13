using ConferenceWebApp.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceWebApp.Application.Controllers;

public class ArchiveController : BaseController
{

    public ArchiveController(IUserProfileRepository userProfileRepository) : base(userProfileRepository) { }

    public IActionResult Index()
    {
        var model = new ArchiveModel
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
                ProgramUrl = "/files/2024_program.pdf",
                Volume1Url = "/files/2024_volume1.pdf",
                Volume2Url = "/files/2024_volume2.pdf",
                PhotosUrl = "/files/2024_photos.zip"
            },
            // Добавьте другие года
        }
        };
        return View(model);
    }

}