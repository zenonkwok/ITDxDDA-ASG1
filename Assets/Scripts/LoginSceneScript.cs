using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;

public class LoginSceneScript : MonoBehaviour
{
	// Input field references. Assign TextMeshPro fields in the Inspector.
	[Header("TextMeshPro Input Fields")]
	[SerializeField] private TMP_InputField tmpEmailField;
	[SerializeField] private TMP_InputField tmpPasswordField;

	// Public programmatic wrapper you can call with explicit credentials.
	public void SignInWithEmail(string email, string password)
	{
		InitializeFirebaseAndSignIn(email, password);
	}

	// Called via a Unity UI Button (no parameters). Reads assigned input fields.
	public void OnLoginButtonClicked()
	{
		string email = tmpEmailField != null ? tmpEmailField.text : "";
		string password = tmpPasswordField != null ? tmpPasswordField.text : "";

		if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
		{
			Debug.LogError("No TMP_InputField assigned or fields are empty. Assign `tmpEmailField` and `tmpPasswordField` in the inspector and provide values.");
			return;
		}

		// Kick off the async sign-in (fire-and-forget from the UI event).
		_ = SignInWithEmailAsync(email, password);
	}

	// Internal async entry (keeps public SignInWithEmail sync-friendly).
	private async Task SignInWithEmailAsync(string email, string password)
	{
		await InitializeFirebaseAndSignIn(email, password).ConfigureAwait(false);
	}

	private async Task InitializeFirebaseAndSignIn(string email, string password)
	{
		Debug.Log("Checking Firebase dependencies...");
		var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync().ConfigureAwait(false);
		if (dependencyStatus == DependencyStatus.Available)
		{
			FirebaseApp app = FirebaseApp.DefaultInstance;
			await SignInUser(email, password).ConfigureAwait(false);
		}
		else
		{
			Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
		}
	}

	private async Task SignInUser(string email, string password)
	{
		var auth = FirebaseAuth.DefaultInstance;
		try
		{
			Debug.Log("Signing in...");
			var userCredential = await auth.SignInWithEmailAndPasswordAsync(email, password).ConfigureAwait(false);
			Debug.LogFormat("Firebase user signed in successfully: {0} ({1})",
				userCredential.User.Email, userCredential.User.UserId);
			Debug.Log($"Signed in: {userCredential.User.Email}");
			// Optionally: proceed to next scene or notify other systems
		}
		catch (System.Exception e)
		{
			Debug.LogError($"Error signing in user: {e.Message}");
		}
	}
}
