using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using Dan.Main; // Leaderboard Creator

public class GameOverUI : MonoBehaviour
{
    [Header("Root & Dim")]
    [SerializeField] CanvasGroup root;   // tüm panel (CanvasGroup)
    [SerializeField] CanvasGroup dimBg;  // tam ekran siyah (Image + CanvasGroup)

    [Header("Score")]
    [SerializeField] TextMeshProUGUI bigScore;   // büyük skor
    [SerializeField] float scoreCountDuration = 1.0f;
    [SerializeField] Ease scoreCountEase = Ease.OutCubic;

    [Header("Name & Controls")]
    [SerializeField] TextMeshProUGUI nameLabel;  // "Your Name"
    [SerializeField] TMP_InputField nameInput;   // isim input
    [SerializeField] Button okButton;            // OK

    [Header("Pop Settings")]
    [SerializeField] float popDuration = 0.28f; // tek öðe pop süresi
    [SerializeField] float popStagger = 0.14f; // öðeler arasý gecikme
    [SerializeField] float popStartScale = 0.72f; // baþlangýç scale

    [Header("Blackout/Show Timings")]
    [SerializeField] float dimToBlack = 0.40f; // full siyaha geçiþ
    [SerializeField] float dimHold = 0.20f; // kýsa bekleme
    [SerializeField] float dimToUi = 0.20f; // 1.0 -> 0.6

    [Header("Next Scene")]
    [SerializeField] string mainMenuScene = "MainMenu";

    [Header("Leaderboard")]
    [SerializeField] LeaderboardReference leaderboardRef = Leaderboards.leaderBoard;
    [SerializeField] string defaultPlayerName = "Player";

    [Header("Optional: Hide on GameOver")]
    [SerializeField] Graphic hideOnGameOver;      // herhangi bir UI Graphic (Image/TMP/RawImage vs.)
    [SerializeField] float hidePopUp = 0.08f;     // önce minik büyüt
    [SerializeField] float hidePopDown = 0.22f;   // sonra küçülüp kaybolsun
    [SerializeField] float hideOvershoot = 1.05f; // minik büyüme oraný

    int finalScore;
    bool uploading;
    Sequence seq;      // UI pop sýrasý
    Tween scoreTween; // 0final sayaç tween

