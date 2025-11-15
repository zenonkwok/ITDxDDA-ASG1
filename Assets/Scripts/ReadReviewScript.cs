using System.Collections;
using UnityEngine;
using TMPro;

public class ReadReviewScript : MonoBehaviour
{
    [SerializeField]
    private TMP_Text reviewHeader;
    [SerializeField]
    private TMP_Text reviewName;
    [SerializeField]
    private TMP_Text reviewText;

    [Header("Placeholder Text")]
    [SerializeField]
    private string placeholderHeader = "Reviews";
    [SerializeField]
    private string placeholderName = "Anonymous";
    [SerializeField]
    [TextArea]
    private string placeholderContent = "No reviews available.";

    private void Start()
    {
        ApplyPlaceholders();
    }

    // Immediately apply placeholder values to the UI fields
    public void ApplyPlaceholders()
    {
        if (reviewHeader != null) reviewHeader.text = placeholderHeader;
        if (reviewName != null) reviewName.text = placeholderName;
        if (reviewText != null) reviewText.text = placeholderContent;
    }

    // Populate UI fields from a ReviewData instance
    public void Populate(ReviewData data)
    {
        if (data == null)
        {
            ApplyPlaceholders();
            return;
        }

        if (reviewHeader != null) reviewHeader.text = string.IsNullOrEmpty(data.header) ? placeholderHeader : data.header;
        if (reviewName != null) reviewName.text = string.IsNullOrEmpty(data.author) ? placeholderName : data.author;
        if (reviewText != null) reviewText.text = string.IsNullOrEmpty(data.content) ? placeholderContent : data.content;
    }

    // Public method to request a review by id. This starts a coroutine that fetches
    // data from a database. Currently uses a mock implementation; replace with real DB call.
    public void LoadReview(string reviewId)
    {
        StartCoroutine(FetchReviewFromDatabaseMock(reviewId));
    }

    // Mock coroutine that simulates fetching data from a remote database and then
    // calls Populate. Replace the contents of this method with your real async DB call.
    private IEnumerator FetchReviewFromDatabaseMock(string reviewId)
    {
        // simulate network latency
        yield return new WaitForSeconds(0.5f);

        // return example data — in production, deserialize your DB response into ReviewData
        var example = new ReviewData
        {
            id = reviewId,
            header = "FoodClub Review",
            author = "Test User",
            content = "This is placeholder review text loaded from the mock database. Replace this method with a real DB query to populate live data."
        };

        Populate(example);
    }

    // Simple data model for a review. Adapt fields to match your database schema.
    public class ReviewData
    {
        public string id;
        public string header;
        public string author;
        public string content;
    }
}
