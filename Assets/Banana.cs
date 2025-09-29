using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

public class Banana : MonoBehaviour
{
    public Transform enemy;
    public Transform connor;
    private bool hit;
    public float hitDammage = 1f;

    [Header("Cartoon Flight")]
    [SerializeField] float outwardSpeed = 5f;
    [SerializeField] float returnSpeed = 5f;
    [SerializeField] float spinSpeedDegPerSec = 720f;
    [SerializeField] Vector3 wobblePunch = new Vector3(0.15f, -0.15f, 0f);
    [SerializeField] float wobbleTime = 0.22f;

    Tween spinT, wobbleT;


    private IEnumerator Start()
    {
        GetComponent<CircleCollider2D>().radius = .3f;
        yield return new WaitForSeconds(.5f);
        GetComponent<CircleCollider2D>().radius = 1;
    }
    void OnEnable()
    {
        spinT = transform
            .DORotate(new Vector3(0, 0, 360f), spinSpeedDegPerSec, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear).SetSpeedBased(true).SetLoops(-1, LoopType.Incremental);

        wobbleT = transform
            .DOPunchScale(wobblePunch, wobbleTime, 8, 0.9f)
            .SetLoops(-1, LoopType.Yoyo);
    }

    

    void Update()
    {
        if (!hit)
        {
            if (enemy == null) { Destroy(gameObject); return; }
            transform.position = Vector2.MoveTowards(transform.position, enemy.position, outwardSpeed * Time.deltaTime);
        }
        else
        {
            if (connor == null) { Destroy(gameObject); return; }
            transform.position = Vector2.MoveTowards(transform.position, connor.position, returnSpeed * Time.deltaTime);
            if ((transform.position - connor.position).sqrMagnitude < 0.0001f)
                Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("BananaTurn"))
        {
            hit = true;
        }
    }

    void OnDestroy()
    {
        spinT?.Kill();
        wobbleT?.Kill();
    }

    
}
