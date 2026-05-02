using System.Collections.Generic;

namespace SportLink.Mobile.Models;

public class PostItem
{
    public string Username { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;

    public string ProfileImageUrl { get; set; } = string.Empty;

    public int LikesCount { get; set; } = 0;
    public bool IsLiked { get; set; } = false;

    public List<CommentItem> Comments { get; set; } = new();

    public string CreatedAt { get; set; } = DateTime.Now.ToString("g");

    public bool HasImage => !string.IsNullOrWhiteSpace(ImageUrl);
    public bool HasProfileImage => !string.IsNullOrWhiteSpace(ProfileImageUrl);

    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Username))
                return "?";

            var parts = Username.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
                return parts[0].Substring(0, 1).ToUpper();

            return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();
        }
    }
}
