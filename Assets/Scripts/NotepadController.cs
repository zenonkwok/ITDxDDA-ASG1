using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main notepad controller - handles screen switching and review viewing
/// </summary>
public class NotepadController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string locationId = "FoodClubChickenRice";

    [Header("Screen Management")]
    [SerializeField] private GameObject mainScreen;    // Shows "Rate Place" / "Read Reviews" buttons
    [SerializeField] private GameObject reviewScreen;   // Shows review form (emoji, comments, submit)

    [Header("Buttons")]
    [SerializeField] private Button ratePlaceButton;
    [SerializeField] private Button viewReviewsButton;
    [SerializeField] private Button closeReviewFormButton;

    [Header("Review Display (for later)")]
    [SerializeField] private GameObject reviewNotepadPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float horizontalSpacing = 1.5f;

    private System.Collections.Generic.List<GameObject> spawnedNotepads = new System.Collections.Generic.List<GameObject>();

    private void Start()
    {
        if (ratePlaceButton) ratePlaceButton.onClick.AddListener(ShowReviewScreen);
        if (viewReviewsButton) viewReviewsButton.onClick.AddListener(OnViewReviewsClicked);
        if (closeReviewFormButton) closeReviewFormButton.onClick.AddListener(ShowMainScreen);

        // Initially show main screen
        ShowMainScreen();

        // Pass location ID to ReviewForm if attached
        var reviewForm = GetComponentInChildren<ReviewForm>();
        if (reviewForm) reviewForm.SetLocationId(locationId);
    }

    private void ShowMainScreen()
    {
        Debug.Log("Showing main screen");
        if (mainScreen) mainScreen.SetActive(true);
        if (reviewScreen) reviewScreen.SetActive(false);
    }

    private void ShowReviewScreen()
    {
        Debug.Log("Showing review screen");
        if (mainScreen) mainScreen.SetActive(false);
        if (reviewScreen) reviewScreen.SetActive(true);
    }

    private void OnViewReviewsClicked()
    {
        Debug.Log($"View Reviews clicked - fetching reviews for {locationId}");
        // TODO: Implement review spawning later
    }

    public void SetLocationId(string newLocationId)
    {
        locationId = newLocationId;
        Debug.Log($"Notepad location set to: {locationId}");
    }
}
