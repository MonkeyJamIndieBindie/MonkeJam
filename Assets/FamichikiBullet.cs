using System.Collections;
using UnityEngine;

public class FamichikiBullet : MonoBehaviour
{
    public float hitDammage;
    bool canGetBig = true;

    [Header("Impact VFX")]
    [SerializeField] ParticleSystem impactVfxPrefab;  //  buraya prefab'ýný sürükle

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

        // >>> ÇARPTIÐI NOKTADA VFX’i çalýþtýr
        PlayImpactVfx(transform.position);

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    void PlayImpactVfx(Vector3 pos)
    {
        if (impactVfxPrefab == null) return;

        // Instantiate + Play
        var ps = Instantiate(impactVfxPrefab, pos, Quaternion.identity);
        ps.Play();

        // Kendi kendini temizlemesi için yok et
        var main = ps.main;
        float life = main.duration + main.startLifetime.constantMax;
        Destroy(ps.gameObject, life + 0.1f);
    }
}
