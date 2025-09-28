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
    int enemyLoot; // bu d��mandan ka� para ��kacak (toplam)

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        anim = GetComponent<Animator>();
        enemyLoot = Random.Range(1, 6); // ister Inspector�a da alabilirsin
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

        // Dalgay� say ama paray� burada EKLEME; coin�ler UI�ya ula��nca eklenecek
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
