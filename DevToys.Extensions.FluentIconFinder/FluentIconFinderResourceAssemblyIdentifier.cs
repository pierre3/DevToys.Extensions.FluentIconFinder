using DevToys.Api;
using System.ComponentModel.Composition;
using System.Reflection;

namespace DevToys.Extensions.FluentIconFinder;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(FluentIconFinderResourceAssemblyIdentifier))]
internal sealed class FluentIconFinderResourceAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = "DevToys.Extensions.FluentIconFinder.Assets.FluentSystemIcons-Filled.ttf";

        Stream stream = assembly.GetManifestResourceStream(resourceName)!;

        return new([new FontDefinition(FluentIcons.FilledIconFontName, stream)]);
    }
}