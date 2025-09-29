using UnityEngine;
using System.Collections;

public enum GameState
{
    Market,
    StartWave,
    Wave,
    EndWave
}

public class MusicManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] AudioSource marketMusic;
    [SerializeField] AudioSource startWaveMusic;
    [SerializeField] AudioSource GameOverMusic;
    [SerializeField] AudioSource endWaveMusic;

    // >>> YEN�: Ana men� m�zi�i
    [SerializeField] AudioSource mainMenuMusic;

    [Header("Defaults")]
    [SerializeField] float fadeDuration = 1.0f;     // genel crossfade s�resi
    [SerializeField] float duckLevel = 0.3f;        // instantIn oldu�unda eski par�an�n anl�k k�s�laca�� seviye (0-1)

    AudioSource currentSource;

    public static MusicManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Varsay�lan: hi�bir source �alm�yorsa currentSource = null
    }

    public void PlayStateMusic(GameState state, bool instantIn = false, float? customFade = null)
    {
        AudioSource target = null;
        switch (state)
        {
            case GameState.Market: target = marketMusic; break;
            case GameState.StartWave: target = startWaveMusic; break;
            case GameState.Wave: target = GameOverMusic; break; // GameOver m�zi�i
            case GameState.EndWave: target = endWaveMusic; break;
        }

        if (target == null) return;
        StopAllCoroutines();
        StartCoroutine(CrossfadeTo(target, instantIn, customFade.HasValue ? customFade.Value : fadeDuration));
    }

    // >>> YEN�: Ana men� m�zi�ini �al (di�erlerini kes)
    public void PlayMainMenuMusic(bool instantIn = false, float? customFade = null)
    {
        if (mainMenuMusic == null) return;
        StopAllCoroutines();
        StartCoroutine(CrossfadeTo(mainMenuMusic, instantIn, customFade.HasValue ? customFade.Value : fadeDuration));
    }

    IEnumerator CrossfadeTo(AudioSource target, bool instantIn, float fadeTime)
    {
        // t�m kaynaklar (>>> YEN�: mainMenuMusic dahil)
        var all = new[] { marketMusic, startWaveMusic, GameOverMusic, endWaveMusic, mainMenuMusic };
        AudioSource old = currentSource != null ? currentSource : GetCurrentlyPlayingOther(target, all);

        if (!target.isPlaying) target.Play();

        if (instantIn)
        {
            target.volume = 1f;

            if (old != null && old != target)
            {
                old.volume = Mathf.Min(old.volume, duckLevel);
                float t = 0f, startVol = old.volume;
                while (t < fadeTime)
                {
                    t += Time.unscaledDeltaTime; // pause etkilenmesin
                    if (old) old.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
                    yield return null;
                }
                if (old) old.Stop();
            }
        }
        else
        {
            float t = 0f;
            float startOld = old ? old.volume : 0f;
            float startNew = 0f;
            target.volume = startNew;

            while (t < fadeTime)
            {
                t += Time.unscaledDeltaTime; // fade her durumda akar
                float k = t / fadeTime;

                if (old) old.volume = Mathf.Lerp(startOld, 0f, k);
                if (target) target.volume = Mathf.Lerp(startNew, 1f, k);

                yield return null;
            }

            // hedefi 1.0'da b�rak, di�er HER �EY� kapat
            if (target) target.volume = 1f;
            foreach (var s in all)
            {
                if (s != null && s != target)
                {
                    s.volume = 0f;
                    s.Stop();
                }
            }
        }

        currentSource = target;
    }

    AudioSource GetCurrentlyPlayingOther(AudioSource except, AudioSource[] all)
    {
        foreach (var s in all)
            if (s != null && s != except && s.isPlaying) return s;
        return null;
    }
}
