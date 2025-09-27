using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] float health;
    GameManager gameManager;


    private void Start()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.GetComponent<FamichikiBullet>() != null)
        {
           StartCoroutine(GetHurt(collision.gameObject.GetComponent<FamichikiBullet>().hitDammage));
        }
        if (collision.gameObject.GetComponent<Banana>() != null)
        {
            StartCoroutine(GetHurt(collision.gameObject.GetComponent<Banana>().hitDammage));
        }
        if (collision.gameObject.GetComponent<HeartBullet>() != null)
        {
            StartCoroutine(GetHurt(collision.gameObject.GetComponent<HeartBullet>().hitDammage));
        }
        if (collision.gameObject.GetComponent<SnakeBullet>() != null)
        {
            StartCoroutine(GetPoisoned(collision.gameObject.GetComponent<SnakeBullet>().hitDammage, collision.gameObject.GetComponent<SnakeBullet>().poisonDammage));
        }

    }
    private void CheckHealth()
    {
        if(health <= 0)
        {
            gameManager.enemyKilledInWave++;
            Destroy(gameObject);
        }
    }

    
    IEnumerator GetHurt(float dammage)
    {
        health -= dammage;
        CheckHealth();
        //Eðer caný bittiyse olüm efekti
        //Dammage animasyonu baþla
        yield return new WaitForSeconds(.5f);
        //Dammage animasyonu bit
    }

    IEnumerator GetPoisoned(float dammage, float poisonDammage)
    {
        health -= dammage;
        CheckHealth();
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(.45f);
            health -= poisonDammage;
            CheckHealth();
        }
    }
}
