using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SFXEntry
{
    public string id;                    // "ui_click", "hit", "coin" gibi
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    public Vector2 pitchRandom = new Vector2(1f, 1f); // (min,max) rastgele pitch
    [Tooltip("Ayný sfx üst üste spam olmasýn diye minimum aralýk (sn). 0 = kapalý")]
    public float cooldown = 0f;
}

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Library")]
    [SerializeField] List<SFXEntry> sfxList = new List<SFXEntry>();

    [Header("Routing / Volumes")]
    [SerializeField] AudioMixerGroup outputMixer; // opsiyonel
    [Range(0f, 1f)] [SerializeField] float masterVolume = 1f;

    [Header("Pooling")]
    [SerializeField] int pool2DSize = 6;

    [Header("3D Defaults")]
    [SerializeField] float spatialBlend3D = 1f;
    [SerializeField] float minDistance = 2f;
    [SerializeField] float maxDistance = 25f;

    readonly Dictionary<string, SFXEntry> _map = new Dictionary<string, SFXEntry>();
    readonly Queue<AudioSource> _pool2D = new Queue<AudioSource>();
    readonly Dictionary<string, float> _lastPlayTimes = new Dictionary<string, float>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Map oluþtur
        _map.Clear();
        foreach (var e in sfxList)
        {
            if (!string.IsNullOrEmpty(e.id) && e.clip != null && !_map.ContainsKey(e.id))
                _map.Add(e.id, e);
        }

        // 2D pool hazýrla
        for (int i = 0; i < pool2DSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f; // 2D
            if (outputMixer) src.outputAudioMixerGroup = outputMixer;
            _pool2D.Enqueue(src);
        }
    }

    // ---- Public API ----

    /// <summary>2D SFX (UI vb.) — ID ile çalar.</summary>
    public void Play(string id, float volumeScale = 1f)
    {
        if (!_map.TryGetValue(id, out var e) || e.clip == null) return;
        if (IsOnCooldown(id, e.cooldown)) return;

        var src = Get2DSource();
        src.clip = e.clip;
        src.volume = masterVolume * e.volume * Mathf.Clamp01(volumeScale);
        src.pitch = Random.Range(e.pitchRandom.x, e.pitchRandom.y);
        src.Play();

        MarkPlayed(id);
        Return2DAfter(src, e.clip.length / Mathf.Max(0.01f, src.pitch));
    }

    /// <summary>3D SFX — dünyada bir pozisyonda çalar.</summary>
    public void PlayAt(string id, Vector3 worldPos, float volumeScale = 1f)
    {
        if (!_map.TryGetValue(id, out var e) || e.clip == null) return;
        if (IsOnCooldown(id, e.cooldown)) return;

        var go = new GameObject($"SFX3D_{id}");
        var src = go.AddComponent<AudioSource>();
        go.transform.position = worldPos;

        src.playOnAwake = false;
        src.loop = false;
        src.clip = e.clip;
        src.volume = masterVolume * e.volume * Mathf.Clamp01(volumeScale);
        src.pitch = Random.Range(e.pitchRandom.x, e.pitchRandom.y);
        src.spatialBlend = spatialBlend3D;
        src.minDistance = minDistance;
        src.maxDistance = maxDistance;
        if (outputMixer) src.outputAudioMixerGroup = outputMixer;

        src.Play();
        MarkPlayed(id);

        Destroy(go, e.clip.length / Mathf.Max(0.01f, src.pitch) + 0.05f);
    }

    public void SetMasterVolume(float v) => masterVolume = Mathf.Clamp01(v);

    // ---- Helpers ----

    bool IsOnCooldown(string id, float cd)
    {
        if (cd <= 0f) return false;
        if (_lastPlayTimes.TryGetValue(id, out var last))
            return Time.unscaledTime - last < cd;
        return false;
    }

    void MarkPlayed(string id) => _lastPlayTimes[id] = Time.unscaledTime;

    AudioSource Get2DSource()
    {
        // Havuzdan al — aktif çalýyorsa sýranýn sonuna atýp bir sonrakini dene
        for (int i = 0; i < _pool2D.Count; i++)
        {
            var src = _pool2D.Dequeue();
            if (!src.isPlaying) { _pool2D.Enqueue(src); return src; }
            _pool2D.Enqueue(src);
        }
        // Hepsi doluysa yeni bir tane ekle (nadir)
        var extra = gameObject.AddComponent<AudioSource>();
        extra.playOnAwake = false;
        extra.loop = false;
        extra.spatialBlend = 0f;
        if (outputMixer) extra.outputAudioMixerGroup = outputMixer;
        return extra;
    }

    void Return2DAfter(AudioSource src, float time)
    {
        // 2D kaynak component olduðu için otomatik döner; burada sadece güvence amaçlý
        // Ýstersen coroutine ile volume fade-out vs. eklenebilir.
    }
}
