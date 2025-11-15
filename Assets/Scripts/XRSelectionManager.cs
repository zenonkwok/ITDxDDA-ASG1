using UnityEngine;

public class XRSelectionManager : MonoBehaviour
{
    public static XRSelectionManager Instance;

    [HideInInspector]
    public XRHighlightOnSelect currentlySelected;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SelectObject(XRHighlightOnSelect newSelection)
    {
        // Unhighlight previous selection
        if (currentlySelected != null && currentlySelected != newSelection)
        {
            currentlySelected.Unhighlight();
        }

        Debug.Log("Selected: " + newSelection.gameObject.name);

        // Highlight new selection
        currentlySelected = newSelection;
        newSelection.Highlight();
    }

    public void DeselectCurrent()
    {
        if (currentlySelected != null)
        {
            currentlySelected.Unhighlight();
            currentlySelected = null;
        }
    }
}
