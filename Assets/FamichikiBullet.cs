using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FamichikiBullet : MonoBehaviour
{
    public float hitDammage;
    bool canGetBig = true;

    [Header("Impact VFX")]
    [SerializeField] ParticleSystem impactVfxPrefab;  // buraya prefab'ýný sürükle

    [Header("Cartoon Flight")]
    [SerializeField] float spinSpeedDegPerSec = 720f;     // sürekli spin
    [SerializeField] Vector3 wobblePunch = new Vector3(0.15f, -0.15f, 0f);
    [SerializeField] float wobbleTime = 0.22f;

    Tween spinT;
    Tween wobbleT;

    public void StartCartoonFlight()
    {
        // Sürekli spin (speed-based)
        spinT?.Kill();
        spinT = transform
            .DORotate(new Vector3(0f, 0f, 360f), spinSpeedDegPerSec, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetSpeedBased(true)
            .SetLoops(-1, LoopType.Incremental);

        // Hafif squash-stretch wobble (loop)
        wobbleT?.Kill();
        wobbleT = transform
            .DOPunchScale(wobblePunch, wobbleTime, 8, 0.9f)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (canGetBig) StartCoroutine(GetBig());
            var ew = collision.gameObject.GetComponent<EnemyWalk>();
            if (ew != null)
            {
                ew.enemySpeed -= 1;
                ew.hit = true;
            }
        }

        if (collision.gameObject.CompareTag("Zemin"))
        {
            if (canGetBig) StartCoroutine(GetBig());
        }
    }

    IEnumerator GetBig()
    {
        canGetBig = false;

        // Tweenleri kapat
        spinT?.Kill();
        wobbleT?.Kill();

        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }

        var cc = GetComponent<CircleCollider2D>();
        if (cc) cc.radius = 2f;

        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.enabled = false;

        // Çarpma noktasýnda VFX
        PlayImpactVfx(transform.position);

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    void PlayImpactVfx(Vector3 pos)
    {
        if (impactVfxPrefab == null) return;

        var ps = Instantiate(impactVfxPrefab, pos, Quaternion.identity);
        ps.Play();

        var main = ps.main;
        float life = main.duration + main.startLifetime.constantMax;
        Destroy(ps.gameObject, life + 0.1f);
    }

    void OnDestroy()
    {
        spinT?.Kill();
        wobbleT?.Kill();
    }
}
