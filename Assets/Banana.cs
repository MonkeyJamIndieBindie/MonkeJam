using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Banana : MonoBehaviour
{
    public Transform enemy;
    public Transform connor;
    private bool hit;

    private void Update()
    {
        if(hit == false)
        {
            transform.position = Vector2.MoveTowards(transform.position, enemy.position, 5 * Time.deltaTime);
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, connor.position, 5 * Time.deltaTime);
            if (transform.position == connor.position)
            {
                Destroy(gameObject);
            }
            
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            hit = true;
        }
    }
}
