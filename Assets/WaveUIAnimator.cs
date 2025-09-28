using UnityEngine;
using TMPro;
using DG.Tweening;

public class WaveUIAnimator : MonoBehaviour
{
    [System.Serializable]
    public class PanelEntry
    {
        public RectTransform rect;     // Panel Rect
        public bool fromBottom = false;// false=�stten gelir, true=alttan gelir
        public float offset = 600f;    // D��ar�daki ba�lang�� uzakl��� (px)
    }

    [Header("Panels")]
    [SerializeField] PanelEntry[] panels;
    [SerializeField] float inDuration = 0.55f;
    [SerializeField] float outDuration = 0.35f;
    [SerializeField] float stagger = 0.07f;
    [SerializeField] Ease inEase = Ease.OutCubic;
    [SerializeField] Ease outEase = Ease.InCubic;

    [Header("Dim & Title")]
    [SerializeField] CanvasGroup dimBg;            // Tam ekran siyah Image �st�nde CanvasGroup
    [SerializeField] float dimTargetAlpha = 0.45f; // Kararma miktar�

    [SerializeField] TextMeshProUGUI titleText;    // Wave Completed!
    [SerializeField] string title = "Wave Completed!";
    [SerializeField] float titleFade = 0.35f;      // Ba�l�k g�r�nme/kaybolma
    [SerializeField] float titleShowDuration = 2f; // G�r�n�r kalma s�resi

    [Header("Title FX")]
    [SerializeField] float titleScaleShown = 1.18f;     // g�sterimde �l�ek
    [SerializeField] Vector2 titleDrift = new Vector2(28f, 14f); // �apraz drift (px)
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
        // panelleri orijinale d�nd�r
        for (int i = 0; i < panels.Length; i++)
            if (panels[i].rect) panels[i].rect.anchoredPosition = origPos[i];
    }

    // ------------------------------------------------------------
    // Yard�mc�lar
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
    // Dizi 1: OYUN �LK A�ILI� (ba�l�k YOK)
    // ------------------------------------------------------------
    public void PlayIntro()
    {
        seq?.Kill();
        PlacePanelsOffscreen();

        seq = DOTween.Sequence();

        if (dimBg)
            seq.Join(dimBg.DOFade(dimTargetAlpha, inDuration)); // dim yava��a gelsin

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
    // Dizi 2: START THE WAVE (paneller d��ar�, dim kapan�r)
    // ------------------------------------------------------------
    public void PlayStartWave()
    {
        seq?.Kill();
        seq = DOTween.Sequence();

        // Paneller d��ar�
        for (int i = 0; i < panels.Length; i++)
        {
            var r = panels[i].rect;
            if (!r) continue;
            r.DOKill();
            seq.Insert(i * stagger,
                r.DOAnchorPos(OffscreenFor(i), outDuration).SetEase(outEase));
        }

        // Dim h�zl�ca kapan�r
        if (dimBg) seq.Join(dimBg.DOFade(0f, 0.2f));

        // Ba�l�k g�r�nmesin
        if (titleText)
        {
            titleText.DOKill();
            titleText.alpha = 0f;
            titleText.rectTransform.localScale = Vector3.one;
            titleText.rectTransform.anchoredPosition = titleOrigPos;
        }
    }

    // ------------------------------------------------------------
    // Dizi 3: END WAVE (dim + ba�l�k tam opak + drift + paneller i�eri)
    // ------------------------------------------------------------
    public void PlayEndWave()
    {
        seq?.Kill();
        PlacePanelsOffscreen();
        MusicManager.Instance.PlayStateMusic(GameState.Market);

        seq = DOTween.Sequence();

        // Dim yava��a gelsin
        if (dimBg) seq.Join(dimBg.DOFade(dimTargetAlpha, inDuration));

        // Ba�l�k ak���: tam opak, b�y�k ve hafif �apraz drift
        if (titleText)
        {
            var tRect = titleText.rectTransform;
            titleText.DOKill();

            // ba�lang��
            titleText.alpha = 0f;
            tRect.localScale = Vector3.one * (titleScaleShown * 0.9f);
            tRect.anchoredPosition = titleOrigPos;

            // Fade-in  tam opakl��a, scale  titleScaleShown
            seq.Append(titleText.DOFade(1f, titleFade)); // FULL opacity
            seq.Join(tRect.DOScale(titleScaleShown, titleFade).SetEase(Ease.OutBack));

            // G�sterim s�resince hafif �apraz drift
            seq.Join(tRect.DOAnchorPos(titleOrigPos + titleDrift, Mathf.Max(0.01f, titleShowDuration))
                           .SetEase(titleDriftEase));

            // G�sterim sonras�: fade-out + yerini s�f�rla
            seq.Append(titleText.DOFade(0f, titleFade * 0.8f));
            seq.Join(tRect.DOScale(1f, titleFade * 0.8f).SetEase(Ease.InSine));
            seq.AppendCallback(() =>
            {
                tRect.anchoredPosition = titleOrigPos;
            });
        }

        // Paneller i�eri
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
