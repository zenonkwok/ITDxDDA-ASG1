using UnityEngine;
using TMPro;

public class FoodStatScript : MonoBehaviour
{
	// Set in Inspector
	public string locationId;

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
			Debug.Log("FoodStatScript: locationId not set.");
			ApplyDefaults();
			return;
		}

		if (DatabaseScript.Instance == null)
		{
			Debug.Log("FoodStatScript: DatabaseScript.Instance is null.");
			ApplyDefaults();
			return;
		}

		DatabaseScript.Instance.GetFoodStats(locationId, stats =>
		{
			Debug.Log("FoodStatScript: Fetched stats for locationId " + locationId);
			// This callback runs on the main thread (Firebase ContinueWithOnMainThread used)
			if (allergiesText != null) allergiesText.text = string.IsNullOrEmpty(stats.allergies) ? "—" : stats.allergies;
			if (tasteText != null) tasteText.text = string.IsNullOrEmpty(stats.taste) ? "—" : stats.taste;
			if (descriptionText != null) descriptionText.text = string.IsNullOrEmpty(stats.description) ? "—" : stats.description;

			float overallRating = 0f;
			if (stats != null)
			{
				overallRating = stats.GetOverallRatingFloat();
			}

			if (overallRating > 4.0f)
			{
				overallRatingText.text = "Highly Rated!";
			}

			if (overallRating > 2.0f && overallRating <= 4.0f)
			{
				overallRatingText.text = "Moderately Rated";
			}
			else if (overallRating > 0.0f && overallRating <= 2.0f)
			{
				overallRatingText.text = "Low Rated";
			}
			else if (overallRating > 0f)
			{
				overallRatingText.text = stats.OverallRatingAsString(1);
			}
			else
			{
				overallRatingText.text = "No rating";
			}
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
