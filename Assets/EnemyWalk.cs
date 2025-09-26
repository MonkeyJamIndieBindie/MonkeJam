using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWalk : MonoBehaviour
{
    Transform connor;
    public float enemySpeed;

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
    }


    public IEnumerator GetFaster()
    {
        yield return new WaitForSeconds(1f);
        enemySpeed = 3;
    }
}
