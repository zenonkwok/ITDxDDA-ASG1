using UnityEngine;
using TMPro;

public class ReviewDisplayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text ratingEmoji;
    [SerializeField] private TMP_Text authorText;
    [SerializeField] private TMP_Text commentText;

    public void DisplayReview(ReviewData review)
    {
        if (review == null) return;
        if (ratingEmoji) ratingEmoji.text = review.GetRatingEmoji();
        if (authorText) authorText.text = review.author;
        if (commentText) commentText.text = string.IsNullOrEmpty(review.comment) ? "(No comment)" : review.comment;
    }
}
