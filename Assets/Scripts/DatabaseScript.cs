using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;

/// <summary>
/// Simple Firebase Review Manager - matches your DB structure:
/// {LocationId}/Reviews/{ReviewId}/{Remarks, UserName, Rating}
/// </summary>
public class DatabaseScript : MonoBehaviour
{
    private static DatabaseScript _instance;
    public static DatabaseScript Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("DatabaseScript");
                _instance = obj.AddComponent<DatabaseScript>();
            }
            return _instance;
        }
    }

    private DatabaseReference dbRef;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFirebase()
    {
        try
        {
            dbRef = FirebaseDatabase.DefaultInstance.RootReference;
            Debug.Log("Firebase Database initialized successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize Firebase: {ex.Message}");
        }
    }

    /// <summary>
    /// Save a review to: {locationId}/Reviews/{autoId}
    /// </summary>
    public void SaveReview(string locationId, string remarks, string userName, int rating, Action<bool> onComplete = null)
    {
        Debug.Log($"[ReviewManager.SaveReview] Called with locationId={locationId}, userName={userName}, rating={rating}, remarks={remarks}");
        Debug.Log($"[ReviewManager.SaveReview] DbRef is {(dbRef != null ? "VALID" : "NULL")}");

        if (dbRef == null)
        {
            Debug.LogError("[ReviewManager.SaveReview] ERROR: Firebase database not initialized!");
            onComplete?.Invoke(false);
            return;
        }

        if (string.IsNullOrEmpty(locationId))
        {
            Debug.LogError("[ReviewManager.SaveReview] ERROR: LocationId is required");
            onComplete?.Invoke(false);
            return;
        }

        try
        {
            Debug.Log($"[ReviewManager.SaveReview] Building path: {locationId}/Reviews");
            var reviewsRef = dbRef.Child(locationId).Child("Reviews");
            var newReviewRef = reviewsRef.Push();
            var reviewId = newReviewRef.Key;

            Debug.Log($"[DatabaseScript.SaveReview] Generated review ID: {reviewId}");

            var reviewData = new Dictionary<string, object>
            {
                { "Remarks", remarks ?? "" },
                { "UserName", userName ?? "Anonymous" },
                { "Rating", Mathf.Clamp(rating, 1, 5) }
            };

            Debug.Log($"[DatabaseScript.SaveReview] Full path: /{locationId}/Reviews/{reviewId}");
            Debug.Log($"[DatabaseScript.SaveReview] Payload: Remarks='{remarks}', UserName='{userName}', Rating={rating}");
            Debug.Log($"[DatabaseScript.SaveReview] Calling SetValueAsync...");

            var task = newReviewRef.SetValueAsync(reviewData);
            Debug.Log($"[DatabaseScript.SaveReview] SetValueAsync task created");

            task.ContinueWithOnMainThread(t =>
            {
                Debug.Log($"[DatabaseScript.SaveReview] Task completed. IsFaulted={t.IsFaulted}, IsCanceled={t.IsCanceled}, IsCompleted={t.IsCompleted}");
                if (t.IsFaulted)
                {
                    Debug.LogError($"[DatabaseScript.SaveReview] FAULTED: {t.Exception?.Message}");
                    if (t.Exception != null)
                    {
                        foreach (var ex in t.Exception.InnerExceptions)
                        {
                            Debug.LogError($"[DatabaseScript.SaveReview] Inner exception: {ex.GetType().Name}: {ex.Message}");
                        }
                        // Print full stack trace for debugging
                        Debug.LogError($"[DatabaseScript.SaveReview] Full exception:\n{t.Exception}");
                    }
                    onComplete?.Invoke(false);
                    return;
                }

                if (t.IsCanceled)
                {
                    Debug.LogError($"[DatabaseScript.SaveReview] CANCELED");
                    onComplete?.Invoke(false);
                    return;
                }

                Debug.Log($"[DatabaseScript.SaveReview] SUCCESS! Review saved with ID: {reviewId}");
                onComplete?.Invoke(true);
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DatabaseScript.SaveReview] EXCEPTION: {ex.Message}\n{ex.StackTrace}");
            onComplete?.Invoke(false);
        }
    }

    /// <summary>
    /// Get all reviews for a location from: {locationId}/Reviews
    /// </summary>
    public void GetReviews(string locationId, Action<List<ReviewData>> onComplete)
    {
        if (dbRef == null || string.IsNullOrEmpty(locationId))
        {
            onComplete?.Invoke(new List<ReviewData>());
            return;
        }

        dbRef.Child(locationId).Child("Reviews").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            var reviews = new List<ReviewData>();

            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning($"Failed to fetch reviews: {task.Exception?.Message}");
                onComplete?.Invoke(reviews);
                return;
            }

            var snapshot = task.Result;
            if (snapshot.Exists)
            {
                foreach (var child in snapshot.Children)
                {
                    try
                    {
                        var remarks = child.Child("Remarks").Value?.ToString() ?? "";
                        var userName = child.Child("UserName").Value?.ToString() ?? "Anonymous";
                        var ratingValue = child.Child("Rating").Value;
                        int rating = 3;

                        if (ratingValue != null && int.TryParse(ratingValue.ToString(), out var parsedRating))
                        {
                            rating = parsedRating;
                        }

                        reviews.Add(new ReviewData
                        {
                            reviewId = child.Key,
                            remarks = remarks,
                            userName = userName,
                            rating = Mathf.Clamp(rating, 1, 5)
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to parse review: {ex.Message}");
                    }
                }

                Debug.Log($"Loaded {reviews.Count} reviews for {locationId}");
            }
            else
            {
                Debug.Log($"No reviews found for {locationId}");
            }

            onComplete?.Invoke(reviews);
        });
    }

    /// <summary>
    /// Get food stats for a location from: {locationId}/FoodStats
    /// Expected children: Ingredients, Taste, Description, OverallCustomerRating
    /// Supports nested items: {locationId}/FoodStats/{itemKey}/{fields}
    /// </summary>
    public void GetFoodStats(string locationId, Action<FoodStats> onComplete, string itemKey = null)
    {
        if (dbRef == null || string.IsNullOrEmpty(locationId))
        {
            onComplete?.Invoke(new FoodStats());
            return;
        }

        // Allow passing locationId like "Location/Item" or "Location:Item"
        var baseLocation = locationId;
        if (locationId.Contains("/") || locationId.Contains(":"))
        {
            var sep = locationId.Contains("/") ? '/' : ':';
            var parts = locationId.Split(sep);
            if (parts.Length >= 2)
            {
                baseLocation = parts[0];
                if (string.IsNullOrEmpty(itemKey))
                    itemKey = parts[1];
            }
        }

        dbRef.Child(baseLocation).Child("FoodStats").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            var result = new FoodStats();

            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning($"GetFoodStats failed for {locationId}: {task.Exception?.Message}");
                onComplete?.Invoke(result);
                return;
            }

            var snapshot = task.Result;
            if (!snapshot.Exists)
            {
                Debug.Log($"No FoodStats found for {baseLocation}");
                onComplete?.Invoke(result);
                return;
            }

            try
            {
                // Case A: FoodStats directly contains the expected fields
                if (snapshot.Child("Allergies").Exists || snapshot.Child("Taste").Exists ||
                    snapshot.Child("Description").Exists || snapshot.Child("OverallCustomerRating").Exists)
                {
                    result.allergies = snapshot.Child("Allergies").Value?.ToString() ?? "";
                    result.taste = snapshot.Child("Taste").Value?.ToString() ?? "";
                    result.description = snapshot.Child("Description").Value?.ToString() ?? "";
                    result.overallCustomerRating = snapshot.Child("OverallCustomerRating").Value?.ToString() ?? "";
                }
                else
                {
                    // Case B: FoodStats contains child items (e.g. "SteamedChickenRice")
                    DataSnapshot targetNode = null;

                    if (!string.IsNullOrEmpty(itemKey) && snapshot.Child(itemKey).Exists)
                    {
                        targetNode = snapshot.Child(itemKey);
                    }
                    else
                    {
                        // pick the first child that contains expected fields
                        foreach (var child in snapshot.Children)
                        {
                            if (child.Child("Allergies").Exists || child.Child("Taste").Exists ||
                                child.Child("Description").Exists || child.Child("OverallCustomerRating").Exists)
                            {
                                targetNode = child;
                                break;
                            }
                        }
                    }

                    if (targetNode != null)
                    {
                        result.allergies = targetNode.Child("Allergies").Value?.ToString() ?? "";
                        result.taste = targetNode.Child("Taste").Value?.ToString() ?? "";
                        result.description = targetNode.Child("Description").Value?.ToString() ?? "";
                        result.overallCustomerRating = targetNode.Child("OverallCustomerRating").Value?.ToString() ?? "";
                    }
                    else
                    {
                        Debug.LogWarning($"GetFoodStats: No suitable child node found under {baseLocation}/FoodStats");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error parsing FoodStats for {baseLocation}: {ex.Message}");
            }

            onComplete?.Invoke(result);
        });
    }
}

