using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeBullet : MonoBehaviour
{
    public float hitDammage;
    public float poisonDammage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //collision.gameObject.GetComponent<EnemyWalk>().enemySpeed -= 1;
            //collision.gameObject.GetComponent<EnemyWalk>().hit = true;
            Debug.Log("Enemy Poisoned");
            Destroy(gameObject);
        }
    }
}
