using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(LineRenderer))]
public class PoisonAreaVFX : MonoBehaviour
{
    [Header("Circle")]
    [SerializeField] int segments = 64;
    [SerializeField] float baseWidth = 0.06f;
    [SerializeField] Color baseColor = new Color(0.75f, 0.25f, 1f, 0.55f); // mor + yarý saydam

    [Header("Pulse")]
    [SerializeField] float pulseWidthDelta = 0.035f;
    [SerializeField] float pulseAlphaDelta = 0.25f;
    [SerializeField] float pulseTime = 0.8f;
    [SerializeField] Ease pulseEase = Ease.InOutSine;

    LineRenderer lr;
    float radius;
    float duration;
    Tween pulseTween;

    public void Init(float radius, float duration)
    {
        this.radius = radius;
        this.duration = duration;
        BuildCircle();
        PlayPulse();
        // otomatik temizle
        Destroy(gameObject, duration + 0.1f);
    }

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        // temel line ayarlarý (istersen Inspector’dan override edebilirsin)
        lr.loop = true;
        lr.useWorldSpace = true;
        lr.textureMode = LineTextureMode.Stretch;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 4;
        lr.widthMultiplier = baseWidth;
        lr.startColor = baseColor;
        lr.endColor = baseColor;
    }

    void BuildCircle()
    {
        lr.positionCount = segments;
        float step = Mathf.PI * 2f / (segments);
        Vector3 center = transform.position;

        for (int i = 0; i < segments; i++)
        {
            float a = step * i;
            Vector3 p = new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
            lr.SetPosition(i, center + p);
        }
    }

    void PlayPulse()
    {
        // 0..1..0 arasý bir parametreyi yoyo loop ile döndürüp width ve alpha’yý “pulse” ettiriyoruz.
        float t = 0f;
        pulseTween?.Kill();
        pulseTween = DOTween.To(() => t, v => {
            t = v;
            // width
            lr.widthMultiplier = baseWidth + pulseWidthDelta * t;
            // color alpha
            Color c = baseColor; c.a = Mathf.Clamp01(baseColor.a + pulseAlphaDelta * t);
            lr.startColor = c; lr.endColor = c;
        }, 1f, pulseTime).SetEase(pulseEase).SetLoops(-1, LoopType.Yoyo);
    }

    void OnDestroy()
    {
        pulseTween?.Kill();
    }
}
