using UnityEngine;
using DG.Tweening;

public class HeartBullet : MonoBehaviour
{
    public Transform enemy;
    [SerializeField] float shootPower = 6f;
    public float hitDammage = 1f;

    [Header("Cartoon Flight")]
    [SerializeField] float spinSpeedDegPerSec = 540f;
    [SerializeField] Vector3 wobblePunch = new Vector3(0.12f, -0.12f, 0f);
    [SerializeField] float wobbleTime = 0.2f;

    Tween spinT, wobbleT;

    void OnEnable()
    {
        // cartoony uçuþ
        spinT = transform
            .DORotate(new Vector3(0, 0, 360f), spinSpeedDegPerSec, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear).SetSpeedBased(true).SetLoops(-1, LoopType.Incremental);

        wobbleT = transform
            .DOPunchScale(wobblePunch, wobbleTime, 8, 0.9f)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void Update()
    {
        if (enemy == null) { Destroy(gameObject); return; }
        transform.position = Vector2.MoveTowards(transform.position, enemy.position, shootPower * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Enemy")) return;

        // (Opsiyonel) düþmana damage ver
        TryDamage(collision.gameObject, hitDammage);

        Destroy(gameObject);
    }

    void TryDamage(GameObject target, float dmg)
    {
        // projenin düþman health API’sine göre doldur.
        var h1 = target.GetComponent<EnemyBase>(); // örnek
        if (h1 != null)
        {
            // h1.TakeDamage(dmg);
        }
        // … baþka health bileþenleri varsa burada dene
        DamageText.Show(target.transform.position, $"-{dmg:0}", new Color(1f, 0.3f, 0.5f, 1f), 0.6f, 0.5f, 1f);
    }

    void OnDestroy()
    {
        spinT?.Kill();
        wobbleT?.Kill();
    }
}
