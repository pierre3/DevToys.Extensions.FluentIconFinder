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
        var resourceNameRegular = "DevToys.Extensions.FluentIconFinder.Assets.FluentSystemIcons-Regular.ttf";
        var streamRegular = assembly.GetManifestResourceStream(resourceNameRegular)!;
        var resourceNameFill = "DevToys.Extensions.FluentIconFinder.Assets.FluentSystemIcons-Filled.ttf";
        var streamFill = assembly.GetManifestResourceStream(resourceNameFill)!;

        return new([
            new FontDefinition(FluentIcons.RegularIconFontName, streamRegular),
            new FontDefinition(FluentIcons.FilledIconFontName, streamFill)
            ]);
    }
}