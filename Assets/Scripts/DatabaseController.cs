using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using TMPro;
using Firebase.Auth;

public class DatabaseController : MonoBehaviour
{
    private Player myPlayer;

    public TMP_Text StatusTextField;
    public TMP_InputField Email;
    public TMP_InputField Password;
    public TMP_InputField Name;

    public void Signup()
    {
        var createUserTask = FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(Email.text, Password.text);

        createUserTask.ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                StatusTextField.text = ("Failed to Create User");
                Debug.Log("Can't create user!");
                return;
            }

            if (task.IsCompleted)
            {
                StatusTextField.text = ("User created successfully");
                Debug.Log($"User created, user ID is: {task.Result.User.UserId}");

                // SAVE THE USER'S PROFILE
            }

        });
    }

    public void SignIn()
    {
        var signInTask = FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(Email.text, Password.text);

        signInTask.ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                StatusTextField.text = ("Failed to Sign In");
                Debug.Log("Can't sign in!");
                return;
            }

            if (task.IsCompleted)
            {
                StatusTextField.text = ("User signed in successfully");
                Debug.Log($"User signed in, user ID is: {task.Result.User.UserId}");

                // LOAD THE USER'S PROFILE
            }

        });
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
                Debug.Log("Error loading player!!!");
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
            // Put the loaded player name into the input field's text
            Name.text = myPlayer.name;
            Debug.Log($"Player name is: {myPlayer.name}");
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
