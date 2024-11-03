using System.IO.Compression;
using System.Text.Json;

var targetDir = File.ReadAllText("targetProjectDir.txt").TrimEnd([' ', '\r', '\n']);

var (version, zip) = await DownloadFluentSystemIconsZip();
try
{
    var root = zip.Entries.First().FullName;
    ExtractToFile(zip, root, "FluentSystemIcons-Filled.json", ".");
    ExtractToFile(zip, root, "FluentSystemIcons-Filled.ttf", Path.Combine(targetDir, "Assets"));
    ExtractToFile(zip, root, "FluentSystemIcons-Regular.json", ".");
    ExtractToFile(zip, root, "FluentSystemIcons-Regular.ttf", Path.Combine(targetDir, "Assets"));
}
finally
{
    zip.Dispose();
}

OutputVersionCs(targetDir, version);
OutputFluentIconsCs(
    "FluentSystemIcons-Regular.json",
    targetDir,
    "FluentIcons.Regular.cs",
    "RegularIconFontName = \"FluentSystemIcons-Regular\"",
    "RegularIcons");
OutputFluentIconsCs(
    "FluentSystemIcons-Filled.json",
    targetDir,
    "FluentIcons.Filled.cs",
    "FilledIconFontName = \"FluentSystemIcons-Filled\"",
"FilledIcons");

static async Task<(string version, ZipArchive zip)> DownloadFluentSystemIconsZip()
{
    static T? DeserializeAnonimousObject<T>(string s, T obj) => JsonSerializer.Deserialize<T>(s);

    var url = "https://api.github.com/repos/microsoft/fluentui-system-icons/tags?per_page=1";
    var http = new HttpClient();
    http.Timeout = TimeSpan.FromMinutes(5);
    http.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    http.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
    http.DefaultRequestHeaders.Add("User-Agent", "DevToys.Extensions.FluentIconFinder");
    var tagsJson = await http.GetStringAsync(url);
    var tags = DeserializeAnonimousObject(tagsJson, new[] { new { name = "", zipball_url = "" } });
    var iconVersion = tags[0].name;
    var zipUrl = tags[0].zipball_url;
    var request = new HttpRequestMessage(HttpMethod.Get, zipUrl);
    request.Headers.Accept.Clear();
    var response = await http.SendAsync(request);
    var stream = await response.Content.ReadAsStreamAsync();
    var zip = new ZipArchive(stream);
    return (iconVersion, zip);
}

static void ExtractToFile(ZipArchive zip, string root, string fileName, string outputDir)
{
    var entry = zip.GetEntry($"{root}fonts/{fileName}");
    entry?.ExtractToFile(Path.Combine(outputDir, fileName), overwrite: true);
}

static void OutputVersionCs(string targetDir, string version)
{
    var source = $$"""
        namespace DevToys.Extensions.FluentIconFinder;
        
        public static partial class FluentIcons
        {
            public static readonly string Version = "{{version}}";
        }
        """;
    File.WriteAllText(Path.Combine(targetDir, "FluentIcons.Version.cs"), source);
}

static bool OutputFluentIconsCs(string jsonName, string targetDir, string targetCsName, string fontNameDefs, string propertyName)
{
    var stream = File.OpenRead(jsonName);
    var data = JsonSerializer.Deserialize<Dictionary<string, int>>(stream);
    if (data is null)
    {
        Console.WriteLine($"Error: {jsonName}");
        return false;
    }
    var icons = data.Select(kvp =>
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
    {{string.Join("," + Environment.NewLine, icons)}}        
        ];
    }
    """;
    File.WriteAllText(Path.Combine(targetDir, targetCsName), regularCs);

    return true;
}
