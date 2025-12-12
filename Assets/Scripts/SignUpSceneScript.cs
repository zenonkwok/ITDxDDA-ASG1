using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.SceneManagement;


public class SignUpSceneScript : MonoBehaviour
{
    // Input field references. Assign TextMeshPro fields in the Inspector.
    [Header("TextMeshPro Input Fields")]
    [SerializeField] private TMP_InputField tmpEmailField;
    [SerializeField] private TMP_InputField tmpPasswordField;
    [Header("UI Status Output")]
    [SerializeField] private TMP_Text statusText;

    // Simple Firebase email/password sign-up for Unity.
    // Usage notes:
    // - Install the Firebase Unity SDK (Auth) and add to the project.
    // - Ensure Firebase is initialized (this method runs a dependency check automatically).
    // - Wire a UI Button to `OnSignUpButtonClicked()` in the Inspector.

    // Public programmatic wrapper you can call with explicit credentials.
    public void SignUpWithEmail(string email, string password)
    {
        _ = InitializeFirebaseAndSignUp(email, password);
    }

    // Called via a Unity UI Button (no parameters). Reads assigned input fields.
    public void OnSignUpButtonClicked()
    {
        string email = tmpEmailField != null ? tmpEmailField.text : "";
        string password = tmpPasswordField != null ? tmpPasswordField.text : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("No TMP_InputField assigned or fields are empty. Assign `tmpEmailField` and `tmpPasswordField` in the inspector and provide values.");
            return;
        }

        // Kick off the async sign-up (fire-and-forget from the UI event).
        _ = SignUpWithEmailAsync(email, password);
    }

    // Internal async entry (keeps public SignUpWithEmail sync-friendly).
    private async Task SignUpWithEmailAsync(string email, string password)
    {
        await InitializeFirebaseAndSignUp(email, password).ConfigureAwait(false);
    }

    private async Task InitializeFirebaseAndSignUp(string email, string password)
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync().ConfigureAwait(false);
        if (dependencyStatus == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            await SignUpUser(email, password).ConfigureAwait(false);
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        }
    }

    private Task SignUpUser(string email, string password)
    {
        var auth = FirebaseAuth.DefaultInstance;
        if (auth == null)
        {
            if (statusText != null) statusText.text = "Firebase Auth not available.";
            Debug.LogError("FirebaseAuth.DefaultInstance is null in SignUpUser().");
            return Task.CompletedTask;
        }

        try
        {
            var createTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            _ = createTask.ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    if (statusText != null) statusText.text = "Signup canceled.";
                    Debug.LogWarning("Signup task was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    var msg = task.Exception != null ? task.Exception.Flatten().Message : "Unknown error";
                    if (statusText != null) statusText.text = "Signup failed: " + msg;
                    Debug.LogError("Signup failed: " + msg);
                    return;
                }

                // Success
                var userCredential = task.Result;
                Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                    userCredential.User.Email, userCredential.User.UserId);
                if (statusText != null) statusText.text = "User created successfully.";

                // Load main menu on success (run on main thread via ContinueWithOnMainThread)
                try
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Failed to load MainMenu after signup: " + ex.Message);
                }
            });

            return createTask;
        }
        catch (System.Exception e)
        {
            if (statusText != null) statusText.text = "Failed to start signup: " + (e.Message ?? "Unknown error");
            Debug.LogError($"Error starting signup: {e.Message}");
            return Task.CompletedTask;
        }
    }
}