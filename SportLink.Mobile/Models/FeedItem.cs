namespace SportLink.Mobile.Models;

public class FeedItem
{
    public string ItemType { get; set; } = string.Empty;

    public PostItem? Post { get; set; }
    public EventItem? Event { get; set; }
    public AnnouncementItem? Announcement { get; set; }
    public PollItem? Poll { get; set; }

    public DateTime SortDate { get; set; }

    public bool IsPost => ItemType == "Post";
    public bool IsEvent => ItemType == "Event";
    public bool IsAnnouncement => ItemType == "Announcement";
    public bool IsPoll => ItemType == "Poll";
}