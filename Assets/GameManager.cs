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
    [SerializeField] MoneyUIFX moneyUIFx; // Inspector’dan ver veya Start’ta bul

    // Puan
    [SerializeField] TextMeshProUGUI pointText;
    float coinPoint;
    float towerHealthPoint;
    int totalPoint;
    [SerializeField] float[] pointMult;   // her level için çarpan
    public float toalGoblinPoint;         // (sen dýþarýdan topluyorsun)

    [Header("Game Over")]
    [SerializeField] GameOverUI gameOverUI;   // Inspector’dan ver
    bool gameOverTriggered = false;

    void Start()
    {
        if (moneyUIFx == null)
            moneyUIFx = GetComponentInChildren<MoneyUIFX>(true);

        UpdateMoney();
        UpdateHeath();

        var ui = closeBetween != null ? closeBetween.GetComponent<WaveUIAnimator>() : null;
        if (ui != null) ui.PlayIntro(); // Ýlk giriþ: dim + paneller (baþlýk yok)
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

    // Para artýþý için tek giriþ kapýsý
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

        // UI: paneller yukarý, dim kapanýr
        var ui = closeBetween != null ? closeBetween.GetComponent<WaveUIAnimator>() : null;
        if (ui != null) ui.PlayStartWave();

        // Kamera: kýsa sarsýntý + ardýndan yumuþak mikro hareket ve hafif zoom-in
        var camZoom = Camera.main != null ? Camera.main.GetComponent<CameraShakeZoom>() : null;
        if (camZoom != null)
        {
            camZoom.StartWaveShake();     // kýsa, rahatsýz etmeyen sarsýntý
            camZoom.BeginBattleAmbient(); // soft drift + hafif yakýnlaþma
        }

        startGame = true;
    }

    void EndWave()
    {
        StopAllCoroutines();
        PuanlarýTopla();
        toalGoblinPoint = 0;

        MusicManager.Instance?.PlayStateMusic(GameState.EndWave, instantIn: true);

        for (int i = 0; i < buddyShooting.Count; i++)
            if (buddyShooting[i] != null) buddyShooting[i].canShoot = false;

        enemySpawner.makeEnemy = false;

        // Kamera: battle ambient'i kapat ve kamerayý orijinale döndür
        var camZoom = Camera.main != null ? Camera.main.GetComponent<CameraShakeZoom>() : null;
        if (camZoom != null) camZoom.EndBattleAmbient();

        // UI: dim gelir + "Wave Completed!" + paneller içeri
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

    // CoinPickup çaðýracak
    public RectTransform GetMoneyIconRect() => moneyIconRect;
    public Canvas GetMainCanvas() => mainCanvas;
    public float GetCoinWaitTime() => coinWaitBeforeMove;
    public float GetCoinMoveDuration() => coinMoveDuration;

    public void PuanlarýTopla()
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

        // Savaþ dursun (AI, spawn vs.)
        StopAllCoroutines();
        if (enemySpawner) enemySpawner.makeEnemy = false;
        startGame = false;
        for (int i = 0; i < buddyShooting.Count; i++)
            if (buddyShooting[i] != null) buddyShooting[i].canShoot = false;

        // Kamera ambient’i kapat
        var camZoom = Camera.main != null ? Camera.main.GetComponent<CameraShakeZoom>() : null;
        if (camZoom != null) camZoom.EndBattleAmbient();

        // Puan hesapla  DEBUG LOG’u ekledim
        PuanlarýTopla();
        Debug.Log($"[GameOver] totalPoint = {totalPoint} (coin={money}, tower={towerHealth}, goblin={toalGoblinPoint})");


        MusicManager.Instance?.PlayStateMusic(GameState.Wave, instantIn: false);

        // UI: önce FULL BLACK, sonra pause + popup’lar
        if (gameOverUI != null)
            gameOverUI.BlackoutThenShow(totalPoint, 0.4f);   // 0.4s’de full siyah
        else
            Debug.LogWarning("GameOverUI referansý atanmadý!", this);
    }

    public void SubmitScoreAndReturnToMenu(string playerName)
    {
        // Güvence: isim, skor
        string name = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName.Trim();

        // Skoru güncellediðinden emin ol
        PuanlarýTopla();  // totalPoint’i günceller

        // Upload  sahne deðiþtir (Time.timeScale'i de aç)
        leaderboardRef.UploadNewEntry(name, totalPoint, isSuccessful =>
        {
            // oyun akýþýný eski haline getir
            Time.timeScale = 1f;

            // log at (debug için)
            Debug.Log($"[Leaderboard] Upload {(isSuccessful ? "OK" : "FAIL")} | {name} - {totalPoint}");

            // ana menüye geç (baþarýlý/baþarýsýz fark etmeden dönmek istersen bu þekilde)
            if (!string.IsNullOrEmpty(mainMenuScene))
                SceneManager.LoadScene(mainMenuScene);
        });
    }

}
