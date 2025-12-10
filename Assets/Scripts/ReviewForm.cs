using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to the review form UI - handles emoji selection and submission
/// </summary>
public class ReviewForm : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string locationId = "FoodClubChickenRice";

    [Header("UI Elements")]
    [SerializeField] private XRHighlightOnSelect[] emojiHighlighters = new XRHighlightOnSelect[5];
    [SerializeField] private TMP_InputField commentsInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text statusText;

    private int selectedRating = 0;
    private XRHighlightOnSelect currentHighlighted;

    private void Awake()
    {
        Debug.Log("[ReviewForm.Awake] ReviewForm component is initializing!");
    }

    private void Start()
    {
        SetupEmojiButtons();
        
        Debug.Log($"[ReviewForm.Start] submitButton is {(submitButton != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"[ReviewForm.Start] closeButton is {(closeButton != null ? "ASSIGNED" : "NULL")}");
        
        if (submitButton)
        {
            submitButton.onClick.AddListener(OnSubmitClicked);
            Debug.Log("[ReviewForm.Start] Submit button listener added successfully");
        }
        else
        {
            Debug.LogError("[ReviewForm.Start] ERROR: submitButton is not assigned in Inspector!");
        }
        
        if (closeButton)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
            Debug.Log("[ReviewForm.Start] Close button listener added successfully");
        }
        else
        {
            Debug.LogWarning("[ReviewForm.Start] WARNING: closeButton is not assigned in Inspector!");
        }
    }

    private void SetupEmojiButtons()
    {
        for (int i = 0; i < emojiHighlighters.Length; i++)
        {
            if (emojiHighlighters[i] == null) continue;

            int rating = i + 1; // 1-5
            var interactable = emojiHighlighters[i].GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
            
            if (interactable)
            {
                interactable.selectEntered.AddListener(_ => SelectRating(rating));
            }
        }
    }

    private void SelectRating(int rating)
    {
        Debug.Log($"Rating selected: {rating}");

        // Unhighlight previous selection
        if (currentHighlighted) currentHighlighted.Unhighlight();

        // Highlight new selection
        selectedRating = rating;
        if (emojiHighlighters[rating - 1])
        {
            currentHighlighted = emojiHighlighters[rating - 1];
            currentHighlighted.Highlight();
        }

        if (statusText) statusText.text = $"Rating: {rating}/5";
    }

    private void OnSubmitClicked()
    {
        Debug.Log("[ReviewForm.OnSubmitClicked] Submit button clicked");
        Debug.Log($"[ReviewForm.OnSubmitClicked] Selected rating: {selectedRating}");

        if (selectedRating == 0)
        {
            ShowStatus("Please select a rating!");
            Debug.LogWarning("[ReviewForm.OnSubmitClicked] No rating selected");
            return;
        }

        string remarks = commentsInput ? commentsInput.text : "";
        string userName = "Player";

        ShowStatus("Submitting...");
        Debug.Log($"[ReviewForm.OnSubmitClicked] About to call ReviewManager.SaveReview");
        Debug.Log($"[ReviewForm.OnSubmitClicked] LocationId={locationId}, Rating={selectedRating}, UserName={userName}, Remarks={remarks}");
        Debug.Log($"[ReviewForm.OnSubmitClicked] ReviewManager.Instance exists: {ReviewManager.Instance != null}");

        ReviewManager.Instance.SaveReview(locationId, remarks, userName, selectedRating, success =>
        {
            Debug.Log($"[ReviewForm.OnSubmitClicked] SaveReview callback received with success={success}");

            if (success)
            {
                ShowStatus("Review submitted!");
                Debug.Log("[ReviewForm.OnSubmitClicked] Review submitted successfully!");
                // Do NOT destroy - let the screen switching handle it
            }
            else
            {
                ShowStatus("Failed to submit review");
                Debug.LogError("[ReviewForm.OnSubmitClicked] Review submission failed");
            }
        });
    }

    private void OnCloseClicked()
    {
        Debug.Log("Closing review form");
        // Do NOT destroy the prefab - your screen switching will handle visibility
    }

    private void ShowStatus(string message)
    {
        if (statusText) statusText.text = message;
        Debug.Log($"Status: {message}");
    }

    public void SetLocationId(string newLocationId)
    {
        locationId = newLocationId;
        Debug.Log($"ReviewForm location set to: {locationId}");
    }
}
