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
    [SerializeField] public GameObject mainScreen;    // Shows "Rate Place" / "Read Reviews" buttons
    [SerializeField] private GameObject reviewScreen;   // Shows review form (emoji, comments, submit)

    [Header("Buttons")]
    [SerializeField] private Button ratePlaceButton;
    [SerializeField] private Button viewReviewsButton;
    [SerializeField] private Button closeReviewFormButton;
    [SerializeField] private Button exitReviewModeButton;

    [Header("Review Display (for later)")]
    [SerializeField] private GameObject reviewNotepadPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float horizontalSpacing = 1.5f;
    [SerializeField] private float verticalSpacing = 1.5f;

    private System.Collections.Generic.List<GameObject> spawnedNotepads = new System.Collections.Generic.List<GameObject>();
    private bool isReviewDisplayNotepad = false;
    private bool isViewingReviews = false;

    // Cache the starting pose (relative to parent/image) so we can restore after navigating back
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private Vector3 initialLocalScale;
    private bool hasCachedInitialPose = false;

    private void OnEnable()
    {
        CacheInitialPoseIfNeeded();
    }

    private void Start()
    {
        if (ratePlaceButton) ratePlaceButton.onClick.AddListener(ShowReviewScreen);
        if (viewReviewsButton) viewReviewsButton.onClick.AddListener(OnViewReviewsClicked);
        if (closeReviewFormButton) closeReviewFormButton.onClick.AddListener(ShowMainScreen);
        if (exitReviewModeButton) exitReviewModeButton.onClick.AddListener(ExitReviewMode);

        // Only show main screen if this is not a spawned review display notepad
        if (!isReviewDisplayNotepad)
        {
            ShowMainScreen();
        }

        // Pass location ID to ReviewForm if attached
        var reviewForm = GetComponentInChildren<ReviewForm>();
        if (reviewForm) reviewForm.SetLocationId(locationId);
    }

    private void ShowMainScreen()
    {
        Debug.Log("Showing main screen");
        if (!isViewingReviews)
        {
            if (!isReviewDisplayNotepad)
            {
                ResetNotepadTransform();
            }
            if (mainScreen) mainScreen.SetActive(true);
            if (reviewScreen) reviewScreen.SetActive(false);
        }
    }

    private void ShowReviewScreen()
    {
        Debug.Log("Showing review screen");
        if (mainScreen) mainScreen.SetActive(false);
        if (reviewScreen) reviewScreen.SetActive(true);
    }

    private void OnViewReviewsClicked()
    {
        Debug.Log($"[NotepadController] View Reviews clicked - fetching reviews for {locationId}");

        // Set viewing mode BEFORE clearing to prevent main screen from reactivating
        isViewingReviews = true;
        
        // Hide main screen and review screen immediately
        if (mainScreen) mainScreen.SetActive(false);
        if (reviewScreen) reviewScreen.SetActive(false);

        // Clear any existing spawned notepads
        foreach (var notepad in spawnedNotepads)
        {
            if (notepad) Destroy(notepad);
        }
        spawnedNotepads.Clear();

        if (!reviewNotepadPrefab)
        {
            Debug.LogError("[NotepadController] No review notepad prefab assigned!");
            return;
        }

        // Fetch reviews from Firebase
        try
        {
            if (DatabaseScript.Instance == null)
            {
                Debug.LogError("[NotepadController] DatabaseScript.Instance is null!");
                return;
            }

            DatabaseScript.Instance.GetReviews(locationId, reviews =>
            {
                try
                {
                    if (reviews == null)
                    {
                        Debug.LogError("[NotepadController] Reviews list is null!");
                        return;
                    }

                    Debug.Log($"[NotepadController] Received {reviews.Count} reviews from Firebase");

                    if (reviews.Count == 0)
                    {
                        SpawnPlaceholderNotepad();
                        return;
                    }

                    // Collect and randomize: pick random 2 positive and 2 negative
                    var allPositive = new System.Collections.Generic.List<ReviewData>();
                    var allNegative = new System.Collections.Generic.List<ReviewData>();

                    foreach (var review in reviews)
                    {
                        if (review == null)
                        {
                            Debug.LogWarning("[NotepadController] Skipping null review");
                            continue;
                        }

                        if (review.IsPositive()) allPositive.Add(review);
                        else if (review.IsNegative()) allNegative.Add(review);
                    }

                    Shuffle(allPositive);
                    Shuffle(allNegative);

                    var displayReviews = new System.Collections.Generic.List<ReviewData>();
                    for (int i = 0; i < allPositive.Count && i < 2; i++) displayReviews.Add(allPositive[i]);
                    for (int i = 0; i < allNegative.Count && i < 2; i++) displayReviews.Add(allNegative[i]);
            
                    // Shuffle combined so order varies as well
                    Shuffle(displayReviews);

            Debug.Log($"[NotepadController] Displaying {displayReviews.Count} reviews (picked randomly from {allPositive.Count} positive, {allNegative.Count} negative)");

            // Show first review on this (original) notepad
            if (displayReviews.Count > 0)
            {
                Debug.Log($"[NotepadController] Showing review on original notepad: {displayReviews[0].userName} - {displayReviews[0].rating}/5");
                // Find ReviewDisplay on this object or its children
                var display = GetComponent<ReviewDisplay>()
                              ?? GetComponentInChildren<ReviewDisplay>();

                if (display)
                {
                    display.ShowReview(displayReviews[0], 1);
                }
                else
                {
                    Debug.LogError("[NotepadController] ReviewDisplay component not found on original notepad or its children. Please add it to the notepad prefab.");
                }
            }

            // Spawn additional notepads for remaining reviews (3 more for total of 4)
            for (int i = 1; i < displayReviews.Count && i < 4; i++)
            {
                Debug.Log($"[NotepadController] Spawning notepad for review {i}: {displayReviews[i].userName} - {displayReviews[i].rating}/5");
                SpawnReviewNotepad(displayReviews[i], i, displayReviews.Count);
            }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[NotepadController] Exception in GetReviews callback: {ex.Message}\n{ex.StackTrace}");
                }
            });
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NotepadController] Exception calling GetReviews: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void ClearSpawnedNotepads()
    {
        foreach (var notepad in spawnedNotepads)
        {
            if (notepad) Destroy(notepad);
        }
        spawnedNotepads.Clear();
        isViewingReviews = false;
        
        // Clear the review display from the original notepad
        var display = GetComponent<ReviewDisplay>()
                      ?? GetComponentInChildren<ReviewDisplay>();
        if (display)
        {
            display.ShowReview(null);
        }
        
        if (!isReviewDisplayNotepad)
        {
            ResetNotepadTransform();
        }

        ShowMainScreen();
        Debug.Log("[NotepadController] Cleared all spawned notepads and restored main screen");
    }

    private void SpawnPlaceholderNotepad()
    {
        Debug.Log("[NotepadController] No reviews found, spawning placeholder");
        var pos = GetSpawnPosition(0, 1);
        var rot = spawnPoint ? spawnPoint.rotation : transform.rotation;
        var notepad = Instantiate(reviewNotepadPrefab, pos, rot);

        var display = notepad.GetComponent<ReviewDisplay>();
        if (display)
        {
            display.ShowReview(new ReviewData
            {
                remarks = "No reviews yet. Be the first!",
                userName = "System",
                rating = 3
            });
        }

        spawnedNotepads.Add(notepad);
    }

    private void SpawnReviewNotepad(ReviewData review, int index, int total)
    {
        Debug.Log($"[NotepadController] Spawning notepad {index + 1}/{total} for review by {review.userName}");
        var pos = GetSpawnPosition(index, total);
        var rot = spawnPoint ? spawnPoint.rotation : transform.rotation;
        var notepad = Instantiate(reviewNotepadPrefab, pos, rot);

        // Mark this as a review display notepad so it doesn't show main screen
        var notepadController = notepad.GetComponent<NotepadController>();
        if (notepadController)
        {
            notepadController.isReviewDisplayNotepad = true;
            
            // Disable the back button on spawned notepads (only original should have it)
            if (notepadController.exitReviewModeButton)
            {
                notepadController.exitReviewModeButton.gameObject.SetActive(false);
            }
        }

        var display = notepad.GetComponent<ReviewDisplay>();
        if (display == null)
        {
            display = notepad.GetComponentInChildren<ReviewDisplay>();
        }
        
        if (display)
        {
            display.ShowReview(review, index + 1);
        }
        else
        {
            Debug.LogWarning("[NotepadController] ReviewDisplay component not found on spawned notepad or its children!");
        }

        spawnedNotepads.Add(notepad);
    }

    private Vector3 GetSpawnPosition(int index, int total)
    {
        // 2x2 grid layout:
        // Index 0 (original notepad): bottom-left
        // Index 1: bottom-right (spawned)
        // Index 2: top-left (spawned)
        // Index 3: top-right (spawned)
        
        float xOffset = 0f;
        float zOffset = 0f;
        
        if (index == 1) // bottom-right
        {
            xOffset = horizontalSpacing;
            zOffset = 0f;
        }
        else if (index == 2) // top-left
        {
            xOffset = 0f;
            zOffset = verticalSpacing;
        }
        else if (index == 3) // top-right
        {
            xOffset = horizontalSpacing;
            zOffset = verticalSpacing;
        }

        if (spawnPoint)
        {
            return spawnPoint.TransformPoint(new Vector3(xOffset, 0f, zOffset));
        }

        return transform.TransformPoint(new Vector3(xOffset, 0f, zOffset));
    }

    private static void Shuffle<T>(System.Collections.Generic.IList<T> list)
    {
        if (list == null || list.Count <= 1) return;
        for (int i = list.Count - 1; i > 0; i--)
        {
            int k = UnityEngine.Random.Range(0, i + 1);
            T tmp = list[i];
            list[i] = list[k];
            list[k] = tmp;
        }
    }

    public void SetLocationId(string newLocationId)
    {
        locationId = newLocationId;
        Debug.Log($"Notepad location set to: {locationId}");
    }

    public bool ShouldPersist()
    {
        return isViewingReviews;
    }
    
    public void ExitReviewMode()
    {
        ClearSpawnedNotepads();
    }

    private void ResetNotepadTransform()
    {
        CacheInitialPoseIfNeeded();
        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;
        transform.localScale = initialLocalScale;
    }

    private void CacheInitialPoseIfNeeded()
    {
        if (hasCachedInitialPose) return;
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
        initialLocalScale = transform.localScale;
        hasCachedInitialPose = true;
    }
}
