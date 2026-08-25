# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A [DevToys 2.0](https://devtoys.app/) extension that lets users search [Fluent UI System Icons](https://github.com/microsoft/fluentui-system-icons) by name and copy their glyph code and icon names. Published as the NuGet package `DevToys.Extensions.FluentIconFinder`.

## Build & commands

```powershell
# Build the extension (also produces the NuGet package via GeneratePackageOnBuild)
dotnet build DevToys.Extensions.FluentIconFinder\DevToys.Extensions.FluentIconFinder.csproj -c Release

# Regenerate icon definitions from the latest upstream release (see "Updating icons" below).
# Pass the extension project dir explicitly (works cross-platform / from any cwd).
dotnet run --project FluentIconDefinitionsGenerator -- "$PWD\DevToys.Extensions.FluentIconFinder"
```

There are no unit tests. There is no run target inside this repo — the extension is exercised by installing the built `.nupkg` into DevToys (Manage Extensions → Install). Target framework is `net8.0`.

## Architecture

Two projects, no test project:

- **`DevToys.Extensions.FluentIconFinder`** — the shipped extension.
- **`FluentIconDefinitionsGenerator`** — a build-time console tool that generates most of the extension's icon source files. Not shipped.

### The extension

DevToys discovers everything through MEF (`System.ComponentModel.Composition`) `[Export]` attributes:

- **`FluentIconFinderGui.cs`** — the single tool. `[Export(typeof(IGuiTool))]`. Builds the whole UI declaratively via the `DevToys.Api.GUI` fluent DSL (`Grid()`, `Cell()`, `DataGrid()`, etc.) in the `View` property. Search filters `FluentIcons.GetIcons(type)` by `IosName.Contains(text)`, paginates client-side (`_pageSize = 100`), and renders each icon by mounting the glyph in a `FluentSystemIcons` font cell. Note `OnDataReceived` throws `NotImplementedException` — this tool doesn't accept inter-tool data. UTF-16 vs UTF-32 glyphs are formatted with `x4` vs `x8` based on `code < 0xFFFF`.
- **`FluentIconFinderResourceAssemblyIdentifier.cs`** — `[Export(typeof(IResourceAssemblyIdentifier))]`. Loads the two embedded `.ttf` fonts from `Assets/` as `FontDefinition`s so the DataGrid can render glyphs. The resource names are hardcoded full manifest paths (`DevToys.Extensions.FluentIconFinder.Assets.FluentSystemIcons-*.ttf`).
- **`FluentIconFinder.resx` / `.Designer.cs`** — localized UI strings, referenced by `nameof(...)` in the `[ToolDisplayInformation]` attribute and throughout the GUI.
- **`FluentIcon.cs`** — `record FluentIcon(string Name, string IosName, string AndroidName, int Size, int Code)`.
- **`FluentIcons.cs`** — hand-written partial: `IconType` enum plus `GetIcons`/`GetFontName` dispatch between Regular and Filled.

### Generated files (do not edit by hand)

These are overwritten by `FluentIconDefinitionsGenerator` and are the bulk of the diff on every version bump:

- **`FluentIcons.Regular.cs`** / **`FluentIcons.Filled.cs`** — the large arrays of `FluentIcon` literals plus the font-name constants (`RegularIconFontName`, `FilledIconFontName`). Partial classes extending `FluentIcons`.
- **`FluentIcons.Version.cs`** — `FluentIcons.Version` string, shown in the DataGrid title.
- **`Assets/FluentSystemIcons-Regular.ttf`** / **`-Filled.ttf`** — the embedded fonts.

## Updating icons (the recurring maintenance task)

The common change to this repo is bumping to a new Fluent icons release. `FluentIconDefinitionsGenerator/Program.cs`:

1. Queries the GitHub tags API for the latest `microsoft/fluentui-system-icons` release and downloads its zipball.
2. Extracts `FluentSystemIcons-{Regular,Filled}.json` (name→codepoint maps) and the matching `.ttf` fonts.
3. Parses each icon key like `ic_fluent_access_time_24_regular` into title / iOS name / Android name / size / code, and regenerates the four generated source files above into the extension project.

The generator resolves the extension project path from `args[0]` when given, otherwise falls back to `targetProjectDir.txt` (written into its output dir by the `.csproj` PostBuild step — this is the path used by a plain Visual Studio "Start"). CI and scripted runs pass the path explicitly and cross-platform:

```powershell
dotnet run --project FluentIconDefinitionsGenerator -- "<abs-path-to>\DevToys.Extensions.FluentIconFinder"
```

Set the `GITHUB_TOKEN` env var to authenticate the tags-API call and avoid GitHub's unauthenticated rate limit (optional locally, set automatically in CI). Intermediate JSON is extracted to a temp dir, so a scripted run leaves nothing but the 4 generated `.cs` files and 2 `.ttf` in the working tree.

The generator does **not** touch `<Version>`, `<PackageReleaseNotes>`, or the `README.md` Release Notes — CI handles those (see below). For a manual run, bump them by hand.

## CI/CD automation

Two GitHub Actions workflows automate the icon-sync → publish loop. Publishing keeps a human gate (PR merge) because NuGet publishes are irreversible.

- **`.github/workflows/update-icons.yml`** — weekly cron (+ manual dispatch) on `ubuntu-latest`. The trigger is conceptually "a new tag was published upstream", but GitHub Actions can't receive another repo's tag events, so the cron polls: it compares the upstream latest tag against the tag currently shipped (in `FluentIcons.Version.cs`) and only continues when a newer one exists (latency ≈ the poll interval — lower the cron for faster reaction). It then regenerates and, **only if the icon definition files `FluentIcons.Regular.cs`/`Filled.cs` actually change** (a moved tag with no icon change is skipped), bumps the package **minor** version (e.g. `1.15.0` → `1.16.0`), rewrites `<PackageReleaseNotes>` and the README Release Notes, and opens a PR via `peter-evans/create-pull-request`. Release-note lines follow the existing changelog notation, embedding the upstream **Fluent tag number** (e.g. `- Updated Fluent UI System Icons to version 1.1.325`).
- **`.github/workflows/publish.yml`** — triggers on push to `master` under `DevToys.Extensions.FluentIconFinder/**` (i.e. when an update PR merges). Gated on whether a git tag matching the `.csproj` `<Version>` already exists (tags use the repo's **bare** `1.16.0` convention, no `v` prefix). If not released, it `dotnet build -c Release`s (which packs via `GeneratePackageOnBuild` — note `dotnet pack` alone fails with NU5026 because the DevToys.Api pack targets need the build-generated `runtimeconfig.json`), `dotnet nuget push --skip-duplicate`es the nupkg from `bin/Release`, then creates the tag and a GitHub Release whose notes use the same changelog notation (embedding the upstream Fluent tag number).

Publishing uses **NuGet trusted publishing (OIDC)** — no static API key. `publish.yml` requests a GitHub OIDC token (`permissions: id-token: write`) and exchanges it for a short-lived key via `NuGet/login@v1`. This requires a matching trusted-publishing policy on nuget.org (owner `pierre3`, repo `DevToys.Extensions.FluentIconFinder`, workflow `publish.yml`, environment `NUGET_API_KEY`); the job declares `environment: NUGET_API_KEY` to satisfy it. The `NuGet/login` `user:` input is the nuget.org account username.

Other prerequisite: the repo setting *Allow GitHub Actions to create and approve pull requests* enabled (needed for `update-icons.yml` to open PRs).
