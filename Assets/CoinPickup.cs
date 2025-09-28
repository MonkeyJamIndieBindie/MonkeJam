using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CoinPickup : MonoBehaviour
{
    [Header("Upward pop")]
    [SerializeField] float initialUpImpulse = 2.0f; // ilk yukar� iti�
    [SerializeField] float lateralImpulse = 0.6f;   // sa�-sol rastgelelik
    [SerializeField] float spinTorque = 20f;        // hafif d�n��

    [Header("UI flight")]
    [SerializeField] float uiEndScale = 0.2f;       // UI hedefe u�arken �l�ek
    [SerializeField] Ease uiEase = Ease.InQuad;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Collider2D col;

    GameManager gm;
    int value;
    bool initialized;

    // EnemyHealth gibi bir yerden �a�r�l�r
    public void Initialize(GameManager gameManager, int v)
    {
        gm = gameManager;
        value = v;

        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!sr) sr = GetComponent<SpriteRenderer>();
        if (!col) col = GetComponent<Collider2D>();

        StartCoroutine(Flow());
        initialized = true;
    }

    IEnumerator Flow()
    {
        // 1) World: k�sa bir yukar� pop + hafif sa�/sol + spin
        if (rb)
        {
            float x = UnityEngine.Random.Range(-lateralImpulse, lateralImpulse);
            float y = UnityEngine.Random.Range(0.9f, 1.2f);
            Vector2 dir = new Vector2(x, y).normalized;

            rb.AddForce(dir * initialUpImpulse, ForceMode2D.Impulse);
            if (rb.gravityScale < 0.1f) rb.gravityScale = 1f;
            rb.AddTorque(UnityEngine.Random.Range(-spinTorque, spinTorque), ForceMode2D.Impulse);
        }

        // 2) Biraz sahnede dursun
        float wait = gm != null ? gm.GetCoinWaitTime() : 1f;
        yield return new WaitForSeconds(wait);

        // 3) UI hedefe u�
        if (gm == null || gm.GetMainCanvas() == null || gm.GetMoneyIconRect() == null)
        {
            // Yine de paray� ekle (failsafe)
            if (gm != null) gm.OnMoneyGained(value);
            Destroy(gameObject);
            yield break;
        }

        // World coin'i gizle ve etkile�imi kapat
        if (sr) sr.enabled = false;
        if (col) col.enabled = false;
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }

        Canvas canvas = gm.GetMainCanvas();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // UI Image olu�tur
        GameObject uiObj = new GameObject("Coin_UI", typeof(RectTransform), typeof(Image));
        uiObj.transform.SetParent(canvas.transform, false);
        uiObj.transform.SetAsLastSibling(); // �stte dursun

        Image img = uiObj.GetComponent<Image>();
        if (sr && sr.sprite) img.sprite = sr.sprite;

        RectTransform rt = img.rectTransform;

        // Ba�lang��: world pozisyonundan canvas local point
        Vector2 startLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Camera.main.WorldToScreenPoint(transform.position),
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out startLocal
        );
        rt.anchoredPosition = startLocal;

        // Hedef: money icon rect'in ekran koordinat�ndan canvas local point
        Vector2 targetLocal;
        Vector3 iconWorldPos = gm.GetMoneyIconRect().position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, iconWorldPos),
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out targetLocal
        );

        // Ba�lang�� boyutu: ikonun boyutuna yak�n bir de�er (opsiyonel)
        rt.sizeDelta = gm.GetMoneyIconRect().rect.size;
        rt.localScale = Vector3.one;

        float dur = gm.GetCoinMoveDuration();

        // U�u�
        Sequence s = DOTween.Sequence();
        s.Append(rt.DOAnchorPos(targetLocal, dur).SetEase(uiEase));
        s.Join(rt.DOScale(uiEndScale, dur).SetEase(uiEase));
        s.OnComplete(() =>
        {
            // Para ekleme ve MoneyUIFX tetikleme
            gm.OnMoneyGained(value);

            // Temizlik
            Destroy(uiObj);
            Destroy(gameObject);
        });
    }

    void Start()
    {
        if (!initialized)
        {
            gm = FindObjectOfType<GameManager>();
            Initialize(gm, 1);
        }
    }
}
