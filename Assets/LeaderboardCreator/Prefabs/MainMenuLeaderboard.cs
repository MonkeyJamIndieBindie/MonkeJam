using UnityEngine;
using TMPro;
using Dan.Main;          // Leaderboard Creator
using System.Collections.Generic;

public class MainMenuLeaderboard : MonoBehaviour
{
    [Header("Leaderboard Source")]
    [SerializeField] LeaderboardReference leaderboardRef = Leaderboards.leaderBoard; // kendi leaderboard’unu seç
    [SerializeField] int maxEntriesToShow = 20;

    [Header("UI Hookups")]
    [SerializeField] Transform contentParent;    // Vertical Layout / Content objesi
    [SerializeField] GameObject entryPrefab;     // EntryUI ekli prefab

    [Header("Optional UI")]
    [SerializeField] TMP_Text loadingText;       // "Loading..." göstermek için (opsiyonel)
    [SerializeField] TMP_Text emptyText;         // "No scores yet" gibi (opsiyonel)

    void Start()
    {
        // Ana menü müziði
        MusicManager.Instance?.PlayMainMenuMusic(false);

        Refresh();
    }

    public void Refresh()
    {
        if (loadingText) loadingText.gameObject.SetActive(true);
        if (emptyText) emptyText.gameObject.SetActive(false);

        ClearContent();

        leaderboardRef.GetEntries(entries =>
        {
            if (loadingText) loadingText.gameObject.SetActive(false);

            if (entries == null || entries.Length == 0)
            {
                if (emptyText)
                {
                    emptyText.text = "No scores yet.";
                    emptyText.gameObject.SetActive(true);
                }
                return;
            }

            int count = Mathf.Min(maxEntriesToShow, entries.Length);
            for (int i = 0; i < count; i++)
            {
                var e = entries[i]; // e.Rank, e.Username, e.Score
                var go = Instantiate(entryPrefab, contentParent);
                var ui = go.GetComponent<EntryUI>();
                if (ui != null) ui.Set(e.Rank, e.Username, e.Score);
            }
        });
    }

    void ClearContent()
    {
        if (!contentParent) return;
        var toDestroy = new List<Transform>();
        foreach (Transform child in contentParent)
            toDestroy.Add(child);
        foreach (var t in toDestroy)
            Destroy(t.gameObject);
    }
}
