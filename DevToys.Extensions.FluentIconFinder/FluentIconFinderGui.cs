﻿using DevToys.Api;
using System.ComponentModel.Composition;
using static DevToys.Api.GUI;

namespace DevToys.Extensions.FluentIconFinder;

[Export(typeof(IGuiTool))]
[Name(nameof(FluentIconFinder))]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uea81',
    GroupName = "Graphic",
    ResourceManagerAssemblyIdentifier = nameof(FluentIconFinderResourceAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Extensions.FluentIconFinder.FluentIconFinder",
    ShortDisplayTitleResourceName = nameof(FluentIconFinder.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(FluentIconFinder.LongDisplayTitle),
    DescriptionResourceName = nameof(FluentIconFinder.Description),
    AccessibleNameResourceName = nameof(FluentIconFinder.AccessibleName))]
public class FluentIconFinderGui : IGuiTool
{
    private static readonly int _pageSize = 100;
    private int _offset = 0;
    private FluentIcon[] _icons = [];
    private readonly IUISingleLineTextInput _searchText = SingleLineTextInput().Title(FluentIconFinder.FindIconsTitle);
    private readonly IUISingleLineTextInput _codeText = SingleLineTextInput().Title(FluentIconFinder.CodeTitle).ReadOnly();
    private readonly IUISingleLineTextInput _iosText = SingleLineTextInput().Title(FluentIconFinder.IosNameTitle).ReadOnly();
    private readonly IUISingleLineTextInput _androidText = SingleLineTextInput().Title(FluentIconFinder.AndroidNameTitle).ReadOnly();
    private readonly IUIDataGrid _dataGrid = DataGrid()
        .Extendable()
        .WithColumns(FluentIconFinder.ColmunNameIcon, FluentIconFinder.ColumnNameName, FluentIconFinder.ColumnNameCode);
    private readonly IUIGridCell _selectedIconCell = Cell(GridRows.Row1, GridColumns.Column1, Icon("FluentSystemIcons", '\ueb87'));
    private readonly IUILabel _selectedIconLabel = Label().Text("Icon Name").Style(UILabelStyle.Body);
    private readonly IUISingleLineTextInput _offsetText = SingleLineTextInput().HideCommandBar().ReadOnly();

    private enum GridRows
    {
        Row1,
        Row2,
        Row3,
        Row4
    }
    private enum GridColumns
    {
        Column1,
        Column2,
        Column3,
        Column4
    }

    public UIToolView View => new(
        Grid()
            .Rows(
                (GridRows.Row1, Auto),
                (GridRows.Row2, Auto),
                (GridRows.Row3, Auto))
            .Columns(
                (GridColumns.Column1, new UIGridLength(1, UIGridUnitType.Fraction)))
            .Cells(
                Cell(
                    GridRows.Row1,
                    GridColumns.Column1,
                    _searchText.OnTextChanged(OnSearchTextChange)),
                Cell(
                    GridRows.Row2,
                    GridColumns.Column1,
                    Grid()
                        .Rows((GridRows.Row1, Auto))
                        .Columns(
                            (GridColumns.Column1, new UIGridLength(1, UIGridUnitType.Fraction)),
                            (GridColumns.Column2, new UIGridLength(1, UIGridUnitType.Fraction)),
                            (GridColumns.Column3, new UIGridLength(1, UIGridUnitType.Fraction)),
                            (GridColumns.Column4, new UIGridLength(1, UIGridUnitType.Fraction)))
                    .Cells(
                        Cell(
                            GridRows.Row1,
                            GridColumns.Column1,
                            Card(
                                Grid()
                                    .Rows((GridRows.Row1, Auto))
                                    .Columns(
                                        (GridColumns.Column1, Auto),
                                        (GridColumns.Column2, new UIGridLength(1, UIGridUnitType.Fraction)))
                                    .Cells(
                                        _selectedIconCell,
                                        Cell(
                                            GridRows.Row1,
                                            GridColumns.Column2,
                                            _selectedIconLabel)))),
                        Cell(GridRows.Row1, GridColumns.Column2, _codeText),
                        Cell(GridRows.Row1, GridColumns.Column3, _iosText),
                        Cell(GridRows.Row1, GridColumns.Column4, _androidText))),
                Cell(
                    GridRows.Row3,
                    GridColumns.Column1,
                    Stack()
                        .Horizontal()
                        .AlignHorizontally(UIHorizontalAlignment.Right)
                        .WithChildren(
                            Button()
                                .Icon("FluentSystemIcons", '\uedb0')
                                .OnClick(OnPagePrevClick),
                            _offsetText.Text(_offset.ToString()),
                            Button()
                                .Icon("FluentSystemIcons", '\uedb5')
                                .OnClick(OnPageNextClick))),
                Cell(
                    GridRows.Row4,
                    GridColumns.Column1,
                    _dataGrid.Title(FluentIconFinder.IconListTitle).OnRowSelected(OnRowSelected))));

    private void OnRowSelected(IUIDataGridRow? row)
    {
        if (row?.Value is FluentIcon icon)
        {
            _selectedIconCell.WithChild(
                Icon("FluentSystemIcons", icon.Code < 0xFFFF ? Convert.ToChar(icon.Code) : '\ueb87')
                    .Size(icon.Size));
            _selectedIconLabel.Text($"{icon.Name} ({icon.Size})");
            _codeText.Text(icon.Code < 0xFFFF ? icon.Code.ToString("x4") : icon.Code.ToString("x8"));
            _iosText.Text(icon.IosName);
            _androidText.Text(icon.AndroidName);
        }
    }

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        throw new NotImplementedException();
    }

    private void OnSearchTextChange(string text)
    {
        _offset = 0;
        Search(text);
    }

    private void OnPagePrevClick()
    {
        _offset -= _pageSize;
        if (_offset < 0) { _offset = 0; }
        Search(_searchText.Text, paging: true);
    }

    private void OnPageNextClick()
    {
        _offset += _pageSize;
        if (_offset > _icons.Length - _pageSize) { _offset = _icons.Length - _pageSize; }
        Search(_searchText.Text, paging: true);
    }

    private void Search(string text, bool paging = false)
    {
        if (!paging)
        {
            _offset = 0;
            _icons = FluentIcons.RegularIcons
                .Where(icon => icon.IosName.Contains(text, StringComparison.OrdinalIgnoreCase)).ToArray();
        }
        var rows = _icons
            .Skip(_offset)
            .Take(_pageSize)
            .Select(icon => Row(
                value: icon,
                Cell(
                    Icon("FluentSystemIcons", icon.Code < 0xFFFF ? Convert.ToChar(icon.Code) : '\ueb87')
                        .Size(icon.Size)
                        .AlignHorizontally(UIHorizontalAlignment.Center)),
                Cell(
                    Label().Text($"{icon.Name} ({icon.Size})")),
                Cell(
                    Label()
                        .Text(icon.Code < 0xFFFF ? icon.Code.ToString("x4") : icon.Code.ToString("x8")))));
        _offsetText.Text($"{_offset}-{Math.Min(_offset + _pageSize, _icons.Length)} / {_icons.Length}");
        _dataGrid.Rows.Clear();
        _dataGrid.Rows.AddRange(rows);
    }
}