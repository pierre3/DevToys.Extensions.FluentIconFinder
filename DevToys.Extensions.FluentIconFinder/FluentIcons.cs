namespace DevToys.Extensions.FluentIconFinder;

public static partial class FluentIcons
{
    public enum IconType
    {
        Regular,
        Filled
    }

    public static IReadOnlyCollection<FluentIcon> GetIcons(IconType iconType)
    {
        return iconType == IconType.Regular ? RegularIcons : FilledIcons;
    }
    public static string GetFontName(IconType iconType)
    {
        return iconType == IconType.Regular ? RegularIconFontName : FilledIconFontName;
    }
}