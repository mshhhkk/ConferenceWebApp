using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.Domain.Enums;

public enum UserApprovalStatus
{
    [Display(Name = "Нет статус")]
    None = 0,

    [Display(Name = "Одобрен тезис")]
    ThesisApproved = 1,

    [Display(Name = "Одобрен расширенный тезис")]
    ExtendedThesisApproved = 2
}
