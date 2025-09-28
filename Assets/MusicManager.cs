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
    [SerializeField] AudioSource waveMusic;
    [SerializeField] AudioSource endWaveMusic;

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
            case GameState.Wave: target = waveMusic; break;
            case GameState.EndWave: target = endWaveMusic; break;
        }

        if (target == null || target == currentSource) return;

        StopAllCoroutines();
        StartCoroutine(CrossfadeTo(target, instantIn, customFade.HasValue ? customFade.Value : fadeDuration));
    }

    IEnumerator CrossfadeTo(AudioSource target, bool instantIn, float fadeTime)
    {
        // Eski kaynak
        AudioSource old = currentSource;

        // Hedef kayna�� haz�rla
        if (!target.isPlaying)
            target.Play();

        if (instantIn)
        {
            // Hedef an�nda 1.0 sesle ba�las�n
            target.volume = 1f;

            // Eski par�a varsa �nce hafif "duck", sonra yumu�ak fade-out
            if (old != null && old != target)
            {
                // an�nda k�s
                old.volume = Mathf.Min(old.volume, duckLevel);

                float t = 0f;
                float startVol = old.volume;
                while (t < fadeTime)
                {
                    t += Time.deltaTime;
                    float k = t / fadeTime;
                    if (old) old.volume = Mathf.Lerp(startVol, 0f, k);
                    yield return null;
                }
                if (old) old.Stop();
            }
        }
        else
        {
            // Klasik crossfade: hedef 0'dan fade-in, eski fade-out
            float t = 0f;
            float startOld = old ? old.volume : 0f;
            target.volume = 0f;

            while (t < fadeTime)
            {
                t += Time.deltaTime;
                float k = t / fadeTime;

                if (old) old.volume = Mathf.Lerp(startOld, 0f, k);
                target.volume = Mathf.Lerp(0f, 1f, k);

                yield return null;
            }

            if (old) old.Stop();
            target.volume = 1f;
        }

        currentSource = target;
    }
}
