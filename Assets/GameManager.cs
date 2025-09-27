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
            if(buddyShooting[i] != null) buddyShooting[i].canShoot = false;
        }
        enemySpawner.makeEnemy = false;
        closeBetween.SetActive(true);
        startGame = false;
    }

    public void CheckEndWave()
    {
        if(maxEnemyForWave == enemyKilledInWave && startGame == true)
        {
            EndWave();
        }
    }
}
