using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public void SwitchtoMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void SwitchtoReviewMode()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
}
