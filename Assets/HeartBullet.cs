using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartBullet : MonoBehaviour
{
    public Transform enemy;
    [SerializeField] float shootPower;
    public float hitDammage;

    private void Update()
    {
        if (enemy == null) Destroy(this.gameObject);
        else transform.position = Vector2.MoveTowards(transform.position, enemy.position, shootPower * Time.deltaTime);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
