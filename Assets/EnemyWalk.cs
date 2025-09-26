using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWalk : MonoBehaviour
{
    Transform connor;
    public float enemySpeed;
    public bool hit;

    private void Start()
    {
        connor = GameObject.FindObjectOfType<Shooting>().transform;
    }

    private void Update()
    {
        if(Vector2.Distance(connor.position,transform.position) > 1.2f)
        {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(connor.position.x, transform.position.y), enemySpeed * Time.deltaTime);
        }
        if(hit == true)
        {
            StartCoroutine(GetFaster());
        }
    }


    public IEnumerator GetFaster()
    {
        hit = false;
        yield return new WaitForSeconds(1f);
        enemySpeed = 3;
    }
}
