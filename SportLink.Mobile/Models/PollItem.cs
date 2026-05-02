namespace SportLink.Mobile.Models;

public class PollItem
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int SelectedOptionIndex { get; set; } = -1;
    public string CreatedAt { get; set; } = string.Empty;
}