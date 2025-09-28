using UnityEngine;
using TMPro;
using DG.Tweening;

public class MoneyUIFX : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Canvas mainCanvas;
    [SerializeField] RectTransform moneyIconRect;

    [Header("Icon Pop")]
    [SerializeField] float popScale = 1.12f;
    [SerializeField] float popDuration = 0.12f;

    [Header("Floating Text")]
    [SerializeField] Vector2 textOffset = new Vector2(30f, 10f);
    [SerializeField] float textRise = 60f;
    [SerializeField] float textDuration = 0.6f;
    [SerializeField] int textFontSize = 28;
    [SerializeField] Color textColor = new Color(1f, 0.95f, 0.3f, 1f); // sarýmsý

    Vector3 _iconStartScale = Vector3.one;

    void Awake()
    {
        if (moneyIconRect != null) _iconStartScale = moneyIconRect.localScale;
    }

    public void PlayGain(float amount)
    {
        // 1) Icon Pop
        if (moneyIconRect != null)
        {
            moneyIconRect.DOKill();
            moneyIconRect.localScale = _iconStartScale;
            moneyIconRect.DOScale(_iconStartScale * popScale, popDuration)
                         .SetEase(Ease.OutQuad)
                         .SetLoops(2, LoopType.Yoyo);
        }

        // 2) Floating +X Text
        if (mainCanvas == null || moneyIconRect == null) return;

        var go = new GameObject("MoneyGainText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(CanvasGroup));
        var rt = go.GetComponent<RectTransform>();
        var tmp = go.GetComponent<TextMeshProUGUI>();
        var cg = go.GetComponent<CanvasGroup>();

        go.transform.SetParent(moneyIconRect.parent, false); // ayný canvas hiyerarþisi
        rt.anchorMin = moneyIconRect.anchorMin;
        rt.anchorMax = moneyIconRect.anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = moneyIconRect.anchoredPosition + textOffset;

        tmp.text = $"+{amount:0}";
        tmp.fontSize = textFontSize;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        cg.alpha = 1f;

        // hareket ve fade
        var startPos = rt.anchoredPosition;
        var endPos = startPos + new Vector2(0f, textRise);

        Sequence s = DOTween.Sequence();
        s.Join(rt.DOAnchorPos(endPos, textDuration).SetEase(Ease.OutQuad));
        s.Join(cg.DOFade(0f, textDuration));
        s.OnComplete(() => Destroy(go));
    }
}
