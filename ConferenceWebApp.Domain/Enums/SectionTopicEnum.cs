using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.Domain.Enums;

public enum SectionTopic
{
    [Display(Name = "Теоретические проблемы квантовой электроники")]
    QuantumElectronicsIssues,

    [Display(Name = "Физика лазеров")]
    LaserPhysics,

    [Display(Name = "Системы и методы квантовой электроники")]
    QuantumElectronicsMethods,

    [Display(Name = "Компьютеризация лазерных исследований")]
    LaserResearch,

    [Display(Name = "Прикладные исследования")]
    AppliedResearch,

    [Display(Name = "Информационные технологии в радиофизике")]
    InformationTechnology,

    [Display(Name = "Методические аспекты преподавания")]
    TeachingAspects
}