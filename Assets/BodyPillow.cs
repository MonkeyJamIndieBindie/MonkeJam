using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BodyPillow : MonoBehaviour
{
    [Header("Damage / AOE (ops.)")]
    public float hitDammage = 2f;
    [SerializeField] float aoeRadius = 0f;
    [SerializeField] LayerMask enemyMask;

    [Header("CAST (spawn)")]
    [SerializeField] float castTime = 0.18f;          // hýzlý üretim
    [SerializeField] float castStartScale = 0.25f;     // çok küçükten
    [SerializeField] float castForwardShift = 0.35f;   // havada hafif ileri kay
    [SerializeField] int castSpinHalfTurns = 3;      // 3 * 180° = 540°

    [Header("FALL (air)")]
    [SerializeField] float fallGravity = 2.2f;
    [SerializeField] float swayAngle = 8f;             // düþerken sað-sol salýným
    [SerializeField] float swayTime = 0.35f;

    [Header("IMPACT")]
    [SerializeField] ParticleSystem impactVfx;
    [SerializeField] float landSquashX = 1.22f;
    [SerializeField] float landSquashY = 0.78f;
    [SerializeField] float bounceHeight = 0.32f;
    [SerializeField] float bounceTime = 0.14f;
    [SerializeField] float settleTime = 0.10f;
    [SerializeField] float lifeAfterImpact = 0.22f;

    [Header("SFX (ops)")]
    [SerializeField] string sfxCastId = "spell_cast";
    [SerializeField] string sfxImpactId = "pillow_impact";

    Rigidbody2D rb;
    SpriteRenderer sr;

    Tween castSeqT;
    Tween swayT;
    bool hasLanded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (rb != null)
        {
            rb.isKinematic = true;      // cast sýrasýnda fizik yok
            rb.freezeRotation = true;   // düþerken rotasyonu biz kontrol ediyoruz
        }
    }

    void OnEnable()
    {
        PlayCastThenFall();
    }

    // ---------------- CAST -> FALL ----------------
    void PlayCastThenFall()
    {
        // baþlangýç: çok küçük + biraz þeffaf
        if (sr != null)
        {
            var c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, 0.9f);
        }
        transform.localScale = Vector3.one * castStartScale;

        // hýzlý üretim animasyonu: küçüklükten büyü + hafif ileri kay + 540° spin
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + transform.right * castForwardShift;
        float spinDeg = castSpinHalfTurns * 180f; // 540°

        SFXManager.Instance?.Play(sfxCastId);

        // Tek bir sequence ile tam yatayda bitiriyoruz (0° mod 180°)
        castSeqT?.Kill();
        var seq = DOTween.Sequence();
        seq.Join(transform.DOMove(endPos, castTime).SetEase(Ease.OutCubic));
        seq.Join(transform.DOScale(1f, castTime).SetEase(Ease.OutBack, 1.4f));
        seq.Join(transform.DORotate(new Vector3(0, 0, spinDeg), castTime, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic));
        seq.OnComplete(() =>
        {
            // tam yatayla (0°) hizala ve düþmeye baþla
            var e = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(0, 0, 0f); // yatay
            BeginFalling();
        });
        castSeqT = seq;
    }

    void BeginFalling()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.gravityScale = fallGravity;
        }

        // düþerken yastýk gibi sað-sol salýným (yoyo rotasyon)
        swayT?.Kill();
        swayT = transform
            .DORotate(new Vector3(0, 0, swayAngle), swayTime)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // ---------------- IMPACT ----------------
    void OnTriggerEnter2D(Collider2D col)
    {
        if (hasLanded) return;

        if (col.CompareTag("Zemin") || col.CompareTag("Enemy"))
        {
            hasLanded = true;
            StartCoroutine(LandSequence());
        }
    }

    IEnumerator LandSequence()
    {
        // tweenleri kes
        castSeqT?.Kill();
        swayT?.Kill();

        // fizik dursun
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }

        // AOE (opsiyonel)
        if (aoeRadius > 0.01f)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius, enemyMask);
            foreach (var h in hits)
            {
                if (!h) continue;
                // kendi health sistemine göre hasar ver:
                // var hp = h.GetComponent<EnemyBase>(); if (hp) hp.TakeDamage(hitDammage);
                DamageText.Show(h.transform.position, $"-{hitDammage:0}", new Color(1f, 0.8f, 0.9f, 1f));
            }
        }

        if (impactVfx) Instantiate(impactVfx, transform.position, Quaternion.identity).Play();
        SFXManager.Instance?.Play(sfxImpactId);

        Vector3 p0 = transform.position;

        // yumuþak squash + minik zýplama + otur
        Sequence land = DOTween.Sequence();
        land.Append(transform.DOScale(new Vector3(landSquashX, landSquashY, 1f), 0.07f).SetEase(Ease.OutCubic));
        land.Append(transform.DOMoveY(p0.y + bounceHeight, bounceTime).SetEase(Ease.OutQuad));
        land.Join(transform.DOScale(Vector3.one, bounceTime).SetEase(Ease.OutSine));
        land.Append(transform.DOMoveY(p0.y, settleTime).SetEase(Ease.InQuad));
        land.OnComplete(() =>
        {
            transform.DOPunchScale(new Vector3(0.03f, -0.03f, 0), 0.1f, 6, 0.9f);
        });

        yield return land.WaitForCompletion();
        yield return new WaitForSeconds(lifeAfterImpact);

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (aoeRadius > 0f)
        {
            Gizmos.color = new Color(1f, 0.8f, 0.9f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, aoeRadius);
        }
    }

    void OnDestroy()
    {
        castSeqT?.Kill();
        swayT?.Kill();
    }
}
