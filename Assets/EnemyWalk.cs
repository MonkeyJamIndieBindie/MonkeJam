using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWalk : MonoBehaviour
{
    Transform connor;
    public float enemySpeed;
    public bool hit;

    Animator anim;

    private void Start()
    {
        connor = GameObject.FindGameObjectWithTag("Kale").transform;
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if(Vector2.Distance(connor.position,transform.position) > 1.2f)
        {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(connor.position.x, transform.position.y), enemySpeed * Time.deltaTime);
            anim.SetBool("Walk", true);
        }
        else
        {
            transform.position = this.transform.position;
            anim.SetBool("Walk", false);
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
