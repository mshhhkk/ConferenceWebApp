using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.Domain.Enums;

public enum ScientificDegree
{
    [Display(Name = "Нет степени")]
    NonDegree = 0,

    [Display(Name = "Кандидат наук")]
    Candidate = 1,

    [Display(Name = "Доктор наук")]
    Doktor = 2
}
