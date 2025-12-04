using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using TMPro;
using Firebase.Auth;
using UnityEngine.SceneManagement;

public class DatabaseController : MonoBehaviour
{
    private Player myPlayer;

    public TMP_Text StatusTextField;
    public TMP_InputField Email;
    public TMP_InputField Password;
    public TMP_InputField Name;

    public void Signup()
    {
        // Validate UI fields are assigned
        if (Email == null || Password == null)
        {
            if (StatusTextField != null) StatusTextField.text = "Signup UI fields not configured.";
            Debug.LogWarning("Email or Password input field is not assigned on DatabaseController.");
            return;
        }

        string emailText = Email.text?.Trim();
        string passwordText = Password.text;

        if (string.IsNullOrEmpty(emailText) || string.IsNullOrEmpty(passwordText))
        {
            if (StatusTextField != null) StatusTextField.text = "Please enter an email and password.";
            return;
        }

        var auth = FirebaseAuth.DefaultInstance;
        if (auth == null)
        {
            if (StatusTextField != null) StatusTextField.text = "Firebase Auth not initialized.";
            Debug.LogError("FirebaseAuth.DefaultInstance is null in Signup().");
            return;
        }

        try
        {
            var createUserTask = auth.CreateUserWithEmailAndPasswordAsync(emailText, passwordText);

            createUserTask.ContinueWithOnMainThread(task =>
            {
                if (StatusTextField == null)
                {
                    Debug.LogWarning("StatusTextField not assigned; cannot show signup status to user.");
                }

                if (task.IsCanceled)
                {
                    if (StatusTextField != null) StatusTextField.text = "Signup canceled.";
                    Debug.LogWarning("Signup task was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    if (StatusTextField != null) StatusTextField.text = "Failed to create user.";
                    Debug.LogError("Signup failed: " + (task.Exception != null ? task.Exception.Flatten().Message : "Unknown error"));
                    return;
                }

                if (task.IsCompleted)
                {
                    if (StatusTextField != null) StatusTextField.text = "User created successfully.";
                    Debug.Log($"User created, user ID is: {task.Result.User.UserId}");

                    // SAVE THE USER'S PROFILE
                    try
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Failed to load MainMenu after signup: " + ex.Message);
                    }
                }
            });
        }
        catch (System.Exception ex)
        {
            if (StatusTextField != null) StatusTextField.text = "Failed to start signup.";
            Debug.LogException(ex);
        }
    }

    public void SignIn()
    {
        // Validate UI fields are assigned
        if (Email == null || Password == null)
        {
            if (StatusTextField != null) StatusTextField.text = "Sign in UI fields not configured.";
            Debug.LogWarning("Email or Password input field is not assigned on DatabaseController.");
            return;
        }

        string emailText = Email.text?.Trim();
        string passwordText = Password.text;

        if (string.IsNullOrEmpty(emailText) || string.IsNullOrEmpty(passwordText))
        {
            if (StatusTextField != null) StatusTextField.text = "Please enter an email and password.";
            return;
        }

        var auth = FirebaseAuth.DefaultInstance;
        if (auth == null)
        {
            if (StatusTextField != null) StatusTextField.text = "Firebase Auth not initialized.";
            Debug.LogError("FirebaseAuth.DefaultInstance is null in SignIn().");
            return;
        }

        try
        {
            var signInTask = auth.SignInWithEmailAndPasswordAsync(emailText, passwordText);

            signInTask.ContinueWithOnMainThread(task =>
            {
                if (StatusTextField == null)
                {
                    Debug.LogWarning("StatusTextField not assigned; cannot show sign-in status to user.");
                }

                if (task.IsCanceled)
                {
                    if (StatusTextField != null) StatusTextField.text = "Sign-in canceled.";
                    Debug.LogWarning("Sign-in task was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    if (StatusTextField != null) StatusTextField.text = "Failed to sign in.";
                    Debug.LogError("Sign-in failed: " + (task.Exception != null ? task.Exception.Flatten().Message : "Unknown error"));
                    return;
                }

                if (task.IsCompleted)
                {
                    if (StatusTextField != null) StatusTextField.text = "User signed in successfully.";
                    Debug.Log($"User signed in, user ID is: {task.Result.User.UserId}");

                    // LOAD THE USER'S PROFILE
                    try
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Failed to load MainMenu after sign-in: " + ex.Message);
                    }
                }

            });
        }
        catch (System.Exception ex)
        {
            if (StatusTextField != null) StatusTextField.text = "Failed to start sign-in.";
            Debug.LogException(ex);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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


        /*
         * OLD CODE BEFORE CRUD
         *
        Player justin = new Player("detach8", "Justin");
        justin.items.Add(new Item("sword", 2));

        Player steve = new Player("steviewonder", "Steve from Minecraft");
        steve.items.Add(new Item("pickaxe", 1));

        Player alex = new Player("alexinwonderland", "Alex also from Minecraft");
        alex.items.Add(new Item("armour", 1));

        // Convert to JSON
        string justinJson = JsonUtility.ToJson(justin, true);
        string steveJson = JsonUtility.ToJson(steve, true);

        // Print the JSON strings out
        Debug.Log(justinJson);
        Debug.Log(steveJson);

        // Save it in the DB
        db.Child("players").Child(justin.playerId).SetRawJsonValueAsync(justinJson);
        db.Child("players").Child(steve.playerId).SetRawJsonValueAsync(steveJson);

        // Using PUSH to add data
        var newChildReference = db.Child("players").Push();

        // Get the key, and set as Alex's playerId
        Debug.Log(newChildReference.Key);
        alex.playerId = newChildReference.Key;

        // Convert alex object to JSON
        string alexJson = JsonUtility.ToJson(alex); // No pretty print
        Debug.Log(alexJson);

        // Write to database
        newChildReference.SetRawJsonValueAsync(alexJson);
        */
    }
}
