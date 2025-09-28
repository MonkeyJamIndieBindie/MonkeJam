using UnityEngine;
using DG.Tweening;

public class BuyTowerBuddy : MonoBehaviour
{
    [SerializeField] GameObject towerBuddy;
    [SerializeField] float buddyCost;
    [SerializeField] Transform nextKale;
    [SerializeField] string characterId;

    GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void BuyBuddy()
    {
        if (gameManager.money < buddyCost) return;

        // 1) Satýn alýrken UI'yi StartTheWave gibi kapat (paneller yukarý, dim kapanýr)
        var waveUI = FindObjectOfType<WaveUIAnimator>();
        if (waveUI != null) waveUI.PlayStartWave();

        // 2) Prefab'ý oluþtur + cartoony pop animasyonu
        Vector3 targetPos = nextKale.position;
        GameObject yeniKale = Instantiate(towerBuddy, targetPos, Quaternion.identity);
        PlaySpawnAnimation(yeniKale, targetPos);
        SFXManager.Instance?.Play("nextKale");

        // 3) Sorting order
        float y = nextKale.position.y;
        var sr = yeniKale.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (Mathf.Approximately(y, -2.4f)) sr.sortingOrder = -2;
            if (Mathf.Approximately(y, 0.2f)) sr.sortingOrder = -3;
            if (Mathf.Approximately(y, 2.8f)) sr.sortingOrder = -4;
            if (Mathf.Approximately(y, 5.4f)) sr.sortingOrder = -5;
        }

        // 4) Game state
        gameManager.buddyShooting.Add(yeniKale.GetComponentInChildren<Shooting>());
        nextKale.position = new Vector2(nextKale.position.x, nextKale.position.y + 2.6f);
        gameManager.money -= buddyCost;
        gameManager.UpdateMoney();
        if (NavbarUI.Instance != null) NavbarUI.Instance.AddCharacterById(characterId);

        // 5) Kamera: hedefe zoom  geri dönünce panelleri/dim'i geri getir (baþlýksýz)
        var camZoom = Camera.main != null ? Camera.main.GetComponent<CameraShakeZoom>() : null;
        if (camZoom != null)
        {
            camZoom.FocusOnTarget(yeniKale.transform, () =>
            {
                // EndWave gibi geri gelsin ama baþlýk olmasýn  Intro sekansý
                if (waveUI != null) waveUI.PlayIntro();
            });
        }
        else
        {
            // Kamera script yoksa yine de UI’yi geri getir
            if (waveUI != null) waveUI.PlayIntro();
        }

        gameObject.SetActive(false);
    }

    void PlaySpawnAnimation(GameObject go, Vector3 targetPos)
    {
        var t = go.transform;
        t.localScale = Vector3.one * 0.4f;
        t.position = targetPos + Vector3.down * 0.6f;
        t.rotation = Quaternion.Euler(0, 0, Random.Range(-6f, 6f));

        Sequence seq = DOTween.Sequence();
        seq.Append(t.DOMoveY(targetPos.y, 0.22f).SetEase(Ease.OutCubic));
        seq.Join(t.DOScale(new Vector3(1.25f, 0.8f, 1f), 0.18f).SetEase(Ease.OutCubic));
        seq.Append(t.DOMoveY(targetPos.y + 0.15f, 0.10f).SetEase(Ease.OutSine));
        seq.Join(t.DOScale(new Vector3(0.9f, 1.15f, 1f), 0.10f).SetEase(Ease.OutSine));
        seq.Append(t.DOScale(Vector3.one, 0.10f).SetEase(Ease.InSine));
        seq.Join(t.DOPunchScale(Vector3.one * 0.06f, 0.12f, 10, 0.9f));
        seq.Join(t.DOPunchRotation(new Vector3(0, 0, Random.Range(-5f, 5f)), 0.12f, 8, 0.9f));
        seq.OnComplete(() =>
        {
            t.position = targetPos;
            t.localScale = Vector3.one;
            t.rotation = Quaternion.identity;
        });
    }
}
