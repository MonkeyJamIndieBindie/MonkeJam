using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWalk : MonoBehaviour
{
    Transform connor;
    public float enemySpeed;
    public bool hit;

    Animator anim;

    public EnemyType enemyType;

    bool canHit = true;
    public float coolDown;
    [SerializeField] float dammage;

    GameManager manager;

    public enum EnemyType
    {
        Slow,
        Fast,
        Roll,
        Far
    }

    private void Start()
    {
        connor = GameObject.FindGameObjectWithTag("Kale").transform;
        anim = GetComponent<Animator>();
        manager = GameObject.FindObjectOfType<GameManager>();
        dammage *= manager.levelHardnes[manager.levelCount].y;
    }

    private void Update()
    {
        if(enemyType == EnemyType.Roll)
        {
             transform.position = Vector2.MoveTowards(transform.position, new Vector2(connor.position.x, transform.position.y), enemySpeed * Time.deltaTime);
        }
        else if(enemyType == EnemyType.Far)
        {
            if (Vector2.Distance(connor.position, transform.position) > 5f)
            {
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(connor.position.x, transform.position.y), enemySpeed * Time.deltaTime);
                anim.SetBool("Walk", true);
            }
            else
            {
                transform.position = this.transform.position;
                if(canHit == true)
                {
                    //uzakçý attack
                }
            }
        }
        else
        {
            if (Vector2.Distance(connor.position, transform.position) > 1.2f)
            {
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(connor.position.x, transform.position.y), enemySpeed * Time.deltaTime);
                anim.SetBool("Walk", true);
            }
            else
            {
                transform.position = this.transform.position;
                if (canHit == true)
                {
                    if (enemyType == EnemyType.Slow) StartCoroutine(HammerAttack());
                    if (enemyType == EnemyType.Fast) StartCoroutine(SwordAttack());
                }
                anim.SetBool("Walk", false);
            }
        }
        if (hit == true)
        {
            StartCoroutine(GetFaster());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Kale"))
        {
            if(enemyType == EnemyType.Roll)
            {
                HurtTower();
                manager.enemyKilledInWave++;
                manager.CheckEndWave();
                Destroy(gameObject);
            }
        }
    }


    void HurtTower()
    {
        manager.towerHealth -= dammage;
        manager.UpdateHeath();
    }

    public IEnumerator GetFaster()
    {
        hit = false;
        yield return new WaitForSeconds(1f);
        enemySpeed = 3;
    }

    IEnumerator HammerAttack()
    {
        canHit = false;
        anim.SetBool("Attack", true);
        yield return new WaitForSeconds(1.5f);
        anim.SetBool("Attack", false);
        yield return new WaitForSeconds(coolDown);
        canHit = true;
    }
    IEnumerator SwordAttack()
    {
        canHit = false;
        anim.SetBool("Attack", true);
        yield return new WaitForSeconds(1.5f);
        anim.SetBool("Attack", false);
        yield return new WaitForSeconds(coolDown);
        canHit = true;
    }

    IEnumerator GetHurt()
    {
        anim.SetBool("Hurt", true);
        yield return new WaitForSeconds(.5f);
        anim.SetBool("Hurt", false);
    }
}
