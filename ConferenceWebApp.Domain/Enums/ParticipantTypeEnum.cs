using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.Domain.Enums;

public enum ParticipantType
{
    [Display(Name = "Слушатель")]
    Spectator,

    [Display(Name = "Выступающий")]
    Speaker
}