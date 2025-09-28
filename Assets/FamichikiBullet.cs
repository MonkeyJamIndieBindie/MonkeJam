using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FamichikiBullet : MonoBehaviour
{
    public float hitDammage;
    bool canGetBig = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if(canGetBig == true) StartCoroutine(GetBig());
            if(collision.gameObject.GetComponent<EnemyWalk>() != null)
            {
                collision.gameObject.GetComponent<EnemyWalk>().enemySpeed -= 1;
                collision.gameObject.GetComponent<EnemyWalk>().hit = true;
            }
        }
        if (collision.gameObject.CompareTag("Zemin"))
        {
            if (canGetBig == true) StartCoroutine(GetBig());
        }
    }

    IEnumerator GetBig()
    {
        canGetBig = false;
        transform.GetComponent<Rigidbody2D>().gravityScale = 0;
        transform.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        GetComponent<CircleCollider2D>().radius = 2;
        transform.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
        yield return new WaitForSeconds(.5f);
        Destroy(gameObject);
    }

}
