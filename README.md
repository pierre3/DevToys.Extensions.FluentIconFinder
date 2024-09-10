# DevToys.Extensions.FluentIconFinder

Search for Fluent System Icons to obtain character codes and icon name.

![fluent-icon-finder](https://raw.githubusercontent.com/pierre3/DevToys.Extensions.FluentIconFinder/master/img/fluent-icon-finder.png)

## How to Use
1. Enter a search term in the "Find icons" text box.
2. A list of icons containing the entered term in their names will be displayed.
3. Double-click on the row of the icon you want to retrieve from the list.
4. The character code and icon name of the selected icon will be entered into the text box. Copy and use this information as needed.

## Known Issues
- When selecting from the list with a single click, the content of the previously selected row, not the highlighted row, is displayed in the text box.
- Icons with surrogate pairs (4-byte character codes) do not display images. However, the character code and icon name can still be retrieved. This is because DevToys can only specify icons using the char type (2 bytes).
