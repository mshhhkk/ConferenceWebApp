using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ConferenceWebApp.Application;

public static class EnumDescriptionGetter
{
    public static string Handle(Enum value)
    {
        var type = value.GetType();

        var name = Enum.GetName(type, value);
        if (name is null) return value.ToString();

        var field = type.GetField(name);
        if (field is null) return name;

        var display = field.GetCustomAttribute<DisplayAttribute>(inherit: false);
        var displayName = display?.GetName();
        if (!string.IsNullOrWhiteSpace(displayName))
            return displayName!;

        var desc = field.GetCustomAttribute<DescriptionAttribute>(inherit: false);
        if (!string.IsNullOrWhiteSpace(desc?.Description))
            return desc!.Description;

        return name;
    }
}
