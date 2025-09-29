using UnityEngine;
using TMPro;

public class EntryUI : MonoBehaviour
{
    [SerializeField] TMP_Text rankText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text scoreText;

    // Inspector'da alanlarý atamadýysan, isimden bulmayý dener
    void Reset()
    {
        if (!rankText) rankText = transform.Find("Rank")?.GetComponent<TMP_Text>();
        if (!nameText) nameText = transform.Find("Name")?.GetComponent<TMP_Text>();
        if (!scoreText) scoreText = transform.Find("Score")?.GetComponent<TMP_Text>();
    }

    public void Set(int rank, string username, int score)
    {
        if (rankText) rankText.text = rank.ToString();
        if (nameText) nameText.text = string.IsNullOrWhiteSpace(username) ? "Player" : username;
        if (scoreText) scoreText.text = score.ToString();
    }
}
