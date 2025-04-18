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

    public static TEnum ParseFromDescription<TEnum>(string description)
        where TEnum : Enum
    {
        ArgumentNullException.ThrowIfNull(description);
        var enumType = typeof(TEnum);
        var trimmedDescription = description.Trim();
        var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);

        var value = Array
            .Find(
                fields,
                field =>
                    field.GetCustomAttribute<DescriptionAttribute>()?.Description.Equals(trimmedDescription, StringComparison.OrdinalIgnoreCase)
                        == true
                    || field.Name.Equals(trimmedDescription, StringComparison.OrdinalIgnoreCase)
            )
            ?.GetValue(null);

        return value is null ? throw new ArgumentException($"Description '{description}' not found in enum {enumType.Name}") : (TEnum)value;
    }
}