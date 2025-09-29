using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Dan.Main;

public class GameManager : MonoBehaviour
{
    public float money;
    [SerializeField] TextMeshProUGUI moneyText;
    public bool startGame;
    public List<Shooting> buddyShooting;
    [SerializeField] EnemySpawner enemySpawner;
    [SerializeField] GameObject closeBetween;          // WaveUIAnimator bu objede
    [SerializeField] TextMeshProUGUI heathText;
    public GameObject kaleForEnemy;

    [Header("Leaderboard")]
    [SerializeField] LeaderboardReference leaderboardRef = Leaderboards.leaderBoard;
    [SerializeField] string mainMenuScene = "MainMenu";

    public int enemyKilledInWave;

    public float towerHealth;

    public Vector2[] levelHardnes;   // x: enemy health mult, y: enemy damage mult
    public int levelCount;

    [Header("Coin Settings")]
    [SerializeField] Canvas mainCanvas;
    [SerializeField] RectTransform moneyIconRect;
    [SerializeField] float coinWaitBeforeMove = 1f;
    [SerializeField] float coinMoveDuration = 0.6f;

    [Header("Money FX")]
    [SerializeField] MoneyUIFX moneyUIFx; // Inspector�dan ver veya Start�ta bul

    // Puan
    [SerializeField] TextMeshProUGUI pointText;
    float coinPoint;
    float towerHealthPoint;
    int totalPoint;
    [SerializeField] float[] pointMult;   // her level i�in �arpan
    public float toalGoblinPoint;         // (sen d��ar�dan topluyorsun)

    [Header("Game Over")]
    [SerializeField] GameOverUI gameOverUI;   // Inspector�dan ver
    bool gameOverTriggered = false;

    void Start()
    {
        if (moneyUIFx == null)
            moneyUIFx = GetComponentInChildren<MoneyUIFX>(true);

        UpdateMoney();
        UpdateHeath();

        var ui = closeBetween != null ? closeBetween.GetComponent<WaveUIAnimator>() : null;
        if (ui != null) ui.PlayIntro(); // �lk giri�: dim + paneller (ba�l�k yok)
    }

    void Update()
    {
        if (!gameOverTriggered && towerHealth <= 0f)
            TriggerGameOver();
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
        Puanlar�Topla();
        toalGoblinPoint = 0;

        MusicManager.Instance?.PlayStateMusic(GameState.EndWave, instantIn: true);

        for (int i = 0; i < buddyShooting.Count; i++)
            if (buddyShooting[i] != null) buddyShooting[i].canShoot = false;

        enemySpawner.makeEnemy = false;

        // Kamera: battle ambient'i kapat ve kameray� orijinale d�nd�r
        var camZoom = Camera.main != null ? Camera.main.GetComponent<CameraShakeZoom>() : null;
        if (camZoom != null) camZoom.EndBattleAmbient();

        // UI: dim gelir + "Wave Completed!" + paneller i�eri
        var ui = closeBetween != null ? closeBetween.GetComponent<WaveUIAnimator>() : null;
        if (ui != null) ui.PlayEndWave();

        levelCount++;
        startGame = false;
    }

    public void CheckEndWave()
    {
        if (enemySpawner.maxEnemyForWave[levelCount] == enemyKilledInWave && startGame)
            EndWave();
    }

    // CoinPickup �a��racak
    public RectTransform GetMoneyIconRect() => moneyIconRect;
    public Canvas GetMainCanvas() => mainCanvas;
    public float GetCoinWaitTime() => coinWaitBeforeMove;
    public float GetCoinMoveDuration() => coinMoveDuration;

    public void Puanlar�Topla()
    {
        coinPoint = money;
        towerHealthPoint = Mathf.Max(0f, towerHealth);
        float mult = (pointMult != null && pointMult.Length > levelCount) ? pointMult[levelCount] : 1f;
        totalPoint = (int)((coinPoint + towerHealthPoint + toalGoblinPoint) * mult);

        Debug.Log($"[Puan] coin={coinPoint}, tower={towerHealthPoint}, goblin={toalGoblinPoint}, mult={mult} => total={totalPoint}");

        if (pointText != null) pointText.text = totalPoint.ToString();
    }

    // GameManager.cs

    void TriggerGameOver()
    {
        gameOverTriggered = true;

        // Sava� dursun (AI, spawn vs.)
        StopAllCoroutines();
        if (enemySpawner) enemySpawner.makeEnemy = false;
        startGame = false;
        for (int i = 0; i < buddyShooting.Count; i++)
            if (buddyShooting[i] != null) buddyShooting[i].canShoot = false;

        // Kamera ambient�i kapat
        var camZoom = Camera.main != null ? Camera.main.GetComponent<CameraShakeZoom>() : null;
        if (camZoom != null) camZoom.EndBattleAmbient();

        // Puan hesapla  DEBUG LOG�u ekledim
        Puanlar�Topla();
        Debug.Log($"[GameOver] totalPoint = {totalPoint} (coin={money}, tower={towerHealth}, goblin={toalGoblinPoint})");


        MusicManager.Instance?.PlayStateMusic(GameState.Wave, instantIn: false);

        // UI: �nce FULL BLACK, sonra pause + popup�lar
        if (gameOverUI != null)
            gameOverUI.BlackoutThenShow(totalPoint, 0.4f);   // 0.4s�de full siyah
        else
            Debug.LogWarning("GameOverUI referans� atanmad�!", this);
    }

    public void SubmitScoreAndReturnToMenu(string playerName)
    {
        // G�vence: isim, skor
        string name = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName.Trim();

        // Skoru g�ncelledi�inden emin ol
        Puanlar�Topla();  // totalPoint�i g�nceller

        // Upload  sahne de�i�tir (Time.timeScale'i de a�)
        leaderboardRef.UploadNewEntry(name, totalPoint, isSuccessful =>
        {
            // oyun ak���n� eski haline getir
            Time.timeScale = 1f;

            // log at (debug i�in)
            Debug.Log($"[Leaderboard] Upload {(isSuccessful ? "OK" : "FAIL")} | {name} - {totalPoint}");

            // ana men�ye ge� (ba�ar�l�/ba�ar�s�z fark etmeden d�nmek istersen bu �ekilde)
            if (!string.IsNullOrEmpty(mainMenuScene))
                SceneManager.LoadScene(mainMenuScene);
        });
    }

}
