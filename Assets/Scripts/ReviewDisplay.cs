using UnityEngine;
using TMPro;

/// <summary>
/// Attach to each spawned review notepad to display review data
/// </summary>
public class ReviewDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text userNameText;
    [SerializeField] private TMP_Text remarksText;
    [SerializeField] private GameObject[] emojiModels; // 5 slots for ratings 1-5

    public void ShowReview(ReviewData review, int reviewNumber = -1)
    {
        try
        {
            if (review == null)
            {
                Debug.LogWarning("[ReviewDisplay] Null review data passed to ReviewDisplay");
                SetEmojiModelActive(-1);
                if (headerText) headerText.text = "";
                if (userNameText) userNameText.text = "";
                if (remarksText) remarksText.text = "";
                return;
            }

            Debug.Log($"[ReviewDisplay] Displaying review: {review.userName} - {review.rating}/5 - {review.remarks}");

            if (headerText)
            {
                if (reviewNumber > 0)
                {
                    headerText.text = $"Review #{reviewNumber}";
                }
                else
                {
                    headerText.text = "Review";
                }
            }

            // 3D emoji models: activate the one matching rating
            SetEmojiModelActive(review.rating);

            if (userNameText) 
            {
                var nameToShow = string.IsNullOrWhiteSpace(review.userName) ? "Unknown" : review.userName;
                userNameText.text = $"Author: {nameToShow}";
            }
            else
            {
                Debug.LogWarning("[ReviewDisplay] userNameText is not assigned!");
            }

            if (remarksText) 
            {
                remarksText.text = string.IsNullOrEmpty(review.remarks) 
                    ? "(No comment)" 
                    : review.remarks;
            }
            else
            {
                Debug.LogWarning("[ReviewDisplay] remarksText is not assigned!");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ReviewDisplay] Exception in ShowReview: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void SetEmojiModelActive(int rating)
    {
        if (emojiModels == null || emojiModels.Length == 0)
        {
            return;
        }

        // Deactivate all models first
        for (int i = 0; i < emojiModels.Length; i++)
        {
            if (emojiModels[i]) emojiModels[i].SetActive(false);
        }

        // Ratings expected 1-5; array index = rating-1
        if (rating >= 1 && rating <= emojiModels.Length)
        {
            var model = emojiModels[rating - 1];
            if (model) model.SetActive(true);
        }
    }
}
