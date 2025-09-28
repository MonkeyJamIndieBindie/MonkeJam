using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeBullet : MonoBehaviour
{
    public float hitDammage;
    public float poisonDammage;
    bool poison = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(poison == true)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                StartCoroutine(Poison());
            }
            if (collision.gameObject.CompareTag("Zemin"))
            {
                StartCoroutine(Poison());
            }
        }
    }

    IEnumerator Poison()
    {
        poison = false;
        transform.GetComponent<Rigidbody2D>().gravityScale = 0;
        transform.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        transform.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
        GetComponent<BoxCollider2D>().size = new Vector2(3, .5f);
        yield return new WaitForSeconds(.5f);
        Destroy(gameObject);
    }
}
