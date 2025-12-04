using System.Threading.Tasks;
using System.Text.RegularExpressions;
using UnityEngine;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase;
using Firebase.Auth;

public class LoginSceneScript : MonoBehaviour
{
	// Input field references. Assign TextMeshPro fields in the Inspector.
	[Header("TextMeshPro Input Fields")]
	[SerializeField] private TMP_InputField tmpEmailField;
	[SerializeField] private TMP_InputField tmpPasswordField;

	[Header("UI Status Output")]
	[SerializeField] private TMP_Text statusText;

	[Header("Navigation")]
	[SerializeField] private string successSceneName = "MainMenu";

	// Public programmatic wrapper you can call with explicit credentials.
	public void SignInWithEmail(string email, string password)
	{
		// Validate email format before attempting sign-in
		if (!IsValidEmail(email))
		{
			if (statusText != null) statusText.text = "Please enter a valid email address.";
			Debug.LogError($"Invalid email format: {email}");
			return;
		}

		// Start the async sign-in flow (exceptions handled inside the async task)
		_ = InitializeFirebaseAndSignIn(email, password);
	}

	// Called via a Unity UI Button (no parameters). Reads assigned input fields.
	public void OnLoginButtonClicked()
	{
		string email = tmpEmailField != null ? tmpEmailField.text : "";
		string password = tmpPasswordField != null ? tmpPasswordField.text : "";

		if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
		{
			if (statusText != null) statusText.text = "Please enter an email and password.";
			Debug.LogError("No TMP_InputField assigned or fields are empty. Assign `tmpEmailField` and `tmpPasswordField` in the inspector and provide values.");
			return;
		}

		// Basic email format validation to provide immediate feedback
		if (!IsValidEmail(email))
		{
			if (statusText != null) statusText.text = "Please enter a valid email address.";
			Debug.LogError("Please enter a valid email address.");
			return;
		}

		// Kick off the async sign-in (fire-and-forget from the UI event).
		_ = SignInWithEmailAsync(email, password);
	}

	// Internal async entry (keeps public SignInWithEmail sync-friendly).
	private async Task SignInWithEmailAsync(string email, string password)
	{
		try
		{
			await InitializeFirebaseAndSignIn(email, password).ConfigureAwait(false);
		}
		catch (System.Exception ex)
		{
			if (statusText != null) statusText.text = "Sign-in failed. See console for details.";
			Debug.LogError($"Sign-in task failed: {ex.Message}");
		}
	}

	private async Task InitializeFirebaseAndSignIn(string email, string password)
	{
		Debug.Log("Checking Firebase dependencies...");
		var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync().ConfigureAwait(false);
		if (dependencyStatus == DependencyStatus.Available)
		{
			FirebaseApp app = FirebaseApp.DefaultInstance;
			// Start sign-in; SignInUser uses ContinueWithOnMainThread to run completion logic on Unity's main thread.
			SignInUser(email, password);
		}
		else
		{
			Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
		}
	}

	private void SignInUser(string email, string password)
	{
		var auth = FirebaseAuth.DefaultInstance;
		if (auth == null)
		{
			if (statusText != null) statusText.text = "Firebase Auth not available.";
			Debug.LogError("FirebaseAuth.DefaultInstance is null in SignInUser().");
			return;
		}

		try
		{
			Debug.Log("Signing in...");
			var signInTask = auth.SignInWithEmailAndPasswordAsync(email, password);
			signInTask.ContinueWithOnMainThread(task =>
			{
				if (task.IsCanceled)
				{
					if (statusText != null) statusText.text = "Sign-in canceled.";
					Debug.LogWarning("Sign-in task was canceled.");
					return;
				}

				if (task.IsFaulted)
				{
					var msg = task.Exception != null ? task.Exception.Flatten().Message : "Unknown error";
					if (msg.ToLower().Contains("badly formatted"))
					{
						if (statusText != null) statusText.text = "Please enter a valid email address.";
						Debug.LogError("Error signing in user: The email address is badly formatted.");
					}
					else
					{
						if (statusText != null) statusText.text = "Sign-in failed: " + msg;
						Debug.LogError("Sign-in failed: " + msg);
					}
					return;
				}

				// Success
				var userCredential = task.Result;
				if (statusText != null) statusText.text = "Signed in: " + (userCredential.User?.Email ?? "<unknown>");
				Debug.LogFormat("Firebase user signed in successfully: {0} ({1})",
					userCredential.User.Email, userCredential.User.UserId);
				Debug.Log($"Signed in: {userCredential.User.Email}");

				// Load the success scene on the main thread
					try
					{
						UnityEngine.SceneManagement.SceneManager.LoadScene(successSceneName);
					}
					catch (System.Exception ex)
					{
						Debug.LogError("Failed to load scene: " + ex.Message);
					}
			});
		}
		catch (System.Exception e)
		{
			if (statusText != null) statusText.text = "Sign-in failed: " + (e.Message ?? "Unknown error");
			Debug.LogError($"Error signing in user: {e.Message}");
		}
	}

	// Basic email format validator. Keeps logic simple and avoids heavy dependencies.
	private bool IsValidEmail(string email)
	{
		if (string.IsNullOrWhiteSpace(email)) return false;
		try
		{
			return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
		}
		catch
		{
			return false;
		}
	}
}
