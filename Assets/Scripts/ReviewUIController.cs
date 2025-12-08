using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReviewUIController : MonoBehaviour
{
    [SerializeField] private string locationId = "foodclub";
    [SerializeField] private Button writeReviewButton;
    [SerializeField] private Button viewReviewsButton;
    [SerializeField] private GameObject reviewFormPrefab;
    [SerializeField] private GameObject notepadPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float horizontalSpacing = 1.5f;
    [SerializeField] private GameObject introMenuRoot;
    [SerializeField] private GameObject reviewUiRoot;

    private readonly List<GameObject> spawnedNotepads = new List<GameObject>();

    private void Start()
    {
        if (writeReviewButton) writeReviewButton.onClick.AddListener(ShowReviewForm);
        if (viewReviewsButton) viewReviewsButton.onClick.AddListener(SpawnReviewNotepads);
    }

    private void ShowReviewForm()
    {
        // Toggle UI groups if assigned: hide intro menu, show review UI.
        if (introMenuRoot) introMenuRoot.SetActive(false);
        if (reviewUiRoot) reviewUiRoot.SetActive(true);

        if (!reviewFormPrefab) return;

        var form = Instantiate(reviewFormPrefab).GetComponent<ReviewFormUI>();
        if (form) form.Initialize(locationId);
    }

    private void SpawnReviewNotepads()
    {
        spawnedNotepads.ForEach(Destroy);
        spawnedNotepads.Clear();

        if (!notepadPrefab)
        {
            Debug.LogWarning("No notepad prefab assigned on ReviewUIController.");
            return;
        }

        ReviewSystem.Instance.GetRandomReviewsForLocation(locationId, 2, ReviewFilter.Positive, positive =>
        {
            ReviewSystem.Instance.GetRandomReviewsForLocation(locationId, 2, ReviewFilter.Negative, negative =>
            {
                var allReviews = new List<ReviewData>(positive);
                allReviews.AddRange(negative);

                // If nothing returned, spawn a placeholder notepad.
                if (allReviews.Count == 0)
                {
                    var pos = GetOffsetPosition(0, 1);
                    var rot = spawnPoint ? spawnPoint.rotation : transform.rotation;
                    var notepad = Instantiate(notepadPrefab, pos, rot);
                    var display = notepad.GetComponent<ReviewDisplayUI>();
                    if (display)
                    {
                        display.DisplayReview(new ReviewData
                        {
                            rating = 3,
                            comment = "No reviews yet. Be the first!",
                            author = "System",
                            locationId = locationId,
                            id = "placeholder"
                        });
                    }
                    spawnedNotepads.Add(notepad);
                    return;
                }

                int total = allReviews.Count;
                for (int i = 0; i < total; i++)
                {
                    var pos = GetOffsetPosition(i, total);
                    var rot = spawnPoint ? spawnPoint.rotation : transform.rotation;
                    var notepad = Instantiate(notepadPrefab, pos, rot);

                    var display = notepad.GetComponent<ReviewDisplayUI>();
                    if (display) display.DisplayReview(allReviews[i]);

                    spawnedNotepads.Add(notepad);
                }
            });
        });
    }

    private Vector3 GetOffsetPosition(int index, int total)
    {
        // If only one review, still push it to the side of the main notepad.
        float offset = (total == 1)
            ? horizontalSpacing
            : index * horizontalSpacing - (total - 1) * 0.5f * horizontalSpacing;

        if (spawnPoint)
        {
            return spawnPoint.TransformPoint(new Vector3(offset, 0f, 0f));
        }

        return transform.TransformPoint(new Vector3(offset, 0f, 0f));
    }

    public void SetLocationId(string id) => locationId = id;
}
