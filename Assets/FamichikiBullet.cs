using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FamichikiBullet : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<EnemyWalk>().enemySpeed -= 1;
            StartCoroutine(collision.gameObject.GetComponent<EnemyWalk>().GetFaster());
            Debug.Log("Enemy Slowed Down");
            Destroy(gameObject);
        }
    }
}
