using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Shooting : MonoBehaviour
{
    [SerializeField] Transform enemy;
    [SerializeField] GameObject bullet;
    [SerializeField] PlayerType type;
    [SerializeField] float shootingPower;
    [SerializeField] float coolDown;

    public bool canShoot;
    [SerializeField] Transform launchPoint;

    [Header("Trajectory Display")]
    public LineRenderer lineRender;
    int linePoints = 175;
    float timeItervalPoints = .01f;

    // --- Cartoon FX (genel) ---
    [Header("Cartoon FX")]
    [SerializeField] Transform shooterRoot;       // karakter ana transformu (boþsa this.transform)
    [SerializeField] float preSquashScaleX = 1.12f;
    [SerializeField] float preSquashScaleY = 0.86f;
    [SerializeField] float preBackOffset = 0.18f;  // local eksende sol tarafa çekilme
    [SerializeField] float preTime = 0.12f;

    [SerializeField] float recoilPunchRot = 7f;     // z (derece)
    [SerializeField] float recoilPunchPos = 0.12f;  // geri tekme mesafesi
    [SerializeField] float recoilTime = 0.14f;

    // Drift'i engellemek için "home" local deðerleri
    Vector3 _homeLocalPos;
    Vector3 _homeLocalScale;
    Quaternion _homeLocalRot;

    GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
        if (shooterRoot == null) shooterRoot = this.transform;

        _homeLocalPos = shooterRoot.localPosition;
        _homeLocalScale = shooterRoot.localScale;
        _homeLocalRot = shooterRoot.localRotation;
    }

    public enum PlayerType
    {
        Connor,
        Ironmouse,
        Chris,
        Joey,
        Garnt
    }

    private void Update()
    {
        if (!canShoot || !gameManager.startGame) return;

        switch (type)
        {
            case PlayerType.Connor: StartCoroutine(ConnorShoot()); break;
            case PlayerType.Ironmouse: StartCoroutine(ShootIronmouse()); break;
            case PlayerType.Chris: StartCoroutine(ChrisShoot()); break;
            case PlayerType.Joey: StartCoroutine(JoeyShoot()); break;
            case PlayerType.Garnt: StartCoroutine(ShootGarnt()); break;
        }
    }

    // -------------------------
    // Yardýmcýlar
    // -------------------------

    void ResetToHome()
    {
        // Bu objeye ait tüm tweenleri kes ve local "home" deðerine sabitle
        shooterRoot.DOKill();
        shooterRoot.localPosition = _homeLocalPos;
        shooterRoot.localScale = _homeLocalScale;
        shooterRoot.localRotation = _homeLocalRot;
    }

    void FindClosestEnemy()
    {
        float distanceToClosestEnemy = Mathf.Infinity;
        EnemyBase[] allEnemies = GameObject.FindObjectsOfType<EnemyBase>();

        foreach (EnemyBase currentEnemy in allEnemies)
        {
            float distanceToEnemy = (currentEnemy.transform.position - this.transform.position).sqrMagnitude;
            if (distanceToEnemy < distanceToClosestEnemy)
            {
                distanceToClosestEnemy = distanceToEnemy;
                enemy = currentEnemy.transform;
            }
        }
    }

    void DrawTrejectory()
    {
        Vector3 origin = launchPoint.position;
        Vector3 strartVelocity = shootingPower * launchPoint.right;
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

    IEnumerator DoPrefireAnticipation()
    {
        // Her seferinde temiz baþlangýç
        ResetToHome();

        Vector3 targetLocal = _homeLocalPos + Vector3.left * preBackOffset;

        Sequence preSeq = DOTween.Sequence();
        preSeq.Join(shooterRoot.DOScale(new Vector3(preSquashScaleX, preSquashScaleY, 1f), preTime).SetEase(Ease.OutCubic));
        preSeq.Join(shooterRoot.DOLocalMove(targetLocal, preTime).SetEase(Ease.OutCubic));
        yield return preSeq.WaitForCompletion();

        // Çok kýsa yerleþme  home’a doðru
        shooterRoot.DOScale(_homeLocalScale, 0.10f).SetEase(Ease.InSine);
        shooterRoot.DOLocalMove(_homeLocalPos, 0.10f).SetEase(Ease.InSine);
    }

    void DoRecoilFX()
    {
        // Punch world position'da çalýþýr ama bitince baþlangýca döner.
        // Yine de drift olmasýn diye coroutine sonunda ResetToHome çaðrýyoruz.
        Sequence recoil = DOTween.Sequence();
        recoil.Join(shooterRoot.DOPunchPosition(Vector3.left * recoilPunchPos, recoilTime, 10, 0.9f));
        recoil.Join(shooterRoot.DOPunchRotation(new Vector3(0, 0, -recoilPunchRot), recoilTime, 8, 0.9f));
    }

    // -------------------------
    // Connor
    // -------------------------
    IEnumerator ConnorShoot()
    {
        canShoot = false;

        yield return DoPrefireAnticipation();

        GameObject banana = Instantiate(bullet, transform.position, Quaternion.identity);
        var b = banana.GetComponent<Banana>();
        if (b != null)
        {
            b.enemy = enemy;
            b.connor = this.transform;
        }

        DoRecoilFX();

        yield return new WaitForSeconds(coolDown);

        // Drift önleme
        ResetToHome();

        canShoot = true;
    }

    // -------------------------
    // Chris
    // -------------------------
    IEnumerator ChrisShoot()
    {
        canShoot = false;

        FindClosestEnemy();
        launchPoint.rotation = Quaternion.Euler(0, 0, Random.Range(20, 71));
        lineRender.enabled = true;
        DrawTrejectory();

        yield return DoPrefireAnticipation();

        GameObject famichicki = Instantiate(bullet, transform.position, Quaternion.identity);
        var rb = famichicki.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = shootingPower * launchPoint.right;

        DoRecoilFX();

        lineRender.enabled = false;

        yield return new WaitForSeconds(coolDown);

        ResetToHome();
        canShoot = true;
    }

    // -------------------------
    // Joey
    // -------------------------
    IEnumerator JoeyShoot()
    {
        canShoot = false;

        launchPoint.rotation = Quaternion.Euler(0, 0, Random.Range(20, 71));
        lineRender.enabled = true;
        DrawTrejectory();

        yield return DoPrefireAnticipation();

        GameObject famichicki = Instantiate(bullet, transform.position, Quaternion.identity);
        var rb = famichicki.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = shootingPower * launchPoint.right;

        DoRecoilFX();

        lineRender.enabled = false;

        yield return new WaitForSeconds(coolDown);

        ResetToHome();
        canShoot = true;
    }

    // -------------------------
    // Ironmouse
    // -------------------------
    IEnumerator ShootIronmouse()
    {
        canShoot = false;

        FindClosestEnemy();

        if (enemy != null && Vector2.Distance(transform.position, enemy.position) < 25f)
        {
            yield return DoPrefireAnticipation();

            GameObject heartBullet = Instantiate(bullet, transform.position, Quaternion.identity);
            var hb = heartBullet.GetComponent<HeartBullet>();
            if (hb != null) hb.enemy = enemy;

            DoRecoilFX();

            yield return new WaitForSeconds(coolDown);
        }

        ResetToHome();
        canShoot = true;
    }

    // -------------------------
    // Garnt
    // -------------------------
    IEnumerator ShootGarnt()
    {
        canShoot = false;

        yield return DoPrefireAnticipation();

        Instantiate(bullet, new Vector2(Random.Range(-10, 7), 0), Quaternion.identity);

        DoRecoilFX();

        yield return new WaitForSeconds(coolDown);

        ResetToHome();
        canShoot = true;
    }
}
