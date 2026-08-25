using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;

// The extension project directory. In CI it is passed as the first argument
// (cross-platform, cwd-independent); locally it falls back to targetProjectDir.txt,
// which the .csproj PostBuild target writes into the generator's output directory.
var targetDir = args.Length > 0
    ? args[0].TrimEnd([' ', '\r', '\n'])
    : File.ReadAllText("targetProjectDir.txt").TrimEnd([' ', '\r', '\n']);

// Intermediate JSON is extracted into a temp directory so the tool never leaves
// stray files in the current working directory (important when run from a repo root in CI).
var workDir = Directory.CreateTempSubdirectory("FluentIconGen").FullName;

var (version, zip) = await DownloadFluentSystemIconsZip();
try
{
    var root = zip.Entries.First().FullName;
    ExtractToFile(zip, root, "FluentSystemIcons-Filled.json", workDir);
    ExtractToFile(zip, root, "FluentSystemIcons-Filled.ttf", Path.Combine(targetDir, "Assets"));
    ExtractToFile(zip, root, "FluentSystemIcons-Regular.json", workDir);
    ExtractToFile(zip, root, "FluentSystemIcons-Regular.ttf", Path.Combine(targetDir, "Assets"));
}
finally
{
    zip.Dispose();
}

OutputVersionCs(targetDir, version);
OutputFluentIconsCs(
    Path.Combine(workDir, "FluentSystemIcons-Regular.json"),
    targetDir,
    "FluentIcons.Regular.cs",
    "RegularIconFontName = \"FluentSystemIcons-Regular\"",
    "RegularIcons");
OutputFluentIconsCs(
    Path.Combine(workDir, "FluentSystemIcons-Filled.json"),
    targetDir,
    "FluentIcons.Filled.cs",
    "FilledIconFontName = \"FluentSystemIcons-Filled\"",
"FilledIcons");

Directory.Delete(workDir, recursive: true);

static async Task<(string version, ZipArchive zip)> DownloadFluentSystemIconsZip()
{
    static T? DeserializeAnonimousObject<T>(string s, T obj) => JsonSerializer.Deserialize<T>(s);

    var url = "https://api.github.com/repos/microsoft/fluentui-system-icons/tags?per_page=100";
    var http = new HttpClient();
    http.Timeout = TimeSpan.FromMinutes(10);
    http.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    http.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
    http.DefaultRequestHeaders.Add("User-Agent", "DevToys.Extensions.FluentIconFinder");
    // Authenticate when a token is available (e.g. in CI) to avoid GitHub's
    // unauthenticated rate limit of 60 requests/hour per IP.
    var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
    if (!string.IsNullOrEmpty(token))
    {
        http.DefaultRequestHeaders.Authorization = new("Bearer", token);
    }
    var tagsJson = await http.GetStringAsync(url);
    var tags = DeserializeAnonimousObject(tagsJson, new[] { new { name = "", zipball_url = "" } })
        ?? throw new InvalidOperationException("Failed to read tags from the GitHub API.");
    // This is a monorepo whose tags mix package-prefixed names (e.g. "eslint-plugin-react-icons@0.0.1")
    // with the icon-font releases ("1.1.338"). Keep only pure version-number tags and take the highest.
    var latest = tags
        .Where(t => Regex.IsMatch(t.name, @"^\d+\.\d+\.\d+$"))
        .OrderByDescending(t => Version.Parse(t.name))
        .First();
    var iconVersion = latest.name;
    var zipUrl = latest.zipball_url;
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
    Dictionary<string, int>? data;
    using (var stream = File.OpenRead(jsonName))
    {
        data = JsonSerializer.Deserialize<Dictionary<string, int>>(stream);
    }
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
