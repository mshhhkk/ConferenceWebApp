using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.Domain.Enums;

public enum Position
{
    [Display(Name = "Студент")]
    Student = 0,

    [Display(Name = "Аспирант")]
    GradStudent = 1,

    [Display(Name = "Профессор")]
    Professor = 2,

    [Display(Name = "Без должности")]
    WithoutFeatures = 3
}
