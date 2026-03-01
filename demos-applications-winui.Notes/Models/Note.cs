using System;

namespace demos_applications_winui.Notes.Models;

public class Note
{
    public string Filename { get; set; } = $"notes{DateTime.Now.ToBinary()}.txt";
    public string Text { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
}