[Serializable]
public class ReviewData
{
    public string reviewId;
    public string remarks;
    public string userName;
    public int rating;

    public string GetRatingEmoji()
    {
        return rating switch
        {
            1 => "😡",
            2 => "😞",
            3 => "😐",
            4 => "😊",
            5 => "😍",
            _ => "😐"
        };
    }

    public bool IsPositive() => rating > 3;
    public bool IsNegative() => rating < 3;
}

[Serializable]
public class FoodStats
{
    // removed ingredients (does not exist in DB)
    public string allergies = "";
    public string taste = "";
    public string description = "";

    // changed to string so we can display raw values like "4.2/5"
    public string overallCustomerRating = "";

    // new: parse the stored string into a float (best-effort)
    public float GetOverallRatingFloat()
    {
        if (string.IsNullOrEmpty(overallCustomerRating))
            return 0f;

        // handle formats like "4.2/5" -> take the left side before '/'
        var raw = overallCustomerRating;
        var parts = raw.Split('/');
        var candidate = parts.Length > 0 ? parts[0].Trim() : raw.Trim();

        // try parse using invariant culture first
        if (float.TryParse(candidate, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var val))
            return val;

        // fallback: normalize commas to dots and strip other chars
        var sb = new System.Text.StringBuilder();
        foreach (var c in candidate)
        {
            if (char.IsDigit(c) || c == '.' || c == ',')
                sb.Append(c == ',' ? '.' : c);
        }

        if (float.TryParse(sb.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out val))
            return val;

        return 0f;
    }

    // return the stored string for display
    public string OverallRatingAsString(int decimals = 1)
    {
        return overallCustomerRating ?? "";
    }
}
