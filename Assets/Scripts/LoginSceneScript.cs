using System.Threading.Tasks;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
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

	[Header("Buttons")]
	[SerializeField] private Button loginButton;

	[Header("UI Status Output")]
	[SerializeField] private TMP_Text statusText;

	[Header("Navigation")]
	[SerializeField] private string successSceneName = "";
	[SerializeField] private bool shouldLoadSceneOnSuccess = true;

	private void OnEnable()
	{
		Debug.Log("[LoginSceneScript.OnEnable] Login scene enabled");
		UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		Debug.Log("[LoginSceneScript.OnDisable] Login scene disabled");
		UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
	{
		Debug.LogWarning($"[LoginSceneScript] SCENE LOADED: {scene.name} (mode: {mode})");
		if (scene.name != "Login" && scene.name != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
		{
			Debug.LogWarning($"[LoginSceneScript] WARNING: Unexpected scene load to {scene.name}");
		}
	}

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
		try
		{
			string email = tmpEmailField != null ? tmpEmailField.text : "";
			string password = tmpPasswordField != null ? tmpPasswordField.text : "";

			// Trim whitespace from inputs
			email = email.Trim();
			password = password.Trim();

			Debug.Log($"[LoginSceneScript.OnLoginButtonClicked] Email input: '{email}', Password length: {password.Length}");

			if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
			{
				if (statusText != null) statusText.text = "Please enter an email and password.";
				Debug.LogWarning("Email or password field is empty.");
				return;
			}

			// Basic email format validation to provide immediate feedback
			if (!IsValidEmail(email))
			{
				Debug.LogWarning($"[LoginSceneScript.OnLoginButtonClicked] Email validation failed for: '{email}'");
				if (statusText != null) statusText.text = "Please enter a valid email address.";
				return;
			}

			Debug.Log($"[LoginSceneScript.OnLoginButtonClicked] Email validation passed. Proceeding with sign-in.");
			// Kick off the async sign-in (fire-and-forget from the UI event).
			_ = SignInWithEmailAsync(email, password);
		}
		catch (System.Exception ex)
		{
			Debug.LogError($"[LoginSceneScript.OnLoginButtonClicked] Unexpected error: {ex.Message}\n{ex.StackTrace}");
			if (statusText != null) statusText.text = "An error occurred. Please try again.";
		}
	}

	// Internal async entry (keeps public SignInWithEmail sync-friendly).
	private async Task SignInWithEmailAsync(string email, string password)
	{
		try
		{
			Debug.Log("[LoginSceneScript.SignInWithEmailAsync] Starting async sign-in...");
			await InitializeFirebaseAndSignIn(email, password).ConfigureAwait(false);
			Debug.Log("[LoginSceneScript.SignInWithEmailAsync] Async sign-in completed.");
		}
		catch (System.Exception ex)
		{
			Debug.LogError($"[LoginSceneScript.SignInWithEmailAsync] Exception caught: {ex.Message}\n{ex.StackTrace}");
			if (statusText != null) statusText.text = "Sign-in failed. Please try again.";
		}
	}

	private async Task InitializeFirebaseAndSignIn(string email, string password)
	{
		try
		{
			Debug.Log("Checking Firebase dependencies...");
			var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync().ConfigureAwait(false);
			if (dependencyStatus == DependencyStatus.Available)
			{
				FirebaseApp app = FirebaseApp.DefaultInstance;
				Debug.Log("[LoginSceneScript.InitializeFirebaseAndSignIn] Firebase dependencies available, calling SignInUser...");
				// Start sign-in; SignInUser uses ContinueWithOnMainThread to run completion logic on Unity's main thread.
				SignInUser(email, password);
			}
			else
			{
				Debug.LogError($"[LoginSceneScript.InitializeFirebaseAndSignIn] Could not resolve all Firebase dependencies: {dependencyStatus}");
				if (statusText != null) statusText.text = "Firebase initialization failed. Please try again.";
			}
		}
		catch (System.Exception ex)
		{
			Debug.LogError($"[LoginSceneScript.InitializeFirebaseAndSignIn] Exception: {ex.Message}\n{ex.StackTrace}");
			if (statusText != null) statusText.text = "An error occurred. Please try again.";
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
				Debug.Log("[LoginSceneScript.SignInUser] ContinueWithOnMainThread callback invoked");
				try
				{
					Debug.Log($"[LoginSceneScript.SignInUser] Task state - IsCanceled: {task.IsCanceled}, IsFaulted: {task.IsFaulted}, IsCompleted: {task.IsCompleted}");
					
					if (task.IsCanceled)
					{
						Debug.Log("[LoginSceneScript.SignInUser] Task was canceled");
						if (statusText != null) statusText.text = "Sign-in canceled.";
						Debug.LogWarning("Sign-in task was canceled.");
						RestoreUI();
						return;
					}

					if (task.IsFaulted)
					{
						Debug.Log("[LoginSceneScript.SignInUser] Task faulted, getting error message");
						string userFriendlyMsg = "Sign-in failed. Please check your email and password.";
						try
						{
							userFriendlyMsg = GetFriendlyErrorMessage(task.Exception);
						}
						catch (System.Exception exInner)
						{
							Debug.LogError($"[LoginSceneScript.SignInUser] Exception getting friendly error message: {exInner.Message}");
						}

						try
						{
							if (statusText != null) statusText.text = userFriendlyMsg;
						}
						catch (System.Exception exInner)
						{
							Debug.LogError($"[LoginSceneScript.SignInUser] Exception setting status text: {exInner.Message}");
						}

						Debug.LogWarning($"Sign-in failed: {userFriendlyMsg}");
						RestoreUI();
						Debug.Log("[LoginSceneScript.SignInUser] Returning from callback after error");
						return;
					}

					// Success - only load scene on successful authentication
					Debug.Log("[LoginSceneScript.SignInUser] Task succeeded, getting user credential");
					var userCredential = task.Result;
					if (userCredential != null && userCredential.User != null)
					{
						if (statusText != null) statusText.text = "Signed in: " + (userCredential.User?.Email ?? "<unknown>");
						Debug.LogFormat("Firebase user signed in successfully: {0} ({1})",
							userCredential.User.Email, userCredential.User.UserId);
						Debug.Log($"Signed in: {userCredential.User.Email}");

						// Load the success scene on the main thread (only if configured)
						if (shouldLoadSceneOnSuccess && !string.IsNullOrEmpty(successSceneName))
						{
							try
							{
								Debug.Log($"[LoginSceneScript] Loading scene: {successSceneName}");
								UnityEngine.SceneManagement.SceneManager.LoadScene(successSceneName);
							}
							catch (System.Exception ex)
							{
								Debug.LogError("Failed to load scene: " + ex.Message);
								if (statusText != null) statusText.text = "Failed to load next scene. Please try again.";
							}
						}
						else if (shouldLoadSceneOnSuccess && string.IsNullOrEmpty(successSceneName))
						{
							Debug.LogWarning("[LoginSceneScript] Success scene name is not configured. Please set 'successSceneName' in the inspector.");
						}
						else
						{
							Debug.Log("[LoginSceneScript] Scene loading disabled - staying on login screen");
						}
					}
					else
					{
						Debug.Log("[LoginSceneScript.SignInUser] User credential is null");
						if (statusText != null) statusText.text = "Sign-in failed: User credential is null.";
						Debug.LogError("User credential is null after successful sign-in task.");
					}
					Debug.Log("[LoginSceneScript.SignInUser] Callback completed");
				}
				catch (System.Exception ex)
				{
					Debug.LogError($"[SignInUser] Exception in ContinueWithOnMainThread: {ex.Message}\n{ex.StackTrace}");
					if (statusText != null) statusText.text = "An error occurred during sign-in. Please try again.";
					RestoreUI();
				}
			});
		}
		catch (System.Exception e)
		{
			if (statusText != null) statusText.text = "Sign-in failed: " + (e.Message ?? "Unknown error");
			Debug.LogError($"Error signing in user: {e.Message}");
		}
	}

	private void RestoreUI()
	{
		try
		{
			if (loginButton) loginButton.interactable = true;
			if (tmpEmailField) tmpEmailField.interactable = true;
			if (tmpPasswordField) tmpPasswordField.interactable = true;
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning($"[LoginSceneScript.RestoreUI] Exception restoring UI: {ex.Message}");
		}
	}

	// Basic email format validator. Keeps logic simple and avoids heavy dependencies.
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

			Debug.Log($"[LoginSceneScript.IsValidEmail] Email validation passed for: {email}");
			return true;
		}
		catch (System.Exception ex)
		{
			Debug.LogError($"[LoginSceneScript.IsValidEmail] Exception during validation: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	/// Converts Firebase Auth exceptions into user-friendly error messages
	/// </summary>
	private string GetFriendlyErrorMessage(System.AggregateException exception)
	{
		if (exception == null)
			return "Sign-in failed. Please try again.";

		var flattenedException = exception.Flatten();
		var innerException = flattenedException.InnerExceptions.Count > 0 ? flattenedException.InnerExceptions[0] : null;
		var errorMsg = innerException?.Message ?? flattenedException.Message;

		// Check for specific Firebase Auth error codes
		if (errorMsg.Contains("INVALID_LOGIN_CREDENTIALS") || errorMsg.Contains("user-not-found"))
			return "This email is not registered. Please sign up first.";

		if (errorMsg.Contains("INVALID_PASSWORD") || errorMsg.Contains("wrong-password"))
			return "Incorrect password. Please try again.";

		if (errorMsg.Contains("USER_DISABLED"))
			return "This account has been disabled.";

		if (errorMsg.Contains("badly formatted") || errorMsg.Contains("INVALID_EMAIL"))
			return "Please enter a valid email address.";

		if (errorMsg.Contains("TOO_MANY_ATTEMPTS"))
			return "Too many failed login attempts. Please try again later.";

		if (errorMsg.Contains("NETWORK") || errorMsg.Contains("network"))
			return "Network error. Please check your internet connection.";

		// Default fallback
		return "Sign-in failed. Please check your email and password.";
	}
}
