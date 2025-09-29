using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using Dan.Main; // Leaderboard Creator

public class GameOverUI : MonoBehaviour
{
    [Header("Root & Dim")]
    [SerializeField] CanvasGroup root;   // t�m panel (CanvasGroup)
    [SerializeField] CanvasGroup dimBg;  // tam ekran siyah (Image + CanvasGroup)

    [Header("Score")]
    [SerializeField] TextMeshProUGUI bigScore;   // b�y�k skor
    [SerializeField] float scoreCountDuration = 1.0f;
    [SerializeField] Ease scoreCountEase = Ease.OutCubic;

    [Header("Name & Controls")]
    [SerializeField] TextMeshProUGUI nameLabel;  // "Your Name"
    [SerializeField] TMP_InputField nameInput;   // isim input
    [SerializeField] Button okButton;            // OK

    [Header("Pop Settings")]
    [SerializeField] float popDuration = 0.28f; // tek ��e pop s�resi
    [SerializeField] float popStagger = 0.14f; // ��eler aras� gecikme
    [SerializeField] float popStartScale = 0.72f; // ba�lang�� scale

    [Header("Blackout/Show Timings")]
    [SerializeField] float dimToBlack = 0.40f; // full siyaha ge�i�
    [SerializeField] float dimHold = 0.20f; // k�sa bekleme
    [SerializeField] float dimToUi = 0.20f; // 1.0 -> 0.6

    [Header("Next Scene")]
    [SerializeField] string mainMenuScene = "MainMenu";

    [Header("Leaderboard")]
    [SerializeField] LeaderboardReference leaderboardRef = Leaderboards.leaderBoard;
    [SerializeField] string defaultPlayerName = "Player";

    [Header("Optional: Hide on GameOver")]
    [SerializeField] Graphic hideOnGameOver;      // herhangi bir UI Graphic (Image/TMP/RawImage vs.)
    [SerializeField] float hidePopUp = 0.08f;     // �nce minik b�y�t
    [SerializeField] float hidePopDown = 0.22f;   // sonra k���l�p kaybolsun
    [SerializeField] float hideOvershoot = 1.05f; // minik b�y�me oran�

    int finalScore;
    bool uploading;
    Sequence seq;      // UI pop s�ras�
    Tween scoreTween; // 0final saya� tween

    void Awake()
    {
        if (!root) root = GetOrAdd<CanvasGroup>(gameObject);
        if (!dimBg) Debug.LogWarning("[GameOverUI] dimBg atanmad� (CanvasGroup).");
        if (!bigScore) Debug.LogWarning("[GameOverUI] bigScore atanmad�.");
        if (!nameInput) Debug.LogWarning("[GameOverUI] nameInput atanmad�.");
        if (!okButton) Debug.LogWarning("[GameOverUI] okButton atanmad�.");

        // Ba�lang�� halleri (aktif kal�rlar!)
        root.alpha = 0f;
        root.interactable = false;
        root.blocksRaycasts = false;

        if (dimBg) dimBg.alpha = 0f;

        PrepHiddenScore();
        PrepHiddenLabel();
        PrepHiddenInput();
        PrepHiddenOk();

        if (okButton) okButton.onClick.AddListener(OnOk);
    }

