using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public float money;
    [SerializeField] TextMeshProUGUI moneyText;
    public bool startGame;
    public List<Shooting> buddyShooting;
    [SerializeField] EnemySpawner enemySpawner;
    [SerializeField] GameObject closeBetween;
    [SerializeField] TextMeshProUGUI heathText;

    public int maxEnemyForWave;
    public int enemyKilledInWave;

    public float towerHealth;

    public Vector2[] levelHardnes;
    public int levelCount;

    [Header("Coin Settings")]
    [SerializeField] Canvas mainCanvas;
    [SerializeField] RectTransform moneyIconRect;
    [SerializeField] float coinWaitBeforeMove = 1f;
    [SerializeField] float coinMoveDuration = 0.6f;

    private void Start()
    {
        UpdateMoney();
        UpdateHeath();
    }

    public void UpdateHeath()
    {
        heathText.text = towerHealth.ToString("0");
    }

    public void UpdateMoney()
    {
        moneyText.text = money.ToString("0");
    }

    public void StartTheWave()
    {
        for (int i = 0; i < buddyShooting.Count; i++)
        {
            if (buddyShooting[i] != null) buddyShooting[i].canShoot = true;
            else break;
        }
        enemySpawner.makeEnemy = true;
        enemySpawner.madeEnemy = 0;
        enemyKilledInWave = 0;
        closeBetween.SetActive(false);
        startGame = true;
    }

    void EndWave()
    {
        StopAllCoroutines();
        for (int i = 0; i < buddyShooting.Count; i++)
        {
            if (buddyShooting[i] != null) buddyShooting[i].canShoot = false;
        }
        enemySpawner.makeEnemy = false;
        closeBetween.SetActive(true);
        levelCount++;
        startGame = false;
    }

    public void CheckEndWave()
    {
        if (maxEnemyForWave == enemyKilledInWave && startGame == true)
        {
            EndWave();
        }
    }

    // Bunlarý CoinPickup çaðýracak
    public RectTransform GetMoneyIconRect() => moneyIconRect;
    public Canvas GetMainCanvas() => mainCanvas;
    public float GetCoinWaitTime() => coinWaitBeforeMove;
    public float GetCoinMoveDuration() => coinMoveDuration;
}
