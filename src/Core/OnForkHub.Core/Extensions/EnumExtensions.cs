namespace OnForkHub.Core.Extensions;

public static class EnumExtensions
{
    public static string GetDescription<TEnum>(this TEnum value)
        where TEnum : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        return typeof(TEnum).GetField(value.ToString())?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value.ToString();
    }

    public static TEnum ParseFromDescription<TEnum>(string description)
        where TEnum : Enum
    {
        ArgumentNullException.ThrowIfNull(description);

        var enumType = typeof(TEnum);
        var trimmedDescription = description.Trim();

        var value = enumType
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(field =>
                field.GetCustomAttribute<DescriptionAttribute>()?.Description.Equals(trimmedDescription, StringComparison.OrdinalIgnoreCase) == true
                || field.Name.Equals(trimmedDescription, StringComparison.OrdinalIgnoreCase)
            )
            ?.GetValue(null);

        return value is null ? throw new ArgumentException($"Description '{description}' not found in enum {enumType.Name}") : (TEnum)value;
    }
}
