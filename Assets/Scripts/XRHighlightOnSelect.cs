using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable))]
public class XRHighlightOnSelect : MonoBehaviour
{
    [Header("Materials")]
    public Material defaultMaterial;
    public Material highlightMaterial;

    private Renderer rend;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

        if (defaultMaterial == null)
            defaultMaterial = rend.material;

        // Subscribe to XR selection events
        interactable.selectEntered.AddListener(OnSelected);

        // **Do NOT subscribe to selectExited**
        // This prevents the object from losing highlight when released
    }

    void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(OnSelected);
    }

    public void Highlight()
    {
        rend.material = highlightMaterial;
    }

    public void Unhighlight()
    {
        rend.material = defaultMaterial;
    }

    // Called by XR Interaction Toolkit when the object is selected
    private void OnSelected(SelectEnterEventArgs args)
    {
        // Tell the selection manager that this object is selected
        XRSelectionManager.Instance.SelectObject(this);
    }
}
