namespace JournalApp;

public static class EnumUtil
{
    public static string GetFriendlyName(this Enum @enum) => GetAttributeOrDefault<DescriptionAttribute>(@enum)?.Description ?? @enum.ToString();

    public static T GetAttributeOrDefault<T>(this Enum @enum) where T : Attribute =>
        @enum.GetType().GetMember(@enum.ToString())?.FirstOrDefault()?.GetCustomAttributes(typeof(T), false)?.FirstOrDefault() as T;
}
