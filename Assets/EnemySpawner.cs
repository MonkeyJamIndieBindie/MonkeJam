using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] enemy;
    public bool makeEnemy;
    [SerializeField] GameManager gameManager;
    public int madeEnemy;

    private void Update()
    {
        if(makeEnemy == true && madeEnemy < gameManager.maxEnemyForWave)
        {
            StartCoroutine(SpawnEnemy());
        }
    }


    IEnumerator SpawnEnemy()
    {
        makeEnemy = false;
        if (gameManager.levelCount < 2) Instantiate(enemy[0], transform.position, Quaternion.identity);
        if (gameManager.levelCount >= 2 && gameManager.levelCount < 4) Instantiate(enemy[Random.Range(0, 2)], transform.position, Quaternion.identity);
        if (gameManager.levelCount >= 4 && gameManager.levelCount < 6) Instantiate(enemy[Random.Range(0, 3)], transform.position, Quaternion.identity);
        if (gameManager.levelCount >= 6) Instantiate(enemy[Random.Range(0, enemy.Length)], transform.position, Quaternion.identity);
        madeEnemy++;
        yield return new WaitForSeconds(5f);
        makeEnemy = true;
    }
}
