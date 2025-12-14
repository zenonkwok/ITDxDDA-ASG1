using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using System; // for Environment.StackTrace
// Editor-only prefab diagnostics
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Attach to the review form UI - handles emoji selection and submission
/// </summary>
public class ReviewForm : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string locationId = "";

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
        Debug.Log($"[ReviewForm.Awake] InstanceID={GetInstanceID()} GameObject='{gameObject.name}' initial locationId='{locationId}'");
    }

    // Called in editor when serialized properties change in the inspector
    private void OnValidate()
    {
        Debug.Log($"[ReviewForm.OnValidate] InstanceID={GetInstanceID()} GameObject='{gameObject.name}' locationId='{locationId}'");
#if UNITY_EDITOR
        var status = PrefabUtility.GetPrefabInstanceStatus(gameObject);
        var source = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
        Debug.Log($"[ReviewForm.OnValidate] PrefabInstanceStatus={status} PrefabSourceName={(source != null ? source.name : "NULL")}");
#endif
    }

    // Called when the component is Reset in Inspector or first added
    private void Reset()
    {
        Debug.Log($"[ReviewForm.Reset] InstanceID={GetInstanceID()} GameObject='{gameObject.name}' locationId='{locationId}'");
    }

    private void Start()
    {
        SetupEmojiButtons();
        
        Debug.Log($"[ReviewForm.Start] submitButton is {(submitButton != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"[ReviewForm.Start] closeButton is {(closeButton != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"[ReviewForm.Start] InstanceID={GetInstanceID()} GameObject='{gameObject.name}' current locationId='{locationId}'");

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
        
        // Get the current user's email from Firebase and extract the username part
        string userName = GetEmailUsername();

        // Diagnostic logs to trace locationId and other instances
        Debug.Log($"[ReviewForm.OnSubmitClicked] InstanceID={GetInstanceID()} GameObject='{gameObject.name}' locationId='{locationId}' IsNullOrWhiteSpace={string.IsNullOrWhiteSpace(locationId)}");
        var all = FindObjectsByType<ReviewForm>(FindObjectsSortMode.None);
        Debug.Log($"[ReviewForm.OnSubmitClicked] Found {all.Length} ReviewForm instance(s) in scene:");
        for (int i = 0; i < all.Length; i++)
        {
            Debug.Log($"  [{i}] InstanceID={all[i].GetInstanceID()} GameObject='{all[i].gameObject.name}' locationId='{all[i].locationId}'");
        }

        ShowStatus("Submitting...");
        Debug.Log($"[ReviewForm.OnSubmitClicked] About to call DatabaseScript.SaveReview");
        Debug.Log($"[ReviewForm.OnSubmitClicked] LocationId={locationId}, Rating={selectedRating}, UserName={userName}, Remarks={remarks}");
        Debug.Log($"[ReviewForm.OnSubmitClicked] DatabaseScript.Instance exists: {DatabaseScript.Instance != null}");

        DatabaseScript.Instance.SaveReview(locationId, remarks, userName, selectedRating, success =>
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
        string prev = locationId;
        locationId = newLocationId;
        Debug.Log($"[ReviewForm.SetLocationId] InstanceID={GetInstanceID()} GameObject='{gameObject.name}' previous='{prev}' new='{locationId}'\nStackTrace:\n{Environment.StackTrace}");
    }

    /// <summary>
    /// Gets the current logged-in user's email and extracts the username part (before @)
    /// </summary>
    private string GetEmailUsername()
    {
        try
        {
            var auth = FirebaseAuth.DefaultInstance;
            if (auth == null || auth.CurrentUser == null)
            {
                Debug.LogWarning("[ReviewForm.GetEmailUsername] Firebase Auth not available or no user logged in");
                return "Anonymous";
            }

            string email = auth.CurrentUser.Email;
            if (string.IsNullOrEmpty(email))
            {
                Debug.LogWarning("[ReviewForm.GetEmailUsername] User email is empty");
                return "Anonymous";
            }

            // Extract the part before @
            int atIndex = email.IndexOf('@');
            if (atIndex > 0)
            {
                string username = email.Substring(0, atIndex);
                Debug.Log($"[ReviewForm.GetEmailUsername] Extracted username: {username} from email: {email}");
                return username;
            }

            Debug.LogWarning($"[ReviewForm.GetEmailUsername] Email format invalid: {email}");
            return "Anonymous";
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ReviewForm.GetEmailUsername] Exception: {ex.Message}");
            return "Anonymous";
        }
    }
}

