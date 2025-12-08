using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Main ReviewSystem MonoBehaviour - manages all review operations and Firebase.
/// Attach this to a GameObject in your scene (auto-creates as singleton if needed).
/// </summary>
public class ReviewSystem : MonoBehaviour
{
    private static ReviewSystem _instance;
    public static ReviewSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ReviewSystem>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("ReviewSystem");
                    _instance = obj.AddComponent<ReviewSystem>();
                }
            }
            return _instance;
        }
    }

    private DatabaseReference _dbRef;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private bool EnsureDatabase()
    {
        if (_dbRef == null)
        {
            _dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        }
        return _dbRef != null;
    }

    #region Firebase Operations
    public void SaveReview(ReviewData review, Action<bool> onComplete = null)
    {
        if (!EnsureDatabase()) { Debug.LogError("Firebase not initialized"); onComplete?.Invoke(false); return; }
        if (string.IsNullOrEmpty(review.locationId)) { Debug.LogError("No locationId"); onComplete?.Invoke(false); return; }

        try
        {
            if (string.IsNullOrEmpty(review.id)) review.id = _dbRef.Child(review.locationId).Child("Reviews").Push().Key;

            // Match DB format: {locationId}/{Reviews}/{reviewId}/{Remarks,UserName,Rating}
            var payload = new Dictionary<string, object>
            {
                { "Remarks", review.comment ?? string.Empty },
                { "UserName", review.author ?? "Anonymous" },
                { "Rating", review.rating }
            };

            var task = _dbRef.Child(review.locationId)
                             .Child("Reviews")
                             .Child(review.id)
                             .SetValueAsync(payload);
            _ = task.ContinueWithOnMainThread(t =>
            {
                if (t.IsFaulted || t.IsCanceled) { Debug.LogError("Save failed"); onComplete?.Invoke(false); return; }
                Debug.Log("Review saved: " + review.id);
                onComplete?.Invoke(true);
            });
        }
        catch (Exception ex) { Debug.LogException(ex); onComplete?.Invoke(false); }
    }

    public void GetReviewsForLocation(string locationId, Action<List<ReviewData>> onComplete)
    {
        if (!EnsureDatabase()) { onComplete?.Invoke(new List<ReviewData>()); return; }

        try
        {
            var task = _dbRef.Child(locationId).Child("Reviews").GetValueAsync();
            _ = task.ContinueWithOnMainThread(t =>
            {
                if (t.IsFaulted || t.IsCanceled) { onComplete?.Invoke(new List<ReviewData>()); return; }

                var reviews = new List<ReviewData>();
                if (t.Result != null && t.Result.Exists)
                {
                    foreach (var child in t.Result.Children)
                    {
                        try
                        {
                            var remarks = child.Child("Remarks")?.Value as string ?? "";
                            var userName = child.Child("UserName")?.Value as string ?? "Anonymous";
                            int rating = 3;
                            var ratingVal = child.Child("Rating")?.Value;
                            if (ratingVal != null && int.TryParse(ratingVal.ToString(), out var parsed)) rating = parsed;

                            reviews.Add(new ReviewData
                            {
                                id = child.Key,
                                locationId = locationId,
                                rating = Mathf.Clamp(rating, 1, 5),
                                comment = remarks,
                                author = userName,
                                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                            });
                        }
                        catch { }
                    }
                }
                onComplete?.Invoke(reviews);
            });
        }
        catch (Exception ex) { Debug.LogException(ex); onComplete?.Invoke(new List<ReviewData>()); }
    }

    public void GetRandomReviewsForLocation(string locationId, int count, ReviewFilter filter, Action<List<ReviewData>> onComplete)
    {
        GetReviewsForLocation(locationId, all =>
        {
            var filtered = filter switch
            {
                ReviewFilter.Positive => all.Where(r => r.IsPositive()).ToList(),
                ReviewFilter.Negative => all.Where(r => r.IsNegative()).ToList(),
                _ => all
            };

            var random = new List<ReviewData>(filtered);
            for (int i = random.Count - 1; i > 0; i--)
            {
                int idx = UnityEngine.Random.Range(0, i + 1);
                (random[i], random[idx]) = (random[idx], random[i]);
            }

            onComplete?.Invoke(random.Take(count).ToList());
        });
    }
    #endregion
}

