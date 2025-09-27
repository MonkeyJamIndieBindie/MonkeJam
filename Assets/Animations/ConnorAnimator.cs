using UnityEngine;

public class ConnorAnimator : MonoBehaviour
{
    public Animator animator;
    public Shooting shooting;

    bool prevCanShoot = true;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!shooting) shooting = GetComponent<Shooting>();
        if (shooting) prevCanShoot = shooting.canShoot;
    }

    void Update()
    {
        if (shooting)
        {
            if (prevCanShoot && !shooting.canShoot)
            {
                animator.SetTrigger("Shoot");
            }
            prevCanShoot = shooting.canShoot;
        }
    }

    public void OnDie()
    {
        animator.SetBool("Dead", true);
    }
}
