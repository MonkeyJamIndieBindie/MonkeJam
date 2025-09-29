using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyWalk : MonoBehaviour
{
    Transform connor;
    public float enemySpeed;
    public bool hit;

    Animator anim;
    Rigidbody2D rb;

    public EnemyType enemyType;

    bool canHit = true;
    public float coolDown;
    [SerializeField] float dammage;
    [SerializeField] float shootingPower;
    [SerializeField] Transform launchPoint;
    [SerializeField] GameObject bone;

    [Header("Trajectory Display")]
    public LineRenderer lineRender;
    int linePoints = 175;
    float timeItervalPoints = .01f;

    GameManager manager;

    [Header("Roll FX")]
    [SerializeField] float rollSpinSpeed = 720f;      // derece/sn
    [SerializeField] float rollAccelTime = 1.2f;      // hýzlanma süresi
    [SerializeField] float rollImpactForce = 8f;      // çarpýnca geri fýrlatma kuvveti
    [SerializeField] Vector2 rollImpactRandom = new Vector2(2f, 3.5f); // X/Y rastgele aralýk

    public enum EnemyType
    {
        Slow,
        Fast,
        Roll,
        Far
    }

    private void Start()
    {
        connor = GameObject.FindGameObjectWithTag("Kale").transform;
        anim = GetComponent<Animator>();
        manager = GameObject.FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody2D>();
        lineRender = GetComponent<LineRenderer>();

        dammage *= manager.levelHardnes[manager.levelCount].y;

        if (enemyType == EnemyType.Roll)
        {
            // Baþlangýçta dönme animasyonu
            transform.DORotate(new Vector3(0, 0, -360f), 0.5f, RotateMode.LocalAxisAdd)
                     .SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
            // Yavaþça hýzlan
            DOTween.To(() => enemySpeed, x => enemySpeed = x, enemySpeed * 1.8f, rollAccelTime)
                   .SetEase(Ease.OutQuad);
        }
    }

    private void Update()
    {
        if (enemyType == EnemyType.Roll)
        {
            // Sadece X ekseninde kuleye doðru yaklaþsýn
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(connor.position.x, transform.position.y), enemySpeed * Time.deltaTime);
        }
        else if (enemyType == EnemyType.Far)
        {
            if (Vector2.Distance(connor.position, transform.position) > 10f)
            {
                transform.position = Vector2.MoveTowards(new Vector2(transform.position.x, transform.position.y),
                    new Vector2(connor.position.x, transform.position.y),
                    enemySpeed * Time.deltaTime);
                anim.SetBool("Walk", true);
            }
            else
            {
                if (canHit)
                {
                    StartCoroutine(LongHit());
                }
            }
        }
        else
        {
            if (Vector2.Distance(connor.position, transform.position) > 1.5f)
            {
                transform.position = Vector2.MoveTowards(transform.position,
                    new Vector2(connor.position.x, transform.position.y),
                    enemySpeed * Time.deltaTime);
                anim.SetBool("Walk", true);
            }
            else
            {
                if (canHit)
                {
                    if (enemyType == EnemyType.Slow) StartCoroutine(HammerAttack());
                    if (enemyType == EnemyType.Fast) StartCoroutine(SwordAttack());
                }
                anim.SetBool("Walk", false);
            }
        }

        if (hit)
        {
            StartCoroutine(GetFaster());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (enemyType == EnemyType.Roll && collision.gameObject.CompareTag("Kale"))
        {
            HurtTower();
            manager.enemyKilledInWave++;
            manager.CheckEndWave();

            // Çarpýnca dönmeyi durdur
            transform.DOKill();

            // Cartoon fýrlama (mevcut kodun kalsýn)
            if (rb)
            {
                rb.isKinematic = false;
                rb.velocity = Vector2.zero;
                Vector2 dir = new Vector2(Random.Range(-rollImpactRandom.x, rollImpactRandom.x),
                                          Random.Range(1f, rollImpactRandom.y)).normalized;
                rb.AddForce(dir * rollImpactForce, ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-300f, 300f));
            }

            // >>> COIN DÖKME SÝNYALÝ (wave'i burada SAYDIÐIN için false ver)
            var eh = GetComponent<EnemyHealth>();
            if (eh != null) eh.ForceKillAndLoot(countWaveKill: false);

            GetComponent<EnemyHealth>().health -= 1;
        }
    }

    void HurtTower()
    {
        manager.towerHealth -= dammage;
        manager.UpdateHeath();
    }

    public IEnumerator GetFaster()
    {
        hit = false;
        yield return new WaitForSeconds(1f);
        enemySpeed = 3;
    }

    IEnumerator HammerAttack()
    {
        canHit = false;
        anim.SetBool("Attack", true);
        yield return new WaitForSeconds(1.5f);
        anim.SetBool("Attack", false);
        yield return new WaitForSeconds(coolDown);
        canHit = true;
    }

    IEnumerator SwordAttack()
    {
        canHit = false;
        anim.SetBool("Attack", true);
        yield return new WaitForSeconds(1f);
        anim.SetBool("Attack", false);
        yield return new WaitForSeconds(coolDown);
        canHit = true;
    }

    void DrawTrejectory()
    {
        Vector3 origin = launchPoint.position;
        Vector3 strartVelocity = shootingPower * launchPoint.up;
        lineRender.positionCount = linePoints;
        float time = 0;
        for (int i = 0; i < linePoints; i++)
        {
            var x = (strartVelocity.x * time) + (Physics.gravity.x / 2 * time * time);
            var y = (strartVelocity.y * time) + (Physics.gravity.y / 2 * time * time);
            Vector3 point = new Vector3(x, y, 0);
            lineRender.SetPosition(i, origin + point);
            time += timeItervalPoints;
        }
    }
    IEnumerator LongHit()
    {
        canHit = false;

        launchPoint.rotation = Quaternion.Euler(0, 0, Random.Range(20, 61));
        lineRender.enabled = true;
        DrawTrejectory();

        GameObject boneBullet = Instantiate(bone, transform.position, Quaternion.identity);
        var rb = boneBullet.GetComponent<Rigidbody2D>();
        boneBullet.GetComponent<BoneBullet>().dammage = dammage;
        if (rb != null) rb.velocity = shootingPower * launchPoint.up;

        lineRender.enabled = false;

        yield return new WaitForSeconds(coolDown);

        canHit = true;
    }

    IEnumerator GetHurt()
    {
        anim.SetBool("Hurt", true);
        yield return new WaitForSeconds(.5f);
        anim.SetBool("Hurt", false);
    }

}
