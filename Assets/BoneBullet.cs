using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneBullet : MonoBehaviour
{
    public float dammage;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Kale"))
        {
            GameManager manager = GameObject.FindObjectOfType<GameManager>();
            manager.towerHealth -= dammage;
            manager.UpdateHeath();
            Destroy(gameObject);
        }
    }
}
