using System.Collections;
using UnityEngine;
using DG.Tweening;

public class SnakeBullet : MonoBehaviour
{
    public float hitDammage = 0f;          // ilk çarpmada düz hasar kullanmýyoruz
    public float poisonDammage = 1f;

    [Header("Poison Tick")]
    [SerializeField] int poisonTicks = 6;
    [SerializeField] float tickInterval = 0.12f;

    [Header("On Ground AOE")]
    [SerializeField] float groundAoeRadius = 2.4f;
    [SerializeField] LayerMask enemyMask;

    [Header("Poison Area VFX (zeminde mor halka)")]
    [SerializeField] PoisonAreaVFX poisonAreaPrefab; //  PoisonAreaVFX prefabýný sürükle

    bool poison = true;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!poison) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            poison = false;
            StartCoroutine(ApplyPoisonToEnemy(collision.gameObject));
        }
        else if (collision.gameObject.CompareTag("Zemin"))
        {
            poison = false;
            StartCoroutine(ApplyPoisonArea(transform.position));
        }
    }

    IEnumerator ApplyPoisonToEnemy(GameObject enemyGo)
    {
        // Mermiyi sakla/durdur
        HideAndFreeze();

        // Tek bir hedefe zehir tikleri
        yield return StartCoroutine(PoisonTicks(enemyGo));

        Destroy(gameObject);
    }

    IEnumerator ApplyPoisonArea(Vector3 center)
    {
        // Mermiyi sakla/durdur
        HideAndFreeze();

        // Yere vurdu  yarýçap içindeki düþmanlara zehir uygula
        var hits = Physics2D.OverlapCircleAll(center, groundAoeRadius, enemyMask);
        foreach (var h in hits)
        {
            if (h != null && h.CompareTag("Enemy"))
                StartCoroutine(PoisonTicks(h.gameObject)); // her birini baðýmsýz týkla
        }

        //  VFX: zeminde mor, yavaþça nabýz atan halka
        if (poisonAreaPrefab != null)
        {
            float vfxDuration = poisonTicks * tickInterval + 0.1f;
            var vfx = Instantiate(poisonAreaPrefab, center, Quaternion.identity);
            vfx.Init(groundAoeRadius, vfxDuration);
        }

        yield return new WaitForSeconds(poisonTicks * tickInterval + 0.1f);
        Destroy(gameObject);
    }

    IEnumerator PoisonTicks(GameObject enemyGo)
    {
        if (enemyGo == null) yield break;

        SpriteRenderer sr = enemyGo.GetComponentInChildren<SpriteRenderer>();
        Color normal = sr ? sr.color : Color.white;
        Color poisonCol = new Color(0.7f, 0.2f, 1f, 1f); // mor

        for (int i = 0; i < poisonTicks; i++)
        {
            if (enemyGo == null) break;

            // Mor flaþ (milisaniye) geri
            if (sr)
            {
                sr.DOKill();
                sr.DOColor(poisonCol, 0.05f).SetEase(Ease.OutSine);
                sr.DOColor(normal, 0.12f).SetDelay(0.05f).SetEase(Ease.InSine);
            }

            // Hasar uygula (projendeki health sistemine baðla)
            TryDamage(enemyGo, poisonDammage);

            // Hasar yazýsý (daha büyük font için DamageText.cs güncel)
            DamageText.Show(enemyGo.transform.position, "-1", poisonCol, 0.5f, 0.45f, 1f);

            yield return new WaitForSeconds(tickInterval);
        }
    }

    void TryDamage(GameObject enemyGo, float dmg)
    {
        // Projendeki health component’e göre doldur:
        // var h1 = enemyGo.GetComponent<EnemyBase>(); if (h1) h1.TakeDamage(dmg);
        // veya:
        // var h2 = enemyGo.GetComponent<EnemyHealth>(); if (h2) h2.Apply(dmg);
    }

    void HideAndFreeze()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.isKinematic = true;
        }

        var spr = GetComponent<SpriteRenderer>();
        if (spr) spr.enabled = false;

        var box = GetComponent<BoxCollider2D>();
        if (box) box.size = new Vector2(3f, .5f); // senin eski ayarýn
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.7f, 0.2f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, groundAoeRadius);
    }
}
