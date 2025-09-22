using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp;

public static class EnumHelper
{
    public static string GetEnumDescription(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));
        return attribute != null ? attribute.Name : value.ToString();
    }
}
