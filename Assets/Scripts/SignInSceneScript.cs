using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;

public class SignInSceneScript : MonoBehaviour
{
    [Header("TextMeshPro Input Fields")]
    [SerializeField] private TMP_InputField tmpEmailField;
    [SerializeField] private TMP_InputField tmpPasswordField;

    [Header("Optional: status text")]
    [SerializeField] private TMP_Text statusText;

    // Called from UI Button OnClick()
    public void OnLoginButtonClicked()
    {
        string email = tmpEmailField != null ? tmpEmailField.text : "";
        string password = tmpPasswordField != null ? tmpPasswordField.text : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            LogStatus("Please enter email and password.", true);
            return;
        }

        _ = SignInWithEmailAsync(email, password);
    }

    // Programmatic call
    public void SignInWithEmail(string email, string password)
    {
        _ = SignInWithEmailAsync(email, password);
    }

    private async Task SignInWithEmailAsync(string email, string password)
    {
        LogStatus("Checking Firebase dependencies...");

        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync().ConfigureAwait(false);
        if (dependencyStatus != DependencyStatus.Available)
        {
            LogStatus($"Could not resolve all Firebase dependencies: {dependencyStatus}", true);
            return;
        }

        try
        {
            var auth = FirebaseAuth.DefaultInstance;
            var userCredential = await auth.SignInWithEmailAndPasswordAsync(email, password).ConfigureAwait(false);

            var user = userCredential?.User;
            if (user != null)
            {
                LogStatus($"Signed in: {user.Email} (UID: {user.UserId})");
            }
            else
            {
                LogStatus("Sign-in succeeded but user object is null.", true);
            }
        }
        catch (System.Exception e)
        {
            LogStatus($"Sign-in failed: {e.Message}", true);
        }
    }

    private void LogStatus(string message, bool isError = false)
    {
        Debug.Log(message);
        if (statusText != null)
        {
            // Update on main thread — use Unity's scheduling if needed (we're only setting text)
            statusText.text = message;
            statusText.color = isError ? Color.red : Color.white;
        }
    }
}
