// CameraShakeZoom.cs
using UnityEngine;
using DG.Tweening;
using System;

public class CameraShakeZoom : MonoBehaviour
{
    [Header("Zoom/Focus")]
    [SerializeField] float zoomInSize = 3.5f;
    [SerializeField] float zoomDuration = 0.35f;
    [SerializeField] float holdDuration = 0.25f;
    [SerializeField] float returnDuration = 0.5f;
    [SerializeField, Range(0f, 1f)] float moveStrength = 1.0f;

    [Header("Start Wave Shake")]
    [SerializeField] float startWaveShakeDuration = 0.35f;
    [SerializeField] Vector2 startWaveShakeStrength = new Vector2(0.15f, 0.1f);
    [SerializeField] int startWaveVibrato = 15;
    [SerializeField] float startWaveRandomness = 85f;
    [SerializeField] float startWaveRotStrength = 1.2f; // z deg

    [Header("Ambient Drift (wave boyunca)")]
    [SerializeField] float ambientZoomInDelta = 0.3f;       // hafif yakýnlaþma miktarý
    [SerializeField] float ambientMoveRadius = 0.15f;       // px deðil; world units (ortografikte)
    [SerializeField] Vector2 ambientStepDuration = new Vector2(0.6f, 1.1f); // her adým süresi
    [SerializeField] float ambientRotMax = 0.4f;            // derece (çok küçük)
    [SerializeField] Ease ambientEase = Ease.InOutSine;

    Camera cam;
    float originalSize;
    Vector3 originalPos;

    Sequence ambientSeq;
    Tween ambientZoomTween;

    void Awake()
    {
        cam = Camera.main;
        if (cam != null)
        {
            originalSize = cam.orthographicSize;
            originalPos = cam.transform.position;
        }
    }

    // --- Odaklan-geri dön (buy anýnda vs.)
    public void FocusOnTarget(Transform target, Action onReturnComplete = null)
    {
        if (cam == null || target == null) return;

        DOTween.Kill(cam.transform);
        DOTween.Kill(cam);

        Vector3 targetPos = new Vector3(target.position.x, target.position.y, originalPos.z);

        Sequence seq = DOTween.Sequence();
        seq.Append(cam.transform.DOMove(Vector3.Lerp(originalPos, targetPos, moveStrength), zoomDuration).SetEase(Ease.OutCubic));
        seq.Join(cam.DOOrthoSize(zoomInSize, zoomDuration).SetEase(Ease.OutCubic));
        seq.AppendInterval(holdDuration);
        seq.Append(cam.transform.DOMove(originalPos, returnDuration).SetEase(Ease.InOutCubic));
        seq.Join(cam.DOOrthoSize(originalSize, returnDuration).SetEase(Ease.InOutCubic));
        seq.OnComplete(() => onReturnComplete?.Invoke());
    }

    // --- StartTheWave anýndaki kýsa shake
    public void StartWaveShake()
    {
        if (cam == null) return;

        var tr = cam.transform;
        var basePos = tr.position;
        var baseRot = tr.rotation;

        tr.DOKill();

        Sequence s = DOTween.Sequence();
        s.Join(tr.DOShakePosition(
            startWaveShakeDuration,
            new Vector3(startWaveShakeStrength.x, startWaveShakeStrength.y, 0f),
            startWaveVibrato,
            startWaveRandomness,
            fadeOut: true,
            snapping: false
        ));
        s.Join(tr.DOShakeRotation(
            startWaveShakeDuration,
            new Vector3(0f, 0f, startWaveRotStrength),
            startWaveVibrato,
            startWaveRandomness,
            fadeOut: true
        ));
        s.OnComplete(() =>
        {
            tr.position = new Vector3(basePos.x, basePos.y, basePos.z);
            tr.rotation = baseRot;
        });
    }

    // --- Ambient drift: hafif zoom-in + küçük, sürekli rastgele hareket
    public void BeginBattleAmbient()
    {
        if (cam == null) return;

        EndBattleAmbient(); // varsa eskiyi kapat

        // hafif yakýnlaþ
        ambientZoomTween = cam.DOOrthoSize(originalSize - ambientZoomInDelta, 0.35f).SetEase(Ease.OutCubic);

        var tr = cam.transform;
        Vector3 center = originalPos; // etrafýnda gezineceðimiz merkez

        ambientSeq = DOTween.Sequence().SetUpdate(false).SetAutoKill(false);

        // Sonsuz döngü: küçük rastgele hedeflere doðru yumuþak hareket + minik rotasyon
        ambientSeq.AppendCallback(() =>
        {
            Vector2 rnd = UnityEngine.Random.insideUnitCircle * ambientMoveRadius;
            float dur = UnityEngine.Random.Range(ambientStepDuration.x, ambientStepDuration.y);

            Vector3 targetPos = new Vector3(center.x + rnd.x, center.y + rnd.y, originalPos.z);
            float targetRotZ = UnityEngine.Random.Range(-ambientRotMax, ambientRotMax);

            tr.DOMove(targetPos, dur).SetEase(ambientEase).SetId("ambientMove");
            tr.DORotate(new Vector3(0, 0, targetRotZ), dur).SetEase(ambientEase).SetId("ambientRot");
        });

        // Her adým bitince bir sonraki rastgele adýma geç
        ambientSeq.AppendInterval(ambientStepDuration.y);
        ambientSeq.SetLoops(-1, LoopType.Restart);
    }

    public void EndBattleAmbient()
    {
        // döngüyü kapat ve kamerayý eski haline döndür
        if (ambientSeq != null && ambientSeq.IsActive()) ambientSeq.Kill();
        DOTween.Kill("ambientMove");
        DOTween.Kill("ambientRot");

        if (cam == null) return;

        var tr = cam.transform;
        tr.DOKill();

        cam.DOOrthoSize(originalSize, 0.3f).SetEase(Ease.InOutSine);
        tr.DOMove(originalPos, 0.3f).SetEase(Ease.InOutSine);
        tr.DORotate(Vector3.zero, 0.25f).SetEase(Ease.InOutSine);
    }
}
