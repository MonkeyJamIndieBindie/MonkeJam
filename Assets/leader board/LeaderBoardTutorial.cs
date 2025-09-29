using UnityEngine;
using TMPro;
using Dan.Main;

public class MainMenuLeaderboardDynamic : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Transform contentParent; // Vertical Layout Group olan parent
    [SerializeField] GameObject rowPrefab;    // Ýçinde TMP_Text olan prefab (RankNameScore)
    [SerializeField] TMP_Text statusText;

    [Header("Leaderboard")]
    [SerializeField] LeaderboardReference leaderboardRef = Leaderboards.leaderBoard;
    [SerializeField] int maxRows = 20;

    void OnEnable() => Refresh();

    public void Refresh()
    {
        if (statusText) statusText.text = "Loading...";
        foreach (Transform c in contentParent) Destroy(c.gameObject);

        leaderboardRef.GetEntries(entries =>
        {
            int count = Mathf.Min(maxRows, entries.Length);
            for (int i = 0; i < count; i++)
            {
                var row = Instantiate(rowPrefab, contentParent);
                var text = row.GetComponentInChildren<TMP_Text>();
                if (text) text.text = $"{entries[i].Rank}. {entries[i].Username} - {entries[i].Score}";
            }

            if (statusText) statusText.text = (count == 0) ? "No entries yet." : "";
        },
        error =>
        {
            if (statusText) statusText.text = "Failed to load.";
            Debug.LogWarning("Leaderboard load failed: " + error);
        });
    }
}
