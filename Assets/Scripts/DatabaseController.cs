using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using TMPro;

public class DatabaseController : MonoBehaviour
{
    public static DatabaseController Instance { get; private set; }

    [Header("Firebase")]
    [SerializeField]
    private bool autoInit = true;

    private Player myPlayer;

    public TMP_Text StatusTextField;
    public TMP_InputField Name;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (autoInit)
        {
            StartCoroutine(InitFirebase());
        }
    }

    private IEnumerator InitFirebase()
    {
        Debug.Log("[DatabaseController] Checking Firebase dependencies...");
        var checkTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => checkTask.IsCompleted);

        var status = checkTask.Result;
        if (status != DependencyStatus.Available)
        {
            Debug.LogWarning("[DatabaseController] Firebase dependencies not available: " + status);
            yield break;
        }

        Debug.Log("[DatabaseController] Firebase initialized and ready.");
    }

    // Public API: fetch the first review child under Restaurants/{restaurantKey}/Reviews
    public void GetFirstReviewForRestaurant(string restaurantKey, Action<ReviewModel> onComplete)
    {
        if (string.IsNullOrEmpty(restaurantKey))
        {
            onComplete?.Invoke(null);
            return;
        }

        var path = $"Restaurants/{restaurantKey}/Reviews";
        Debug.Log("[DatabaseController] Querying path: " + path);

        var dbRef = FirebaseDatabase.DefaultInstance.GetReference(path);
        dbRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogWarning("[DatabaseController] GetValueAsync faulted: " + task.Exception);
                onComplete?.Invoke(null);
                return;
            }

            if (!task.IsCompleted)
            {
                Debug.LogWarning("[DatabaseController] GetValueAsync did not complete.");
                onComplete?.Invoke(null);
                return;
            }

            var snapshot = task.Result;
            if (snapshot == null || !snapshot.Exists)
            {
                Debug.Log("[DatabaseController] No reviews found at path: " + path);
                onComplete?.Invoke(null);
                return;
            }

            // pick the first review child and map fields to ReviewModel
            foreach (var child in snapshot.Children)
            {
                if (child == null) continue;

                var id = child.Key;
                var userName = child.Child("UserName")?.Value?.ToString() ?? "";
                var remarks = child.Child("Remarks")?.Value?.ToString() ?? "";
                var ratingVal = child.Child("Rating")?.Value?.ToString() ?? "";

                var review = new ReviewModel
                {
                    id = id,
                    header = string.IsNullOrEmpty(ratingVal) ? "Review" : $"Rating: {ratingVal}",
                    author = string.IsNullOrEmpty(userName) ? "Anonymous" : userName,
                    content = string.IsNullOrEmpty(remarks) ? "No remarks." : remarks
                };

                Debug.Log($"[DatabaseController] Found review id={id}, user={userName}, rating={ratingVal}");
                onComplete?.Invoke(review);
                return; // only first child
            }

            onComplete?.Invoke(null);
        });
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Init()
    {
        var db = FirebaseDatabase.DefaultInstance.RootReference;

        var getPlayerTask = db.Child("players").Child("steviewonder").GetValueAsync();

        getPlayerTask.ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("Error loading player: " + (task.Exception != null ? task.Exception.Flatten().Message : "Unknown error"));
                return;
            }

            // Check if player data exists
            if (!task.Result.Exists)
            {
                Debug.Log("Player does not exist!");
                return;
            }

            // This is the JSON data of the player we just loaded
            string json = task.Result.GetRawJsonValue();
            Debug.Log(json);

            // Converting the json string back to Player object
            myPlayer = JsonUtility.FromJson<Player>(json);

            // Put the loaded player name into the input field's text (guard null)
            if (Name != null)
            {
                Name.text = myPlayer != null && !string.IsNullOrEmpty(myPlayer.name) ? myPlayer.name : "";
            }

            Debug.Log($"Player name is: {(myPlayer != null ? myPlayer.name : "<null>")}");
        });
    }
}