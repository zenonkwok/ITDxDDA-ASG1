using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    // Set these in the Inspector
    public string mainMenuScene = "MainMenu";
    public string reviewScene = "SampleScene";
    public string foodieScene = "FoodieScene"; // new: set the Foodie scene name in the Inspector

    public void SwitchtoMainMenu()
    {
        if (string.IsNullOrEmpty(mainMenuScene))
        {
            Debug.LogWarning("SceneManager: mainMenuScene is not set in Inspector.");
            return;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
    }

    public void SwitchtoReviewMode()
    {
        if (string.IsNullOrEmpty(reviewScene))
        {
            Debug.LogWarning("SceneManager: reviewScene is not set in Inspector.");
            return;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(reviewScene);
    }

    public void SwitchtoFoodieScene() // new: call this from UI to load the Foodie scene
    {
        if (string.IsNullOrEmpty(foodieScene))
        {
            Debug.LogWarning("SceneManager: foodieScene is not set in Inspector.");
            return;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(foodieScene);
    }
}
