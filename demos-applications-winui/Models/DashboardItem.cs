using System;

namespace demos_applications_winui.Models;

public class DashboardItem
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Glyph { get; init; }
    public required Type PageType { get; init; }
}
