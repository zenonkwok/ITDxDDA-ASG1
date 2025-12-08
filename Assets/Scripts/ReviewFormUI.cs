using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReviewFormUI : MonoBehaviour
{
    [SerializeField] private XRHighlightOnSelect[] emojiHighlighters = new XRHighlightOnSelect[5];
    [SerializeField] private TMP_InputField commentField;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text statusText;

    private string locationId;
    private int selectedRating;
    private XRHighlightOnSelect currentlySelected;

    public void Initialize(string location)
    {
        locationId = location;

        for (int i = 0; i < emojiHighlighters.Length; i++)
        {
            int rating = i + 1;
            if (emojiHighlighters[i])
            {
                emojiHighlighters[i].gameObject.name = $"EmojiRating{rating}";
                var interactable = emojiHighlighters[i].GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
                if (interactable)
                {
                    interactable.selectEntered.AddListener(_ => SelectRating(rating));
                }
            }
        }

        if (submitButton) submitButton.onClick.AddListener(OnSubmit);
        if (closeButton) closeButton.onClick.AddListener(() => Destroy(gameObject));
    }

    private void SelectRating(int rating)
    {
        // Unhighlight previously selected emoji model.
        if (currentlySelected) currentlySelected.Unhighlight();

        // Highlight new selection.
        selectedRating = rating;
        if (emojiHighlighters[rating - 1])
        {
            currentlySelected = emojiHighlighters[rating - 1];
            currentlySelected.Highlight();
        }
    }

    private void OnSubmit()
    {
        if (selectedRating == 0)
        {
            if (statusText) statusText.text = "Select a rating";
            return;
        }

        var review = new ReviewData(locationId, selectedRating, commentField?.text ?? "", "Player");
        if (statusText) statusText.text = "Submitting...";

        ReviewSystem.Instance.SaveReview(review, success =>
        {
            if (statusText) statusText.text = success ? "Submitted!" : "Failed";
            if (success) Invoke(nameof(Close), 1.5f);
        });
    }

    private void Close() => Destroy(gameObject);
}
