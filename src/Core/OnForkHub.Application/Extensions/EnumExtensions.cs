namespace OnForkHub.Application.Extensions;

public static class EnumExtensions
{
    public static string ToDescriptionString<TEnum>(this TEnum tEnum)
    {
        if (tEnum is null)
        {
            throw new ArgumentNullException(nameof(tEnum));
        }
        var info = tEnum.GetType().GetField(tEnum.ToString()!)!;
        var attributes = (DescriptionAttribute[])info.GetCustomAttributes(typeof(DescriptionAttribute), false)!;
        if (attributes.Length > 0)
        {
            return attributes[0].Description;
        }
        return tEnum.ToString()!;
    }

    public static TEnum ParseEnumFromDescription<TEnum>(string description)
        where TEnum : struct
    {
        foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (
                (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                && attribute.Description.Equals(description, StringComparison.OrdinalIgnoreCase)
                && Enum.TryParse<TEnum>(field.Name, out var value)
            )
            {
                return value;
            }
        }
        throw new ArgumentException($"Could not find a matching value for '{description}' em {typeof(TEnum).Name}.");
    }
}
