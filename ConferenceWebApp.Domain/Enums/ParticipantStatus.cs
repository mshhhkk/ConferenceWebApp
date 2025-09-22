using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.Domain.Enums;

public enum ParticipantStatus
{
    [Display(Name = "Зарегестрирован на сайте")]
    Registered = 0,

    [Display(Name = "Заполнен профиль")]
    ProfileCompleted = 1,

    [Display(Name = "Предложен чек")]
    CheckSubmitted = 2,

    [Display(Name = "Участие подстврждено")]
    ParticipationConfirmed = 3
}
