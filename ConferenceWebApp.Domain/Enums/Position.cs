using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
