﻿# DevToys.Extensions.FluentIconFinder
[![NuGet version](https://badge.fury.io/nu/DevToys.Extensions.FluentIconFinder.svg)](https://badge.fury.io/nu/DevToys.Extensions.FluentIconFinder)

This is an extension for [DevToys](https://devtoys.app/) 2.0.

Search for [Fluent UI System Icons](https://github.com/microsoft/fluentui-system-icons) to obtain glyph codes and icon name.

![fluent-icon-finder](https://raw.githubusercontent.com/pierre3/DevToys.Extensions.FluentIconFinder/master/img/fluent-icon-finder.png)

## Installation

1. Download the `DevToys.Extensions.FluentIconFinder` NuGet package from [NuGet.org](https://www.nuget.org/packages/DevToys.Extensions.FluentIconFinder).
1. In DevToys, open `Manager Extensions`, click on `Install` and select the downloaded NuGet package.

## How to Use
1. Select the icon type from the 'Icon Type' dropdown menu, choosing either 'Regular' or 'Filled'.
2. Enter a search term in the "Find icons" text box.
3. A list of icons containing the entered term in their names will be displayed.
4. Double-click on the row of the icon you want to retrieve from the list.
5. The glyph code and icon name of the selected icon will be entered into the text box. Copy and use this information as needed.

## Known Issues
- When selecting from the list with a single click, the content of the previously selected row, not the highlighted row, is displayed in the text box.

## Release Notes
[1.7.0]
- Updated Fluent UI System Icons to version 1.1.302

[1.7.0]
- Updated Fluent UI System Icons to version 1.1.300

[1.6.0]
- Updated Fluent UI System Icons to version 1.1.298

[1.5.0]
- Updated Fluent UI System Icons to version 1.1.297

[1.4.0]
- Updated Fluent UI System Icons to version 1.1.293

[1.3.0]
- Updated Fluent UI System Icons to version 1.1.271

[1.2.0]
- Support for UTF-32 icon glyphs.
- Updated Fluent UI System Icons to version 1.1.265
- Compatible DevToys version: DevToys 2.0-preview6 (2.0.6.0) or later
 
[1.1.1]

- Updated Fluent UI System Icons to version 1.1.264

[1.1.0]

- Added Filled type icons. You can now switch between Regular and Filled types.
- Fixed incorrect glyph codes for some icons.
