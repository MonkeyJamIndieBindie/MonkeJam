using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] float health;
    [SerializeField] GameObject coinWorldPrefab; // SpriteRenderer + Rigidbody2D + CoinPickup olmal�
    [SerializeField] int minCoins = 2;
    [SerializeField] int maxCoins = 4;

    GameManager gameManager;
    Animator anim;
    int enemyLoot;          // bu d��mandan ka� para ��kacak (toplam)
    bool isDying;           // �ift tetiklemeyi �nle

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        anim = GetComponent<Animator>();
        enemyLoot = Random.Range(1, 6); // istersen Inspector�a al
        health *= gameManager.levelHardnes[gameManager.levelCount].x;
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        if (isDying) return;

        var fami  = c.GetComponent<FamichikiBullet>();
        if (fami)  { StartCoroutine(GetHurt(fami.hitDammage)); return; }

        var banana = c.GetComponent<Banana>();
        if (banana){ StartCoroutine(GetHurt(banana.hitDammage)); return; }

        var heart = c.GetComponent<HeartBullet>();
        if (heart) { StartCoroutine(GetHurt(heart.hitDammage)); return; }

        var snake = c.GetComponent<SnakeBullet>();
        if (snake) { StartCoroutine(GetPoisoned(snake.hitDammage, snake.poisonDammage)); return; }
    }

    void CheckHealth()
    {
        if (health > 0f) return;
        // Normal �l�m: hem coin d�k hem dalgay� say
        ForceKillAndLoot(countWaveKill: true);
    }

    /// <summary>
    /// D��ar�dan (�r. EnemyWalk.Roll �arp��mas�) �a��r:
    /// countWaveKill=true  dalgay� burada sayar.
    /// countWaveKill=false  dalgay� d��ar�da say�yorsun (EnemyWalk�ta) ve burada SAYMA.
    /// </summary>
    public void ForceKillAndLoot(bool countWaveKill)
    {
        if (isDying) return;
        isDying = true;
        StartCoroutine(DieAndSpawnCoins(countWaveKill));
    }

    // Eski imzayla uyum istersen:
    IEnumerator DieAndSpawnCoins() => DieAndSpawnCoins(true);

    IEnumerator DieAndSpawnCoins(bool countWaveKill)
    {
        // �ste�e ba�l� �l�m animasyonu
        // if (anim) anim.SetTrigger("Die");

        // Ka� coin d��ecek?
        int coinCount = Mathf.Clamp(Random.Range(minCoins, maxCoins + 1), 1, 50);

        // Toplam loot�u coin�lere da��t (m�mk�nse e�it)
        int baseVal = Mathf.Max(1, enemyLoot / coinCount);
        int rem = Mathf.Max(0, enemyLoot - baseVal * coinCount);

        for (int i = 0; i < coinCount; i++)
        {
            int value = baseVal + (i < rem ? 1 : 0);

            if (coinWorldPrefab != null)
            {
                var coin = Instantiate(coinWorldPrefab, transform.position, Quaternion.identity);
                var cp = coin.GetComponent<CoinPickup>();
                if (cp != null) cp.Initialize(gameManager, value);
            }
            else
            {
                Debug.LogWarning("coinWorldPrefab atanmad�.", this);
            }
        }

        // Dalgay� burada say (iste�e ba�l�)
        if (countWaveKill)
        {
            gameManager.enemyKilledInWave++;
            gameManager.CheckEndWave();
        }

        Destroy(gameObject);
        yield return null;
    }

    IEnumerator GetHurt(float dmg)
    {
        if (isDying) yield break;

        health -= dmg;
        CheckHealth();
        if (anim)
        {
            anim.SetBool("Hurt", true);
            yield return new WaitForSeconds(0.5f);
            anim.SetBool("Hurt", false);
        }
    }

    IEnumerator GetPoisoned(float dmg, float poison)
    {
        yield return StartCoroutine(GetHurt(dmg));
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.45f);
            yield return StartCoroutine(GetHurt(poison));
        }
    }
}
