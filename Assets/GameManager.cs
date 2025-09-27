using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public float money;
    [SerializeField] TextMeshProUGUI moneyText;
    public bool stratGame;
    [SerializeField] Shooting[] buddyShooting;
    [SerializeField] EnemySpawner enemySpawner;
    [SerializeField] GameObject closeBetween;

    public int maxEnemyForWave;
    public int enemyKilledInWave;

    private void Start()
    {
        UpdateMoney();
    }

    public void UpdateMoney()
    {
        moneyText.text = money.ToString("0");
    }

    public void StartTheWve()
    {
        for (int i = 0; i < buddyShooting.Length; i++)
        {
            buddyShooting[i].canShoot = true;
        }
        enemySpawner.makeEnemy = true;
        closeBetween.SetActive(false);
    }

    void EndWave()
    {
        StopAllCoroutines();
        for (int i = 0; i < buddyShooting.Length; i++)
        {
            buddyShooting[i].canShoot = false;
        }
        enemySpawner.makeEnemy = false;
        closeBetween.SetActive(true);
    }

    private void Update()
    {
        if(maxEnemyForWave == enemyKilledInWave)
        {
            EndWave();
        }
    }
}
