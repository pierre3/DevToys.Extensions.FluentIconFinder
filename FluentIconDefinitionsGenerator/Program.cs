using System.Text.Json;

OutputFluentIconsCs(
    "FluentSystemIcons-Regular.json",
    "FluentIcons.Regular.cs",
    "RegularIconFontName = \"FluentSystemIcons\"",
    "RegularIcons");
OutputFluentIconsCs(
    "FluentSystemIcons-Filled.json",
    "FluentIcons.Filled.cs",
    "FilledIconFontName = \"FluentSystemIcons-Filled\"",
    "FilledIcons");


static bool OutputFluentIconsCs(string jsonName, string targetCsName, string fontNameDefs, string propertyName)
{

    var targetDir = File.ReadAllText("targetProjectDir.txt").TrimEnd([' ', '\r', '\n']);
    var stream = File.OpenRead(jsonName);
    var data = JsonSerializer.Deserialize<Dictionary<string, int>>(stream);
    if (data is null)
    {
        Console.WriteLine($"Error: {jsonName}");
        return false;
    }
    var regularIcons = data.Select(kvp =>
    {
        var parts = kvp.Key.Split('_');
        var title = string.Join(" ", parts[2..^2].Select(s => char.ToUpper(s[0]) + s[1..]));
        var size = int.Parse(parts[^2]);
        var ios = string.Concat([parts[2], .. (parts[3..].Select(s => char.ToUpper(s[0]) + s[1..]))]);
        return $"        new(\"{title}\", \"{ios}\", \"{kvp.Key}\", {size}, {kvp.Value})";
    });
    var regularCs = $$"""
    namespace DevToys.Extensions.FluentIconFinder;

    public static partial class FluentIcons
    {
        public static readonly string {{fontNameDefs}};

        public static IReadOnlyList<FluentIcon> {{propertyName}} { get; } = [
    {{string.Join("," + Environment.NewLine, regularIcons)}}        
        ];
    }
    """;
    File.WriteAllText(Path.Combine(targetDir, targetCsName), regularCs);

    return true;
}
