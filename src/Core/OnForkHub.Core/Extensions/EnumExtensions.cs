namespace OnForkHub.Core.Extensions;

public static class EnumExtensions
{
    public static T GetAttribute<T>(this Enum value)
        where T : Attribute
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value)!;

        return type.GetField(name)!.GetCustomAttributes(false).OfType<T>().FirstOrDefault()!;
    }

    public static string GetDescription(this Enum value)
    {
        var attributes = (DescriptionAttribute[])value.GetType().GetField(value.ToString())!.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return Array.Find(attributes, a => true)?.Description ?? value.ToString();
    }
}
