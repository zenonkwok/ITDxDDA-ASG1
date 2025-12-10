using UnityEngine;
using TMPro;

public class FoodStatScript : MonoBehaviour
{
	// Set in Inspector
	public string locationId;

	// show Allergies as per updated JSON
	public TextMeshProUGUI allergiesText;
	public TextMeshProUGUI tasteText;
	public TextMeshProUGUI descriptionText;
	public TextMeshProUGUI overallRatingText;

	// Optionally refresh from other scripts/UI
	public void Start()
	{
		Refresh();
	}

	public void Refresh()
	{
		if (string.IsNullOrEmpty(locationId))
		{
			Debug.LogWarning("FoodStatScript: locationId not set.");
			ApplyDefaults();
			return;
		}

		if (DatabaseScript.Instance == null)
		{
			Debug.LogWarning("FoodStatScript: DatabaseScript.Instance is null.");
			ApplyDefaults();
			return;
		}

		DatabaseScript.Instance.GetFoodStats(locationId, stats =>
		{
			// This callback runs on the main thread (Firebase ContinueWithOnMainThread used)
			if (allergiesText != null) allergiesText.text = string.IsNullOrEmpty(stats.allergies) ? "—" : stats.allergies;
			if (tasteText != null) tasteText.text = string.IsNullOrEmpty(stats.taste) ? "—" : stats.taste;
			if (descriptionText != null) descriptionText.text = string.IsNullOrEmpty(stats.description) ? "—" : stats.description;
			if (overallRatingText != null) overallRatingText.text = stats.overallCustomerRating > 0f ? stats.OverallRatingAsString(1) : "No rating";
		});
	}

	void ApplyDefaults()
	{
		if (allergiesText != null) allergiesText.text = "—";
		if (tasteText != null) tasteText.text = "—";
		if (descriptionText != null) descriptionText.text = "—";
		if (overallRatingText != null) overallRatingText.text = "No rating";
	}
}
