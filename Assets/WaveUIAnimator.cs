using UnityEngine;
using TMPro;
using DG.Tweening;

public class WaveUIAnimator : MonoBehaviour
{
    [System.Serializable]
    public class PanelEntry
    {
        public RectTransform rect;     // Panel Rect
        public bool fromBottom = false;// false=üstten gelir, true=alttan gelir
        public float offset = 600f;    // Dýþarýdaki baþlangýç uzaklýðý (px)
    }

    [Header("Panels")]
    [SerializeField] PanelEntry[] panels;
    [SerializeField] float inDuration = 0.55f;
    [SerializeField] float outDuration = 0.35f;
    [SerializeField] float stagger = 0.07f;
    [SerializeField] Ease inEase = Ease.OutCubic;
    [SerializeField] Ease outEase = Ease.InCubic;

    [Header("Dim & Title")]
    [SerializeField] CanvasGroup dimBg;            // Tam ekran siyah Image üstünde CanvasGroup
    [SerializeField] float dimTargetAlpha = 0.45f; // Kararma miktarý

    [SerializeField] TextMeshProUGUI titleText;    // Wave Completed!
    [SerializeField] string title = "Wave Completed!";
    [SerializeField] float titleFade = 0.35f;      // Baþlýk görünme/kaybolma
    [SerializeField] float titleShowDuration = 2f; // Görünür kalma süresi

    [Header("Title FX")]
    [SerializeField] float titleScaleShown = 1.18f;     // gösterimde ölçek
    [SerializeField] Vector2 titleDrift = new Vector2(28f, 14f); // çapraz drift (px)
    [SerializeField] Ease titleDriftEase = Ease.InOutSine;

    Sequence seq;
    Vector2[] origPos;
    Vector2 titleOrigPos;

    void Awake()
    {
        origPos = new Vector2[panels.Length];
        for (int i = 0; i < panels.Length; i++)
            if (panels[i].rect) origPos[i] = panels[i].rect.anchoredPosition;

        if (dimBg) dimBg.alpha = 0f;

        if (titleText)
        {
            titleText.text = title;
            titleText.alpha = 0f;
            titleText.rectTransform.localScale = Vector3.one;
            titleOrigPos = titleText.rectTransform.anchoredPosition;
        }
    }

    void OnDisable()
    {
        seq?.Kill();
        if (titleText)
        {
            titleText.alpha = 0f;
            titleText.rectTransform.localScale = Vector3.one;
            titleText.rectTransform.anchoredPosition = titleOrigPos;
        }
        // panelleri orijinale döndür
        for (int i = 0; i < panels.Length; i++)
            if (panels[i].rect) panels[i].rect.anchoredPosition = origPos[i];
    }

    // ------------------------------------------------------------
    // Yardýmcýlar
    // ------------------------------------------------------------
    Vector2 OffscreenFor(int i)
    {
        var p = panels[i];
        float dy = p.fromBottom ? -p.offset : +p.offset;
        return origPos[i] + new Vector2(0f, dy);
    }

    void PlacePanelsOffscreen()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            var r = panels[i].rect;
            if (!r) continue;
            r.DOKill();
            r.anchoredPosition = OffscreenFor(i);
        }
    }

    // ------------------------------------------------------------
    // Dizi 1: OYUN ÝLK AÇILIÞ (baþlýk YOK)
    // ------------------------------------------------------------
    public void PlayIntro()
    {
        seq?.Kill();
        PlacePanelsOffscreen();

        seq = DOTween.Sequence();

        if (dimBg)
            seq.Join(dimBg.DOFade(dimTargetAlpha, inDuration)); // dim yavaþça gelsin

        float t0 = seq.Duration(false);
        for (int i = 0; i < panels.Length; i++)
        {
            var r = panels[i].rect;
            if (!r) continue;
            seq.Insert(t0 + i * stagger,
                r.DOAnchorPos(origPos[i], inDuration).SetEase(inEase));
        }
    }

    // ------------------------------------------------------------
    // Dizi 2: START THE WAVE (paneller dýþarý, dim kapanýr)
    // ------------------------------------------------------------
    public void PlayStartWave()
    {
        seq?.Kill();
        seq = DOTween.Sequence();

        // Paneller dýþarý
        for (int i = 0; i < panels.Length; i++)
        {
            var r = panels[i].rect;
            if (!r) continue;
            r.DOKill();
            seq.Insert(i * stagger,
                r.DOAnchorPos(OffscreenFor(i), outDuration).SetEase(outEase));
        }

        // Dim hýzlýca kapanýr
        if (dimBg) seq.Join(dimBg.DOFade(0f, 0.2f));

        // Baþlýk görünmesin
        if (titleText)
        {
            titleText.DOKill();
            titleText.alpha = 0f;
            titleText.rectTransform.localScale = Vector3.one;
            titleText.rectTransform.anchoredPosition = titleOrigPos;
        }
    }

    // ------------------------------------------------------------
    // Dizi 3: END WAVE (dim + baþlýk tam opak + drift + paneller içeri)
    // ------------------------------------------------------------
    public void PlayEndWave()
    {
        seq?.Kill();
        PlacePanelsOffscreen();
        MusicManager.Instance.PlayStateMusic(GameState.Market);

        seq = DOTween.Sequence();

        // Dim yavaþça gelsin
        if (dimBg) seq.Join(dimBg.DOFade(dimTargetAlpha, inDuration));

        // Baþlýk akýþý: tam opak, büyük ve hafif çapraz drift
        if (titleText)
        {
            var tRect = titleText.rectTransform;
            titleText.DOKill();

            // baþlangýç
            titleText.alpha = 0f;
            tRect.localScale = Vector3.one * (titleScaleShown * 0.9f);
            tRect.anchoredPosition = titleOrigPos;

            // Fade-in  tam opaklýða, scale  titleScaleShown
            seq.Append(titleText.DOFade(1f, titleFade)); // FULL opacity
            seq.Join(tRect.DOScale(titleScaleShown, titleFade).SetEase(Ease.OutBack));

            // Gösterim süresince hafif çapraz drift
            seq.Join(tRect.DOAnchorPos(titleOrigPos + titleDrift, Mathf.Max(0.01f, titleShowDuration))
                           .SetEase(titleDriftEase));

            // Gösterim sonrasý: fade-out + yerini sýfýrla
            seq.Append(titleText.DOFade(0f, titleFade * 0.8f));
            seq.Join(tRect.DOScale(1f, titleFade * 0.8f).SetEase(Ease.InSine));
            seq.AppendCallback(() =>
            {
                tRect.anchoredPosition = titleOrigPos;
            });
        }

        // Paneller içeri
        float t0 = seq.Duration(false);
        for (int i = 0; i < panels.Length; i++)
        {
            var r = panels[i].rect;
            if (!r) continue;
            seq.Insert(t0 + i * stagger,
                r.DOAnchorPos(origPos[i], inDuration).SetEase(inEase));
        }
    }
}
