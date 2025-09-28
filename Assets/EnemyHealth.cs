using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] float health;
    [SerializeField] GameObject coinWorldPrefab; // SpriteRenderer + Rigidbody2D + CoinPickup olmalý
    [SerializeField] int minCoins = 2;
    [SerializeField] int maxCoins = 4;

    GameManager gameManager;
    Animator anim;
    int enemyLoot; // bu düþmandan kaç para çýkacak (toplam)

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        anim = GetComponent<Animator>();
        enemyLoot = Random.Range(1, 6); // ister Inspector’a da alabilirsin
        health *= gameManager.levelHardnes[gameManager.levelCount].x;
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        var fami = c.GetComponent<FamichikiBullet>();
        if (fami) { StartCoroutine(GetHurt(fami.hitDammage)); return; }

        var banana = c.GetComponent<Banana>();
        if (banana) { StartCoroutine(GetHurt(banana.hitDammage)); return; }

        var heart = c.GetComponent<HeartBullet>();
        if (heart) { StartCoroutine(GetHurt(heart.hitDammage)); return; }

        var snake = c.GetComponent<SnakeBullet>();
        if (snake) { StartCoroutine(GetPoisoned(snake.hitDammage, snake.poisonDammage)); return; }
    }

    void CheckHealth()
    {
        if (health > 0) return;
        StartCoroutine(DieAndSpawnCoins());
    }

    IEnumerator DieAndSpawnCoins()
    {
        // Ýsteðe baðlý ölüm animasyonu
        // if (anim) anim.SetTrigger("Die");

        // Kaç coin düþecek?
        int coinCount = Mathf.Clamp(Random.Range(minCoins, maxCoins + 1), 1, 50);

        // Toplam loot’u coin’lere daðýt (mümkünse eþit)
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
                Debug.LogWarning("coinWorldPrefab atanmadý.", this);
            }
        }

        // Dalgayý say ama parayý burada EKLEME; coin’ler UI’ya ulaþýnca eklenecek
        gameManager.enemyKilledInWave++;
        gameManager.CheckEndWave();

        Destroy(gameObject);
        yield return null;
    }

    IEnumerator GetHurt(float dmg)
    {
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
