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

    public void StartTheWave()
    {
        for (int i = 0; i < buddyShooting.Count; i++)
        {
            if (buddyShooting[i] != null) buddyShooting[i].canShoot = true;
            else break;
        }
        enemySpawner.makeEnemy = true;
        closeBetween.SetActive(false);
        startGame = true;
    }

    void EndWave()
    {
        StopAllCoroutines();
        Debug.Log("Wave Durdu");
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