    void Awake()
    {
        if (!root) root = GetOrAdd<CanvasGroup>(gameObject);
        if (!dimBg) Debug.LogWarning("[GameOverUI] dimBg atanmadý (CanvasGroup).");
        if (!bigScore) Debug.LogWarning("[GameOverUI] bigScore atanmadý.");
        if (!nameInput) Debug.LogWarning("[GameOverUI] nameInput atanmadý.");
        if (!okButton) Debug.LogWarning("[GameOverUI] okButton atanmadý.");

        // Baþlangýç halleri (aktif kalýrlar!)
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
    // Hazýrlýk (baþlangýç)
    // -----------------------
    void PrepHiddenScore()
    {
        if (!bigScore) return;
        var rt = bigScore.rectTransform;
        rt.localScale = Vector3.one * popStartScale;

        var c = bigScore.color;
        c.a = 0f; // görünmez
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
    // DIÞARIDAN ÇAÐIR
    // -----------------------
    // Blackout -> Pause -> (hideOnGameOver pop-out) -> Score pop & count -> Name -> Input -> OK
    public void BlackoutThenShow(int score, float blackDurationOverride = -1f)
    {
        finalScore = Mathf.Max(0, score);
        Debug.Log($"[GameOverUI] incoming score = {finalScore}");

        gameObject.SetActive(true);
        root.interactable = true;
        root.blocksRaycasts = true;

        // Tüm öðeleri gizli baþlangýca çek (yeniden çaðrý güvenliði)
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
                // 2) kýsa tutma
                DOVirtual.DelayedCall(dimHold, () =>
                {
                    // 3) OYUNU DURDUR
                    Time.timeScale = 0f;

                    // 4) Dim’i 1
                    dimBg.DOFade(0.6f, dimToUi).SetUpdate(true);
                    root.DOFade(1f, dimToUi).SetUpdate(true);

                    // 4.5) (Opsiyonel) Ekrandaki bir görseli pop-out ile yok et
                    HideGraphicPopOut();

                    // 5) Popup sýrasý
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

    // Yarý-siy
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
    // Sýral
    // -----------------------
    void RunPopupSequence()
    {
        seq = DOTween.Sequence().SetUpdate(true); // unscaled
        float cursor = 0f;

        // -
        if (bigScore)
        {
            // görünür pop
            seq.Insert(cursor, bigScore.DOFade(1f, popDuration));
            seq.Insert(cursor, bigScore.rectTransform.DOScale(1f, popDuration).SetEase(Ease.OutBack));

            // 0final sayýmý
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

            // baþlangýçta kapalý
            cg.alpha = 0f;
            cg.interactable = false;           // << güvenlik
            cg.blocksRaycasts = false;         // << güvenlik

            seq.Insert(cursor, cg.DOFade(1f, popDuration));
            seq.Insert(cursor, rt.DOScale(1f, popDuration).SetEase(Ease.OutBack));

            // görünür olur olmaz etkileþimi aç
            seq.InsertCallback(cursor + popDuration, () =>
            {
                cg.interactable = true;        // << AÇ
                cg.blocksRaycasts = true;      // << AÇ
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

            // güvenlik: butonun image'ý raycast alsýn
            var img = okButton.GetComponent<Image>();
            if (img) img.raycastTarget = true;

            // güvenlik: ebeveyn canvasgrouplarý da týklama geçirsin
            EnableParentsForRaycast(okButton.transform);

            // en üste getir (ayný canvas içi)
            okButton.transform.SetAsLastSibling();

            // animasyon
            seq.Insert(cursor, cg.DOFade(1f, popDuration));
            seq.Insert(cursor, rt.DOScale(1f, popDuration).SetEase(Ease.OutBack));

            // görünür olur olmaz etkileþimi AÇ
            seq.InsertCallback(cursor + popDuration, () =>
            {
                cg.interactable = true;
                cg.blocksRaycasts = true;
                if (img) img.raycastTarget = true;
            });
        }

    }

    // Skor yazýsý için yumuþak loop (unscaled)
    void StartScoreIdlePulse()
    {
        if (bigScore == null) return;
        var rt = bigScore.rectTransform;
        rt.DOKill();

        // Çok hafif
        DOTween.Sequence().SetUpdate(true).SetLoops(-1, LoopType.Yoyo)
            .Append(rt.DOScale(1.05f, 0.55f).SetEase(Ease.InOutSine))
            .Join(rt.DOLocalRotate(new Vector3(0, 0, 2.2f), 0.55f).SetEase(Ease.InOutSine))
            .Append(rt.DOScale(1.00f, 0.55f).SetEase(Ease.InOutSine))
            .Join(rt.DOLocalRotate(Vector3.zero, 0.55f).SetEase(Ease.InOutSine));
    }

    // -----------------------
    // OK  GameManager’a delege
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
            Debug.LogWarning("[GameOverUI] GameManager bulunamadý, fallback ile ana menüye dönülüyor.");
            if (!string.IsNullOrEmpty(mainMenuScene))
                SceneManager.LoadScene(mainMenuScene);
        }
    }

    // -----------------------
    // Opsiyonel görseli pop-out ile yok et
    // -----------------------
    void HideGraphicPopOut()
    {
        if (hideOnGameOver == null) return;

        var rt = hideOnGameOver.rectTransform;
        var cg = hideOnGameOver.GetComponent<CanvasGroup>();
        if (!cg) cg = hideOnGameOver.gameObject.AddComponent<CanvasGroup>();

        // animasyona baþlamadan önce alpha 1 yap
        cg.alpha = 1f;
        rt.localScale = Vector3.one;

        // minik büyü  küçül fade inactive
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
        // köke kadar tüm CanvasGroup'larý aç
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

        // dim katmaný týklama yutmasýn
        if (dimBg)
        {
            dimBg.blocksRaycasts = false;
            // Ek: eðer dim Image var ve üstteyse:
            var dimGraphic = dimBg.GetComponent<Graphic>();
            if (dimGraphic) dimGraphic.raycastTarget = false;
        }
    }
}
