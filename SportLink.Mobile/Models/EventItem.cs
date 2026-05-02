using System;
using System.Collections.Generic;

namespace SportLink.Mobile.Models;

public class EventItem
{
    public string Title { get; set; } = "";

    public DateTime Date { get; set; } = DateTime.Now;

    public string Type { get; set; } = "";
    public string Time { get; set; } = "";
    public string Location { get; set; } = "";

    public string Description { get; set; } = "";

    public int ParticipantsCount { get; set; } = 0;
    public int MaxParticipants { get; set; } = 10;

    public bool IsPaid { get; set; }
    public double Price { get; set; }

    public bool IsCreatedByCurrentUser { get; set; } = true;
    public bool IsJoined { get; set; } = false;

    public List<ReviewItem> Reviews { get; set; } = new();

    public string SpotsLeft =>
    $"{MaxParticipants - ParticipantsCount} places left";
}