    T GetOrAdd<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        if (!c) c = go.AddComponent<T>();
        return c;
    }

    // -----------------------
    // Haz�rl�k (ba�lang��)
    // -----------------------
    void PrepHiddenScore()
    {
        if (!bigScore) return;
        var rt = bigScore.rectTransform;
        rt.localScale = Vector3.one * popStartScale;

        var c = bigScore.color;
        c.a = 0f; // g�r�nmez
        bigScore.color = c;
        bigScore.text = "0";
    }

    void PrepHiddenLabel()
    {
        if (!nameLabel) return;
        var rt = nameLabel.rectTransform;
        rt.localScale = Vector3.one * popStartScale;
        nameLabel.alpha = 0f; // TMP alpha
    }

    void PrepHiddenInput()
    {
        if (!nameInput) return;
        var cg = nameInput.GetComponent<CanvasGroup>() ?? nameInput.gameObject.AddComponent<CanvasGroup>();
        var rt = nameInput.transform as RectTransform;
        rt.localScale = Vector3.one * popStartScale;
        cg.alpha = 0f;
        nameInput.text = "";
    }

    void PrepHiddenOk()
    {
        if (!okButton) return;
        var cg = okButton.GetComponent<CanvasGroup>() ?? okButton.gameObject.AddComponent<CanvasGroup>();
        var rt = okButton.transform as RectTransform;
        rt.localScale = Vector3.one * popStartScale;
        cg.alpha = 0f;
    }

    // -----------------------
    // DI�ARIDAN �A�IR
    // -----------------------
    // Blackout -> Pause -> (hideOnGameOver pop-out) -> Score pop & count -> Name -> Input -> OK
    public void BlackoutThenShow(int score, float blackDurationOverride = -1f)
    {
        finalScore = Mathf.Max(0, score);
        Debug.Log($"[GameOverUI] incoming score = {finalScore}");

        gameObject.SetActive(true);
        root.interactable = true;
        root.blocksRaycasts = true;

        // T�m ��eleri gizli ba�lang�ca �ek (yeniden �a�r� g�venli�i)
        seq?.Kill(); scoreTween?.Kill();
        PrepHiddenScore();
        PrepHiddenLabel();
        PrepHiddenInput();
        PrepHiddenOk();

        float toBlack = (blackDurationOverride > 0f) ? blackDurationOverride : dimToBlack;

        // 1) FULL BLACK (unscaled)
        if (dimBg)
        {
            dimBg.DOFade(1f, toBlack).SetUpdate(true).OnComplete(() =>
            {
                // 2) k�sa tutma
                DOVirtual.DelayedCall(dimHold, () =>
                {
                    // 3) OYUNU DURDUR
                    Time.timeScale = 0f;

                    // 4) Dim�i 1
                    dimBg.DOFade(0.6f, dimToUi).SetUpdate(true);
                    root.DOFade(1f, dimToUi).SetUpdate(true);

                    // 4.5) (Opsiyonel) Ekrandaki bir g�rseli pop-out ile yok et
                    HideGraphicPopOut();

                    // 5) Popup s�ras�
                    RunPopupSequence();
                }, true);
            });
        }
        else
        {
            // dim yoksa dir
            Time.timeScale = 0f;
            root.DOFade(1f, 0.2f).SetUpdate(true);
            HideGraphicPopOut();
            RunPopupSequence();
        }
    }

    // Yar�-siy
    public void Show(int score)
    {
        finalScore = Mathf.Max(0, score);
        Debug.Log($"[GameOverUI] Show() score = {finalScore}");

        gameObject.SetActive(true);
        root.interactable = true;
        root.blocksRaycasts = true;

        seq?.Kill(); scoreTween?.Kill();
        PrepHiddenScore();
        PrepHiddenLabel();
        PrepHiddenInput();
        PrepHiddenOk();

        if (dimBg) dimBg.DOFade(0.6f, 0.35f).SetUpdate(true);
        root.DOFade(1f, 0.25f).SetUpdate(true);

        HideGraphicPopOut();
        RunPopupSequence();
    }

    // -----------------------
    // S�ral
    // -----------------------
    void RunPopupSequence()
    {
        seq = DOTween.Sequence().SetUpdate(true); // unscaled
        float cursor = 0f;

        // -
        if (bigScore)
        {
            // g�r�n�r pop
            seq.Insert(cursor, bigScore.DOFade(1f, popDuration));
            seq.Insert(cursor, bigScore.rectTransform.DOScale(1f, popDuration).SetEase(Ease.OutBack));

            // 0final say�m�
            float v = 0f;
            scoreTween = DOTween.To(() => v, x =>
            {
                v = x;
                bigScore.text = Mathf.RoundToInt(v).ToString();
            }, finalScore, scoreCountDuration)
            .SetEase(scoreCountEase)
            .SetUpdate(true)
            .OnComplete(StartScoreIdlePulse);

            cursor += popDuration + popStagger;
        }

        // --- NAME LABEL ---
        if (nameLabel)
        {
            seq.Insert(cursor, nameLabel.DOFade(1f, popDuration)); // TMP alpha
            seq.Insert(cursor, nameLabel.rectTransform.DOScale(1f, popDuration).SetEase(Ease.OutBack));
            cursor += popDuration + popStagger;
        }

        if (nameInput)
        {
            var cg = nameInput.GetComponent<CanvasGroup>() ?? nameInput.gameObject.AddComponent<CanvasGroup>();
            var rt = (RectTransform)nameInput.transform;

            // ba�lang��ta kapal�
            cg.alpha = 0f;
            cg.interactable = false;           // << g�venlik
            cg.blocksRaycasts = false;         // << g�venlik

            seq.Insert(cursor, cg.DOFade(1f, popDuration));
            seq.Insert(cursor, rt.DOScale(1f, popDuration).SetEase(Ease.OutBack));

            // g�r�n�r olur olmaz etkile�imi a�
            seq.InsertCallback(cursor + popDuration, () =>
            {
                cg.interactable = true;        // << A�
                cg.blocksRaycasts = true;      // << A�
                nameInput.ActivateInputField();
            });

            cursor += popDuration + popStagger;
        }

        // --- OK ---
        if (okButton)
        {
            var cg = okButton.GetComponent<CanvasGroup>() ?? okButton.gameObject.AddComponent<CanvasGroup>();
            var rt = (RectTransform)okButton.transform;

            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            // g�venlik: butonun image'� raycast als�n
            var img = okButton.GetComponent<Image>();
            if (img) img.raycastTarget = true;

            // g�venlik: ebeveyn canvasgrouplar� da t�klama ge�irsin
            EnableParentsForRaycast(okButton.transform);

            // en �ste getir (ayn� canvas i�i)
            okButton.transform.SetAsLastSibling();

            // animasyon
            seq.Insert(cursor, cg.DOFade(1f, popDuration));
            seq.Insert(cursor, rt.DOScale(1f, popDuration).SetEase(Ease.OutBack));

            // g�r�n�r olur olmaz etkile�imi A�
            seq.InsertCallback(cursor + popDuration, () =>
            {
                cg.interactable = true;
                cg.blocksRaycasts = true;
                if (img) img.raycastTarget = true;
            });
        }

    }

    // Skor yaz�s� i�in yumu�ak loop (unscaled)
    void StartScoreIdlePulse()
    {
        if (bigScore == null) return;
        var rt = bigScore.rectTransform;
        rt.DOKill();

        // �ok hafif
        DOTween.Sequence().SetUpdate(true).SetLoops(-1, LoopType.Yoyo)
            .Append(rt.DOScale(1.05f, 0.55f).SetEase(Ease.InOutSine))
            .Join(rt.DOLocalRotate(new Vector3(0, 0, 2.2f), 0.55f).SetEase(Ease.InOutSine))
            .Append(rt.DOScale(1.00f, 0.55f).SetEase(Ease.InOutSine))
            .Join(rt.DOLocalRotate(Vector3.zero, 0.55f).SetEase(Ease.InOutSine));
    }

    // -----------------------
    // OK  GameManager�a delege
    // -----------------------
    void OnOk()
    {
        string playerName = nameInput && !string.IsNullOrWhiteSpace(nameInput.text)
            ? nameInput.text.Trim()
            : defaultPlayerName;

        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.SubmitScoreAndReturnToMenu(playerName);
        }
        else
        {
            Time.timeScale = 1f;
            MusicManager.Instance?.PlayMainMenuMusic(/* instantIn: */ false);
            Debug.LogWarning("[GameOverUI] GameManager bulunamad�, fallback ile ana men�ye d�n�l�yor.");
            if (!string.IsNullOrEmpty(mainMenuScene))
                SceneManager.LoadScene(mainMenuScene);
        }
    }

    // -----------------------
    // Opsiyonel g�rseli pop-out ile yok et
    // -----------------------
    void HideGraphicPopOut()
    {
        if (hideOnGameOver == null) return;

        var rt = hideOnGameOver.rectTransform;
        var cg = hideOnGameOver.GetComponent<CanvasGroup>();
        if (!cg) cg = hideOnGameOver.gameObject.AddComponent<CanvasGroup>();

        // animasyona ba�lamadan �nce alpha 1 yap
        cg.alpha = 1f;
        rt.localScale = Vector3.one;

        // minik b�y�  k���l fade inactive
        var s = DOTween.Sequence().SetUpdate(true);
        s.Append(rt.DOScale(hideOvershoot, hidePopUp).SetEase(Ease.OutBack));
        s.Append(rt.DOScale(0.0f, hidePopDown).SetEase(Ease.InBack));
        s.Join(cg.DOFade(0f, hidePopDown));
        s.OnComplete(() =>
        {
            if (hideOnGameOver) hideOnGameOver.gameObject.SetActive(false);
        });
    }

    void EnableParentsForRaycast(Transform t)
    {
        // k�ke kadar t�m CanvasGroup'lar� a�
        var cur = t;
        while (cur != null)
        {
            var pg = cur.GetComponent<CanvasGroup>();
            if (pg)
            {
                pg.interactable = true;
                pg.blocksRaycasts = true;
            }
            cur = cur.parent;
        }

        // dim katman� t�klama yutmas�n
        if (dimBg)
        {
            dimBg.blocksRaycasts = false;
            // Ek: e�er dim Image var ve �stteyse:
            var dimGraphic = dimBg.GetComponent<Graphic>();
            if (dimGraphic) dimGraphic.raycastTarget = false;
        }
    }
}
