using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CoinPickup : MonoBehaviour
{
    [Header("Upward pop")]
    [SerializeField] float initialUpImpulse = 2.0f; // ilk yukarý itiþ
    [SerializeField] float lateralImpulse = 0.6f; // sað-sol rastgelelik
    [SerializeField] float spinTorque = 20f;  // hafif dönüþ

    [Header("UI flight")]
    [SerializeField] float uiEndScale = 0.2f; // UI hedefe uçarken ölçek

    Rigidbody2D rb;
    SpriteRenderer sr;

    GameManager gm;
    int value;
    bool initialized;

    // EnemyHealth çaðýrýr
    public void Initialize(GameManager gameManager, int v)
    {
        gm = gameManager;
        value = v;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        StartCoroutine(Flow());
        initialized = true;
    }

    IEnumerator Flow()
    {
        // 1) World: yukarý doðru kýsa bir pop + hafif sað/sol + spin
        if (rb)
        {
            float x = Random.Range(-lateralImpulse, lateralImpulse);
            float y = Random.Range(0.9f, 1.2f); // daima yukarý yön
            Vector2 dir = new Vector2(x, y).normalized;

            rb.AddForce(dir * initialUpImpulse, ForceMode2D.Impulse);
            if (rb.gravityScale < 0.1f) rb.gravityScale = 1f;
            rb.AddTorque(Random.Range(-spinTorque, spinTorque), ForceMode2D.Impulse);
        }

        // 2) Biraz sahnede kalsýn (görünsün)
        yield return new WaitForSeconds(gm != null ? gm.GetCoinWaitTime() : 1f);

        // 3) UI hedefe uç
        if (gm == null || gm.GetMainCanvas() == null || gm.GetMoneyIconRect() == null)
        {
            Destroy(gameObject);
            yield break;
        }

        // World coin'i gizle ve durdur
        if (sr) sr.enabled = false;
        if (rb) rb.velocity = Vector2.zero;

        // UI Image oluþtur
        Canvas canvas = gm.GetMainCanvas();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        GameObject uiObj = new GameObject("Coin_UI");
        uiObj.transform.SetParent(canvas.transform, false);
        Image img = uiObj.AddComponent<Image>();
        if (sr && sr.sprite) img.sprite = sr.sprite;

        RectTransform rt = img.rectTransform;

        // Baþlangýç: world pozisyonunu Canvas local point'e çevir
        Vector2 startLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Camera.main.WorldToScreenPoint(transform.position),
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out startLocal
        );
        rt.anchoredPosition = startLocal;
        rt.localScale = Vector3.one;

        // Hedef: money icon rect
        Vector2 targetLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, gm.GetMoneyIconRect().position),
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out targetLocal
        );

        float dur = gm.GetCoinMoveDuration();

        Sequence s = DOTween.Sequence();
        s.Append(rt.DOAnchorPos(targetLocal, dur).SetEase(Ease.InQuad));
        s.Join(rt.DOScale(uiEndScale, dur).SetEase(Ease.InQuad));
        s.OnComplete(() =>
        {
            gm.money += value;
            gm.UpdateMoney();
            Destroy(uiObj);
            Destroy(gameObject);
        });
    }

    // Güvenlik: Initialize unutulursa yine de çalýþsýn
    void Start()
    {
        if (!initialized)
        {
            gm = FindObjectOfType<GameManager>();
            Initialize(gm, 1);
        }
    }
}
