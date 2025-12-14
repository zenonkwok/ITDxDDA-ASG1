using System.Threading.Tasks;
using System.Text.RegularExpressions;
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
        try
        {
            string email = tmpEmailField != null ? tmpEmailField.text : "";
            string password = tmpPasswordField != null ? tmpPasswordField.text : "";

            // Trim whitespace from inputs
            email = email.Trim();
            password = password.Trim();

            Debug.Log($"[SignUpSceneScript.OnSignUpButtonClicked] Email input: '{email}', Password length: {password.Length}");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                if (statusText != null) statusText.text = "Please enter both email and password.";
                Debug.LogWarning("Email or password field is empty.");
                return;
            }

            if (!IsValidEmail(email))
            {
                Debug.LogWarning($"[SignUpSceneScript.OnSignUpButtonClicked] Email validation failed for: '{email}'");
                if (statusText != null) statusText.text = "Please enter a valid email address.";
                return;
            }

            if (password.Length < 6)
            {
                if (statusText != null) statusText.text = "Password must be at least 6 characters long.";
                Debug.LogWarning("Password is too short.");
                return;
            }

            Debug.Log($"[SignUpSceneScript.OnSignUpButtonClicked] Validation passed. Proceeding with sign-up.");
            // Kick off the async sign-up (fire-and-forget from the UI event).
            _ = SignUpWithEmailAsync(email, password);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SignUpSceneScript.OnSignUpButtonClicked] Unexpected error: {ex.Message}\n{ex.StackTrace}");
            if (statusText != null) statusText.text = "An error occurred. Please try again.";
        }
    }

    // Internal async entry (keeps public SignUpWithEmail sync-friendly).
    private async Task SignUpWithEmailAsync(string email, string password)
    {
        try
        {
            Debug.Log("[SignUpSceneScript.SignUpWithEmailAsync] Starting async sign-up...");
            // Resume on Unity main thread to safely update UI
            await InitializeFirebaseAndSignUp(email, password);
            Debug.Log("[SignUpSceneScript.SignUpWithEmailAsync] Async sign-up completed.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SignUpSceneScript.SignUpWithEmailAsync] Exception caught: {ex.Message}\n{ex.StackTrace}");
            if (statusText != null) statusText.text = "Sign-up failed. Please try again.";
        }
    }

    private async Task InitializeFirebaseAndSignUp(string email, string password)
    {
        try
        {
            Debug.Log("[SignUpSceneScript.InitializeFirebaseAndSignUp] Checking Firebase dependencies...");
            // Keep continuation on main thread to allow UI updates
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                Debug.Log("[SignUpSceneScript.InitializeFirebaseAndSignUp] Firebase dependencies available, calling SignUpUser...");
                // Schedule signup handling on main thread and avoid awaiting the faulting task
                // so expected failures (e.g., email already in use) don't bubble as exceptions here.
                _ = SignUpUser(email, password);
            }
            else
            {
                Debug.LogError($"[SignUpSceneScript.InitializeFirebaseAndSignUp] Could not resolve all Firebase dependencies: {dependencyStatus}");
                if (statusText != null) statusText.text = "Firebase initialization failed. Please try again.";
            }
        }
        catch (System.Exception ex)
        {
            // Downgrade expected signup failures (e.g., email already in use) to warnings
            var msg = ex.Message ?? "Unknown error";
            if (msg.Contains("already in use") || msg.Contains("EMAIL_EXISTS") || msg.Contains("email-already-in-use"))
            {
                var friendly = "This email is already registered. Please log in or use a different email.";
                Debug.LogWarning($"[SignUpSceneScript.InitializeFirebaseAndSignUp] {friendly}");
                if (statusText != null) statusText.text = friendly;
            }
            else
            {
                Debug.LogError($"[SignUpSceneScript.InitializeFirebaseAndSignUp] Exception: {ex.Message}\n{ex.StackTrace}");
                if (statusText != null) statusText.text = "An error occurred. Please try again.";
            }
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
                try
                {
                    if (task.IsCanceled)
                    {
                        if (statusText != null) statusText.text = "Signup canceled.";
                        Debug.LogWarning("Signup task was canceled.");
                        return;
                    }

                    if (task.IsFaulted)
                    {
                        string userFriendlyMsg = GetFriendlyErrorMessage(task.Exception);
                        if (statusText != null) statusText.text = userFriendlyMsg;
                        Debug.LogWarning($"Signup failed: {userFriendlyMsg}");
                        return;
                    }

                    // Success - only load scene on successful account creation
                    var userCredential = task.Result;
                    if (userCredential != null && userCredential.User != null)
                    {
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
                            if (statusText != null) statusText.text = "Signup successful, but failed to load next scene. Please try again.";
                        }
                    }
                    else
                    {
                        if (statusText != null) statusText.text = "Signup failed: User credential is null.";
                        Debug.LogError("User credential is null after successful signup task.");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[SignUpUser] Exception in ContinueWithOnMainThread: {ex.Message}\n{ex.StackTrace}");
                    if (statusText != null) statusText.text = "An error occurred during signup. Please try again.";
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

    /// <summary>
    /// Validates email format
    /// </summary>
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        
        // Trim whitespace
        email = email.Trim();
        
        try
        {
            // Simple check: must contain @ and at least one dot after @
            if (!email.Contains("@"))
                return false;

            int atIndex = email.LastIndexOf("@");
            if (atIndex < 1 || atIndex == email.Length - 1)
                return false;

            string afterAt = email.Substring(atIndex + 1);
            if (!afterAt.Contains("."))
                return false;

            // Ensure there's at least one character between @ and .
            int dotIndex = afterAt.IndexOf(".");
            if (dotIndex < 1 || dotIndex == afterAt.Length - 1)
                return false;

            Debug.Log($"[SignUpSceneScript.IsValidEmail] Email validation passed for: {email}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SignUpSceneScript.IsValidEmail] Exception during validation: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Converts Firebase Auth exceptions into user-friendly error messages
    /// </summary>
    private string GetFriendlyErrorMessage(System.AggregateException exception)
    {
        if (exception == null)
            return "Signup failed. Please try again.";

        var flattenedException = exception.Flatten();
        var innerException = flattenedException.InnerExceptions.Count > 0 ? flattenedException.InnerExceptions[0] : null;
        var errorMsg = innerException?.Message ?? flattenedException.Message;

        // Check for specific Firebase Auth error codes
        if (errorMsg.Contains("EMAIL_EXISTS") || errorMsg.Contains("email-already-in-use"))
            return "This email is already registered. Please log in or use a different email.";

        if (errorMsg.Contains("INVALID_EMAIL"))
            return "Please enter a valid email address.";

        if (errorMsg.Contains("WEAK_PASSWORD"))
            return "Password is too weak. Please use a stronger password (at least 6 characters).";

        if (errorMsg.Contains("badly formatted"))
            return "Please enter a valid email address.";

        if (errorMsg.Contains("NETWORK") || errorMsg.Contains("network"))
            return "Network error. Please check your internet connection.";

        if (errorMsg.Contains("INVALID_PASSWORD"))
            return "Password must be at least 6 characters long.";

        // Default fallback
        return "Signup failed. Please check your email and password.";
    }
}