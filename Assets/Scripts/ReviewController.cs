using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using TMPro;
using Firebase.Auth;
/*
public class ReviewController : MonoBehaviour
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
    

    public void DeletePlayer()
    {
        // Delete the player that matches the name currently in the input field
        if (Name == null)
        {
            Debug.LogError("Name input field is not assigned on ReviewController");
            StatusTextField?.SetText("Name input not assigned");
            return;
        }

        string nameStr = Name.text;
        if (string.IsNullOrEmpty(nameStr))
        {
            StatusTextField.text = "Enter a player name to delete";
            return;
        }

        FindPlayerByName(nameStr, (player) =>
        {
            var db = FirebaseDatabase.DefaultInstance.RootReference;
            db.Child("players").Child(player.playerId).RemoveValueAsync().ContinueWithOnMainThread(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    StatusTextField.text = "Failed to delete player";
                    return;
                }

                StatusTextField.text = $"Deleted player '{player.name}'";
                if (myPlayer != null && myPlayer.playerId == player.playerId) myPlayer = null;
            });
        }, () =>
        {
            StatusTextField.text = "Player not found";
        });
    }

    public void UpdatePlayerName()
    {
        if (myPlayer == null)
        {
            StatusTextField?.SetText("No player loaded to update");
            Debug.LogWarning("UpdatePlayerName called but myPlayer is null");
            return;
        }

        if (Name == null)
        {
            Debug.LogError("Name input field is not assigned on ReviewController");
            StatusTextField?.SetText("Name input not assigned");
            return;
        }

        var db = FirebaseDatabase.DefaultInstance.RootReference;

        // Use the input field's text value when writing to the database
        db.Child("players").Child(myPlayer.playerId).Child("name").SetValueAsync(Name.text);
    }

    public void UpdatePlayerMultipleValues()
    {
        if (myPlayer == null)
        {
            StatusTextField?.SetText("No player loaded to update");
            Debug.LogWarning("UpdatePlayerMultipleValues called but myPlayer is null");
            return;
        }

        if (Name == null)
        {
            Debug.LogError("Name input field is not assigned on ReviewController");
            StatusTextField?.SetText("Name input not assigned");
            return;
        }

        var db = FirebaseDatabase.DefaultInstance.RootReference;

        // Create a new empty dictionary
        Dictionary<string, object> data = new Dictionary<string, object>();

        // Setting the dictionary with a key = "name", and value taken from the Name variable
        data["name"] = Name.text;
        data["score"] = 1234;

        db.Child("players").Child(myPlayer.playerId).UpdateChildrenAsync(data);
    }

    // Create a new player with the name from the input field
    public void CreatePlayer()
    {
        if (Name == null)
        {
            Debug.LogError("Name input field is not assigned on ReviewController");
            StatusTextField?.SetText("Name input not assigned");
            return;
        }

        string nameStr = Name.text;
        if (string.IsNullOrEmpty(nameStr))
        {
            StatusTextField.text = "Enter a player name to create";
            return;
        }

        var db = FirebaseDatabase.DefaultInstance.RootReference;
        var newRef = db.Child("players").Push();
        string newId = newRef.Key;

        Player p = new Player(newId, nameStr);
        p.score = 0;

        string json = JsonUtility.ToJson(p);
        newRef.SetRawJsonValueAsync(json).ContinueWithOnMainThread(t =>
        {
            if (t.IsFaulted || t.IsCanceled)
            {
                StatusTextField.text = "Failed to create player";
                return;
            }

            myPlayer = p;
            StatusTextField.text = $"Created player '{p.name}'";
        });
    }

    // Add a potion item to the player with the name in the input field
    public void AddPotion()
    {
        if (Name == null)
        {
            Debug.LogError("Name input field is not assigned on ReviewController");
            StatusTextField?.SetText("Name input not assigned");
            return;
        }
        UpdateItemForPlayer(Name.text, "potion", 1);
    }

    // Add an arrow item to the player with the name in the input field
    public void AddArrow()
    {
        if (Name == null)
        {
            Debug.LogError("Name input field is not assigned on ReviewController");
            StatusTextField?.SetText("Name input not assigned");
            return;
        }
        UpdateItemForPlayer(Name.text, "arrow", 1);
    }

    // Helper: find a player by name by scanning the players node
    private void FindPlayerByName(string nameToFind, System.Action<Player> onFound, System.Action onNotFound)
    {
        if (string.IsNullOrEmpty(nameToFind))
        {
            onNotFound?.Invoke();
            return;
        }

        var db = FirebaseDatabase.DefaultInstance.RootReference;
        db.Child("players").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                onNotFound?.Invoke();
                return;
            }

            DataSnapshot snapshot = task.Result;
            foreach (var child in snapshot.Children)
            {
                string json = child.GetRawJsonValue();
                Player p = JsonUtility.FromJson<Player>(json);
                if (p != null && p.name == nameToFind)
                {
                    p.playerId = child.Key;
                    onFound?.Invoke(p);
                    return;
                }
            }

            onNotFound?.Invoke();
        });
    }

    // Helper: add/update an item for a player found by name
    private void UpdateItemForPlayer(string playerName, string itemType, int amount)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            StatusTextField.text = "Enter a player name";
            return;
        }

        FindPlayerByName(playerName, (player) =>
        {
            if (player.items == null) player.items = new System.Collections.Generic.List<Item>();

            // Try to find existing item of same type
            Item found = null;
            foreach (var it in player.items)
            {
                if (it.type == itemType)
                {
                    found = it;
                    break;
                }
            }

            if (found != null)
            {
                found.quantity += amount; //item quantity
            }
            else
            {
                player.items.Add(new Item(itemType, amount));
            }

            // Save updated player
            var db = FirebaseDatabase.DefaultInstance.RootReference;
            string json = JsonUtility.ToJson(player);
            db.Child("players").Child(player.playerId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    StatusTextField.text = "Failed to update player items";
                    return;
                }

                myPlayer = player;
                StatusTextField.text = $"Added {amount} {itemType}(s) to '{player.name}'";
            });
        }, () =>
        {
            StatusTextField.text = "Player not found";
        });
    }

    private void NewMessageAdded(object sender, ChildChangedEventArgs e)
    {
        var childValue = (string)e.Snapshot.Value(false);
        Debug.Log($"New message added: {childValue}");
    }

    private void UnreadMessagesChanged(object sender, ValueChangedEventArgs e)
    {
        var count = Convert.ToInt32(e.Snapshot.Value(false));
        newMessagesText.text = $"New messages: {count}";
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var db = FirebaseDatabase.DefaultInstance.RootReference;

        db.Child("messages").Child("unread").ValueChanged += UnreadMessagesChanged;

        db.Child("messages").Child("queue").ChildAdded += NewMessageAdded;

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
        
    }
}
*/