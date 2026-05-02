namespace SportLink.Mobile.Models;

public class CommentItem
{
    public string Username { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = DateTime.Now.ToString("g");
}