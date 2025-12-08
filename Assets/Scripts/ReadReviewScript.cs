using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class ReadReviewScript : MonoBehaviour
{
    [SerializeField]
    private TMP_Text reviewHeader;
    [SerializeField]
    private TMP_Text reviewName;
    [SerializeField]
    private TMP_Text reviewText;

    [Header("Placeholder Text")]
    [SerializeField]
    private string placeholderHeader = "Reviews";
    [SerializeField]
    private string placeholderName = "Anonymous";
    [SerializeField]
    [TextArea]
    private string placeholderContent = "No reviews available.";

    [Header("Auto Load (for testing)")]
    [SerializeField]
    private bool autoLoadExample = false;
    [SerializeField]
    private string exampleReviewId = "example1";

    [Header("Database Controller")]
    [SerializeField]
    private bool useDatabaseController = true;
    [SerializeField]
    private string restaurantKey = "FoodClubChickenRice";

    private void Start()
    {
        Debug.Log("[ReadReviewScript] Start: applying placeholders");
        ApplyPlaceholders();

        if (autoLoadExample)
        {
            Debug.Log("[ReadReviewScript] Auto-loading review id: " + exampleReviewId);
            LoadReview(exampleReviewId);
        }

        if (useDatabaseController)
        {
            StartCoroutine(WaitForDatabaseControllerAndLoad());
        }
    }

    private IEnumerator WaitForDatabaseControllerAndLoad()
    {
        float timeout = 3f;
        float elapsed = 0f;

        while (DatabaseController.Instance == null && elapsed < timeout)
        {
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        if (DatabaseController.Instance == null)
        {
            Debug.LogWarning("[ReadReviewScript] DatabaseController not found within timeout — falling back to mock.");
            yield break;
        }

        Debug.Log("[ReadReviewScript] Requesting random review for: " + restaurantKey);
        LoadReviewsFromFirebaseRandom(restaurantKey);
    }

    public void ApplyPlaceholders()
    {
        Debug.Log("[ReadReviewScript] ApplyPlaceholders");
        if (reviewHeader != null) reviewHeader.text = placeholderHeader;
        if (reviewName != null) reviewName.text = placeholderName;
        if (reviewText != null) reviewText.text = placeholderContent;
    }

    public void Populate(ReviewModel data)
    {
        if (data == null)
        {
            Debug.LogWarning("[ReadReviewScript] Populate called with null data -> applying placeholders");
            ApplyPlaceholders();
            return;
        }

        Debug.Log($"[ReadReviewScript] Populate: id={data.id} header='{data.header}' author='{data.author}'");
        if (reviewHeader != null) reviewHeader.text = string.IsNullOrEmpty(data.header) ? placeholderHeader : data.header;
        if (reviewName != null) reviewName.text = string.IsNullOrEmpty(data.author) ? placeholderName : data.author;
        if (reviewText != null) reviewText.text = string.IsNullOrEmpty(data.content) ? placeholderContent : data.content;
    }

    public void LoadReview(string reviewId)
    {
        Debug.Log("[ReadReviewScript] LoadReview called with id: " + reviewId);
        StartCoroutine(FetchReviewFromDatabaseMock(reviewId));
    }

    private IEnumerator FetchReviewFromDatabaseMock(string reviewId)
    {
        Debug.Log("[ReadReviewScript] FetchReviewFromDatabaseMock: simulating network delay for id " + reviewId);
        yield return new WaitForSeconds(0.5f);

        var example = new ReviewModel
        {
            id = reviewId,
            header = "FoodClub Review",
            author = "Test User",
            content = "This is placeholder review text loaded from the mock database. Replace this method with a real DB query to populate live data."
        };

        Debug.Log("[ReadReviewScript] FetchReviewFromDatabaseMock: populated example data, calling Populate");
        Populate(example);
    }

    // Fetch all reviews under Restaurants/{restaurant}/Reviews, pick one at random, then Populate.
    private void LoadReviewsFromFirebaseRandom(string restaurant)
    {
        var path = $"Restaurants/{restaurant}/Reviews";
        Debug.Log("[ReadReviewScript] LoadReviewsFromFirebaseRandom: querying path: " + path);

        var dbRef = FirebaseDatabase.DefaultInstance.GetReference(path);
        dbRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogWarning("[ReadReviewScript] Firebase GetValueAsync faulted: " + task.Exception);
                return;
            }

            if (!task.IsCompleted)
            {
                Debug.LogWarning("[ReadReviewScript] Firebase task not completed.");
                return;
            }

            var snapshot = task.Result;
            if (snapshot == null || !snapshot.Exists)
            {
                Debug.Log("[ReadReviewScript] No reviews found at path: " + path);
                return;
            }

            var reviews = new List<ReviewModel>();
            foreach (var child in snapshot.Children)
            {
                if (child == null) continue;

                var id = child.Key;

                var userName = child.Child("UserName")?.Value?.ToString();
                if (string.IsNullOrEmpty(userName)) userName = placeholderName;

                var remarks = child.Child("Remarks")?.Value?.ToString();
                if (string.IsNullOrEmpty(remarks)) remarks = placeholderContent;

                // Rating handling: Firebase may return numeric types (long/double) or string
                int rating = 0;
                object ratingObj = child.Child("Rating")?.Value;
                if (ratingObj != null)
                {
                    if (ratingObj is long)
                    {
                        rating = (int)(long)ratingObj;
                    }
                    else if (ratingObj is double)
                    {
                        rating = (int)(double)ratingObj;
                    }
                    else
                    {
                        int.TryParse(ratingObj.ToString(), out rating);
                    }
                }

                var header = rating > 0 ? $"Rating: {rating}" : "Review";

                var review = new ReviewModel
                {
                    id = id,
                    header = header,
                    author = userName,
                    content = remarks
                };

                reviews.Add(review);
            }

            if (reviews.Count == 0)
            {
                Debug.Log("[ReadReviewScript] No reviews collected for random selection.");
                return;
            }

            int idx = UnityEngine.Random.Range(0, reviews.Count);
            var chosen = reviews[idx];
            Debug.Log($"[ReadReviewScript] Selected random review index={idx} id={chosen.id} author={chosen.author}");
            Populate(chosen);
        });
    }

    // Legacy direct single-first-child loader (kept for reference, not used by default)
    private void LoadReviewsFromFirebase(string restaurant)
    {
        var path = $"Restaurants/{restaurant}/Reviews";
        Debug.Log("[ReadReviewScript] LoadReviewsFromFirebase: querying path: " + path);

        var dbRef = FirebaseDatabase.DefaultInstance.GetReference(path);
        dbRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogWarning("[ReadReviewScript] Firebase GetValueAsync faulted: " + task.Exception);
                return;
            }

            if (!task.IsCompleted)
            {
                Debug.LogWarning("[ReadReviewScript] Firebase task not completed.");
                return;
            }

            var snapshot = task.Result;
            if (snapshot == null || !snapshot.Exists)
            {
                Debug.Log("[ReadReviewScript] No reviews found at path: " + path);
                return;
            }

            foreach (var child in snapshot.Children)
            {
                if (child == null) continue;

                var id = child.Key;
                var userName = child.Child("UserName")?.Value?.ToString() ?? placeholderName;
                var remarks = child.Child("Remarks")?.Value?.ToString() ?? placeholderContent;

                int rating = 0;
                object ratingObj = child.Child("Rating")?.Value;
                if (ratingObj != null)
                {
                    if (ratingObj is long)
                        rating = (int)(long)ratingObj;
                    else if (ratingObj is double)
                        rating = (int)(double)ratingObj;
                    else
                        int.TryParse(ratingObj.ToString(), out rating);
                }

                var review = new ReviewModel
                {
                    id = id,
                    header = rating > 0 ? $"Rating: {rating}" : "Review",
                    author = userName,
                    content = remarks
                };

                Debug.Log($"[ReadReviewScript] Firebase: found review id={id}, user={userName}, rating={rating}");
                Populate(review);
                break;
            }
        });
    }
}