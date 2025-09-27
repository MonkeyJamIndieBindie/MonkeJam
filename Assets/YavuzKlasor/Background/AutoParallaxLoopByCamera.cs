using UnityEngine;

public class AutoParallaxLoopByCamera : MonoBehaviour
{
    public float speed = 1f;
    public Camera cam;
    public float extraOffset = 0f; // boþluk ayarý

    Transform[] tiles;
    SpriteRenderer[] srs;
    float camHalfWidth;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        tiles = new Transform[transform.childCount];
        srs = new SpriteRenderer[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = transform.GetChild(i);
            srs[i] = tiles[i].GetComponent<SpriteRenderer>();
        }
        if (cam.orthographic) camHalfWidth = cam.orthographicSize * cam.aspect;
        else camHalfWidth = 10f;
    }

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        float camLeft = cam.transform.position.x - camHalfWidth;

        for (int i = 0; i < tiles.Length; i++)
        {
            var r = srs[i].bounds;
            if (r.max.x < camLeft)
            {
                float rightmost = float.NegativeInfinity;
                for (int k = 0; k < tiles.Length; k++)
                {
                    if (k == i) continue;
                    float rx = srs[k].bounds.max.x;
                    if (rx > rightmost) rightmost = rx;
                }
                float w = r.size.x;
                var p = tiles[i].position;
                p.x = rightmost + w + extraOffset;
                tiles[i].position = p;
            }
        }
    }
}
