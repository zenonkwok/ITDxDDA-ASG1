using System;
using UnityEngine;

[Serializable]
public class ReviewData
{
    public string id;
    public string locationId;
    public int rating;
    public string comment;
    public string author;
    public long timestamp;

    public ReviewData() { }

    public ReviewData(string locationId, int rating, string comment = "", string author = "Anonymous")
    {
        this.id = "";
        this.locationId = locationId;
        this.rating = Mathf.Clamp(rating, 1, 5);
        this.comment = comment ?? "";
        this.author = author ?? "Anonymous";
        this.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string GetRatingEmoji() => rating switch
    {
        1 => "😡",
        2 => "😞",
        3 => "😐",
        4 => "😊",
        5 => "😍",
        _ => "😐"
    };

    public bool IsPositive() => rating >= 4;
    public bool IsNegative() => rating <= 2;
}

public enum ReviewFilter { Positive, Negative, All }
