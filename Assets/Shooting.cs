using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    [SerializeField] Transform enemy;
    [SerializeField] GameObject bullet;
    [SerializeField] PlayerType type;
    [SerializeField] float shootingPower;

    [SerializeField] bool canShoot;
    [SerializeField] Transform launchPoint;

    [Header("Trajectory Display")]
    public LineRenderer lineRender;
    int linePoints = 175;
    float timeItervalPoints = .01f;

    public enum PlayerType
    {
        Connor,
        Ironmouse,
        Chris,
        Joey,
        Garnt
    }

    private void Start()
    {
        //canShoot = true;
    }

    private void Update()
    {
        switch (type)
        {
            case PlayerType.Connor:
                if(canShoot == true)
                {
                    StartCoroutine(ConnorShoot());
                }
                break;
            case PlayerType.Ironmouse:
                if (canShoot == true)
                {
                    
                }
                break;
            case PlayerType.Chris:
                if (canShoot == true)
                {
                    StartCoroutine(ChrisShoot());
                }
                break;
            case PlayerType.Joey:
                if (canShoot == true)
                {
                    StartCoroutine(JoeyShoot());
                }
                break;
            case PlayerType.Garnt:
                if (canShoot == true)
                {
                    
                }
                break;
            default:
                break;
        }
    }


    void DrawTrejectory()
    {
        Vector3 origin = launchPoint.position;
        Vector3 strartVelocity = shootingPower * launchPoint.right;
        lineRender.positionCount = linePoints;
        float time = 0;
        for (int i = 0; i < linePoints; i++)
        {
            var x = (strartVelocity.x * time) + (Physics.gravity.x / 2 * time * time);
            var y = (strartVelocity.y * time) + (Physics.gravity.y / 2 * time * time);
            Vector3 point = new Vector3(x, y, 0);
            lineRender.SetPosition(i, origin + point);
            time += timeItervalPoints;
        }
    }

    IEnumerator ConnorShoot()
    {
        canShoot = false;
        GameObject banana = Instantiate(bullet, transform.position, Quaternion.identity) as GameObject;
        banana.GetComponent<Banana>().enemy = enemy;
        banana.GetComponent<Banana>().connor = this.transform;
        yield return new WaitForSeconds(1f);
        canShoot = true;
    }

    IEnumerator ChrisShoot()
    {
        canShoot = false;
        launchPoint.rotation = Quaternion.Euler(0,0,Random.Range(20, 71));
        DrawTrejectory();
        lineRender.enabled = true;
        yield return new WaitForSeconds(.75f);
        GameObject famichicki = Instantiate(bullet, transform.position, Quaternion.identity) as GameObject;
        famichicki.GetComponent<Rigidbody2D>().velocity = shootingPower * launchPoint.right;
        lineRender.enabled = false;
        yield return new WaitForSeconds(.75f);
        canShoot = true;
    }

    IEnumerator JoeyShoot()
    {
        canShoot = false;
        launchPoint.rotation = Quaternion.Euler(0, 0, Random.Range(20, 71));
        DrawTrejectory();
        lineRender.enabled = true;
        yield return new WaitForSeconds(.75f);
        GameObject famichicki = Instantiate(bullet, transform.position, Quaternion.identity) as GameObject;
        famichicki.GetComponent<Rigidbody2D>().velocity = shootingPower * launchPoint.right;
        lineRender.enabled = false;
        yield return new WaitForSeconds(.75f);
        canShoot = true;
    }
}
