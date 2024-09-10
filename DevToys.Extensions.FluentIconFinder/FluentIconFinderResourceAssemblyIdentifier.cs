﻿using DevToys.Api;
using System.ComponentModel.Composition;

namespace DevToys.Extensions.FluentIconFinder;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(FluentIconFinderResourceAssemblyIdentifier))]
internal sealed class FluentIconFinderResourceAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        return ValueTask.FromResult<FontDefinition[]>([]);
    }
}