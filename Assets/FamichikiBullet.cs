using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FamichikiBullet : MonoBehaviour
{
    public float hitDammage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if(collision.gameObject.GetComponent<EnemyWalk>() != null)
            {
                collision.gameObject.GetComponent<EnemyWalk>().enemySpeed -= 1;
                collision.gameObject.GetComponent<EnemyWalk>().hit = true;
            }
            Destroy(gameObject);
        }
    }
}
