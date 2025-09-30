using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.Application;

public static class EnumDescriptionGetter
{
    public static string Handle(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));
        return attribute != null ? attribute.Name : value.ToString();
    }
}
