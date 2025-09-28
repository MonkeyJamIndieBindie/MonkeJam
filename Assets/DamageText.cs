using UnityEngine;
using TMPro;
using DG.Tweening;

public static class DamageText
{
    public static void Show(Vector3 worldPos, string text, Color color, float rise = 0.8f, float duration = 0.6f, float startScale = 1f)
    {
        var go = new GameObject("DamageText", typeof(TextMeshPro), typeof(Canvas));
        var tm = go.GetComponent<TextMeshPro>();
        var canvas = go.GetComponent<Canvas>();

        // World-space TMP
        canvas.renderMode = RenderMode.WorldSpace;
        var tr = go.transform;
        tr.position = worldPos + new Vector3(0f, 0.5f, 0f);
        tr.localScale = Vector3.one * startScale;

        tm.text = text;
        tm.color = color;
        tm.fontSize = 6f;   // <<< daha büyük yazý
        tm.alignment = TextAlignmentOptions.Center;
        tm.enableKerning = true;
        tm.enableWordWrapping = false;
        tm.sortingOrder = 100;

        // Animasyon: yukarý süzül, büyü, fade-out
        var endPos = tr.position + new Vector3(0f, rise, 0f);
        var seq = DOTween.Sequence();
        seq.Join(tr.DOMove(endPos, duration).SetEase(Ease.OutQuad));
        seq.Join(tm.DOFade(0f, duration));
        seq.Join(tr.DOScale(startScale * 1.4f, duration * 0.5f).SetEase(Ease.OutBack)); // daha tok büyüme
        seq.OnComplete(() => Object.Destroy(go));
    }
}
