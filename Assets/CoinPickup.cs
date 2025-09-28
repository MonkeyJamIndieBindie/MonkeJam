using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CoinPickup : MonoBehaviour
{
    [Header("Upward pop")]
    [SerializeField] float initialUpImpulse = 2.0f; // ilk yukar� iti�
    [SerializeField] float lateralImpulse = 0.6f; // sa�-sol rastgelelik
    [SerializeField] float spinTorque = 20f;  // hafif d�n��

    [Header("UI flight")]
    [SerializeField] float uiEndScale = 0.2f; // UI hedefe u�arken �l�ek

    Rigidbody2D rb;
    SpriteRenderer sr;

    GameManager gm;
    int value;
    bool initialized;

    // EnemyHealth �a��r�r
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
        // 1) World: yukar� do�ru k�sa bir pop + hafif sa�/sol + spin
        if (rb)
        {
            float x = Random.Range(-lateralImpulse, lateralImpulse);
            float y = Random.Range(0.9f, 1.2f); // daima yukar� y�n
            Vector2 dir = new Vector2(x, y).normalized;

            rb.AddForce(dir * initialUpImpulse, ForceMode2D.Impulse);
            if (rb.gravityScale < 0.1f) rb.gravityScale = 1f;
            rb.AddTorque(Random.Range(-spinTorque, spinTorque), ForceMode2D.Impulse);
        }

        // 2) Biraz sahnede kals�n (g�r�ns�n)
        yield return new WaitForSeconds(gm != null ? gm.GetCoinWaitTime() : 1f);

        // 3) UI hedefe u�
        if (gm == null || gm.GetMainCanvas() == null || gm.GetMoneyIconRect() == null)
        {
            Destroy(gameObject);
            yield break;
        }

        // World coin'i gizle ve durdur
        if (sr) sr.enabled = false;
        if (rb) rb.velocity = Vector2.zero;

        // UI Image olu�tur
        Canvas canvas = gm.GetMainCanvas();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        GameObject uiObj = new GameObject("Coin_UI");
        uiObj.transform.SetParent(canvas.transform, false);
        Image img = uiObj.AddComponent<Image>();
        if (sr && sr.sprite) img.sprite = sr.sprite;

        RectTransform rt = img.rectTransform;

        // Ba�lang��: world pozisyonunu Canvas local point'e �evir
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

    // G�venlik: Initialize unutulursa yine de �al��s�n
    void Start()
    {
        if (!initialized)
        {
            gm = FindObjectOfType<GameManager>();
            Initialize(gm, 1);
        }
    }
}
