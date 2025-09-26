using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] enemy;
    bool makeEnemy;
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1.5f);
        makeEnemy = true;
    }

    private void Update()
    {
        if(makeEnemy == true)
        {
            StartCoroutine(SpawnEnemy());
        }
    }


    IEnumerator SpawnEnemy()
    {
        makeEnemy = false;
        Instantiate(enemy[0], transform.position, Quaternion.identity);
        yield return new WaitForSeconds(5f);
        makeEnemy = true;
    }
}
