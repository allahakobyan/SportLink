namespace SportLink.Mobile.Models;

public class ReviewItem
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;

    public string Stars => new string('★', Rating) + new string('☆', 5 - Rating);
}
