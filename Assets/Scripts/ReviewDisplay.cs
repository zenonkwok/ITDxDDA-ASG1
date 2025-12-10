using UnityEngine;
using TMPro;

/// <summary>
/// Attach to each spawned review notepad to display review data
/// </summary>
public class ReviewDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text emojiText;
    [SerializeField] private TMP_Text userNameText;
    [SerializeField] private TMP_Text remarksText;

    public void ShowReview(ReviewData review)
    {
        if (review == null)
        {
            Debug.LogWarning("Null review data passed to ReviewDisplay");
            return;
        }

        Debug.Log($"Displaying review: {review.userName} - {review.rating}/5 - {review.remarks}");

        if (emojiText) emojiText.text = review.GetRatingEmoji();
        if (userNameText) userNameText.text = review.userName;
        if (remarksText) remarksText.text = string.IsNullOrEmpty(review.remarks) 
            ? "(No comment)" 
            : review.remarks;
    }
}
