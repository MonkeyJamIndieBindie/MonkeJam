using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] enemy;
    public bool makeEnemy;
    [SerializeField] GameManager gameManager;
    int madeEnemy;
    /*private IEnumerator Start()
    {
        yield return new WaitForSeconds(1.5f);
        makeEnemy = true;
    }*/

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
        Instantiate(enemy[Random.Range(0,enemy.Length)], transform.position, Quaternion.identity);
        madeEnemy++;
        yield return new WaitForSeconds(5f);
        makeEnemy = true;
    }
}
