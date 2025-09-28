using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public float money;
    [SerializeField] TextMeshProUGUI moneyText;
    public bool startGame;
    public List<Shooting> buddyShooting;
    [SerializeField] EnemySpawner enemySpawner;
    [SerializeField] GameObject closeBetween;          // WaveUIAnimator bu objede
    [SerializeField] TextMeshProUGUI heathText;

    public int maxEnemyForWave;
    public int enemyKilledInWave;

    public float towerHealth;

    public Vector2[] levelHardnes;
    public int levelCount;

    [Header("Coin Settings")]
    [SerializeField] Canvas mainCanvas;
    [SerializeField] RectTransform moneyIconRect;
    [SerializeField] float coinWaitBeforeMove = 1f;
    [SerializeField] float coinMoveDuration = 0.6f;

    [Header("Money FX")]
    [SerializeField] MoneyUIFX moneyUIFx; // Inspector'dan ver veya Start'ta bul

    void Start()
    {
        if (moneyUIFx == null)
        {
            moneyUIFx = GetComponentInChildren<MoneyUIFX>(true);
        }

        UpdateMoney();
        UpdateHeath();

        var ui = closeBetween != null ? closeBetween.GetComponent<WaveUIAnimator>() : null;
        if (ui != null) ui.PlayIntro(); // �lk giri�: dim + paneller (ba�l�k yok)
    }

    public void UpdateHeath()
    {
        heathText.text = towerHealth.ToString("0");
    }

    public void UpdateMoney()
    {
        moneyText.text = money.ToString("0");
    }

    // Para art��� i�in tek giri� kap�s�
    public void OnMoneyGained(float amount)
    {
        money += amount;
        UpdateMoney();

        if (moneyUIFx != null)
            moneyUIFx.PlayGain(amount);
    }

    public void StartTheWave()
    {
        MusicManager.Instance.PlayStateMusic(GameState.StartWave);
        for (int i = 0; i < buddyShooting.Count; i++)
        {
            if (buddyShooting[i] != null) buddyShooting[i].canShoot = true;
            else break;
        }

        enemySpawner.makeEnemy = true;
        enemySpawner.madeEnemy = 0;
        enemyKilledInWave = 0;

        // UI: paneller yukar�, dim kapan�r
        var ui = closeBetween != null ? closeBetween.GetComponent<WaveUIAnimator>() : null;
        if (ui != null) ui.PlayStartWave();

        // Kamera: k�sa sars�nt� + ard�ndan yumu�ak mikro hareket ve hafif zoom-in
        var camZoom = Camera.main != null ? Camera.main.GetComponent<CameraShakeZoom>() : null;
        if (camZoom != null)
        {
            camZoom.StartWaveShake();     // k�sa, rahats�z etmeyen sars�nt�
            camZoom.BeginBattleAmbient(); // soft drift + hafif yak�nla�ma
        }

        startGame = true;
    }

    void EndWave()
    {

        StopAllCoroutines();
        MusicManager.Instance?.PlayStateMusic(GameState.EndWave, instantIn: true);
        for (int i = 0; i < buddyShooting.Count; i++)
            if (buddyShooting[i] != null) buddyShooting[i].canShoot = false;

        enemySpawner.makeEnemy = false;

        // Kamera: battle ambient'i kapat ve kameray� orijinale d�nd�r
        var camZoom = Camera.main != null ? Camera.main.GetComponent<CameraShakeZoom>() : null;
        if (camZoom != null) camZoom.EndBattleAmbient();

        // UI: dim gelir  "Wave Completed!" (WaveUIAnimator i�inde)  paneller i�eri
        var ui = closeBetween != null ? closeBetween.GetComponent<WaveUIAnimator>() : null;
        if (ui != null) ui.PlayEndWave();

        levelCount++;
        startGame = false;
    }

    public void CheckEndWave()
    {
        if (maxEnemyForWave == enemyKilledInWave && startGame)
            EndWave();
    }

    // CoinPickup �a��racak
    public RectTransform GetMoneyIconRect() => moneyIconRect;
    public Canvas GetMainCanvas() => mainCanvas;
    public float GetCoinWaitTime() => coinWaitBeforeMove;
    public float GetCoinMoveDuration() => coinMoveDuration;
}
