using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SoundEntry
    {
        public string name;
        public AudioClip clip;
    }

    public List<SoundEntry> sounds = new();
    public List<SoundEntry> musicTracks = new();

    public int initialPoolSize = 10;  // like thread pools, so multiple instances can play at once without killing the other.

    private Dictionary<string, AudioClip> soundDict;
    private List<AudioSource> pool;
    private Transform poolParent;

    private Dictionary<string, AudioClip> musicDict;
    private AudioSource musicSource;
    private Transform musicParent;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        soundDict = new Dictionary<string, AudioClip>();
        foreach (var s in sounds)
            soundDict[s.name] = s.clip;

        pool = new List<AudioSource>();
        poolParent = new GameObject("SFX Pool").transform;
        poolParent.SetParent(transform);

        for (int i = 0; i < initialPoolSize; i++)
            CreateNewSource();

        musicDict = new Dictionary<string, AudioClip>();
        foreach (var m in musicTracks)
            musicDict[m.name] = m.clip;

        musicParent = new GameObject("Music Sources").transform;
        musicParent.SetParent(transform);
        musicSource = musicParent.gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 1f;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        AudioListener.volume = 0.5f; // Lowered the volume, OUCH!
        Instance.PlayMusic("1", 0.7f);
    }

    private AudioSource CreateNewSource()
    {
        var src = poolParent.gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        pool.Add(src);
        return src;
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var s in pool)
            if (!s.isPlaying)
                return s;

        return null;
    }

    public void PlaySFX(string name, float volume = 1f, float minPitch = 0.95f, float maxPitch = 1.05f)
    {
        if (!soundDict.TryGetValue(name, out var clip)) return;

        var src = GetAvailableSource();
        if (!src) return;
        src.clip = clip;
        src.pitch = Random.Range(minPitch, maxPitch);  // for variety of sounds
        src.volume = volume;
        src.Play();
    }

    public void PlayMusic(string name, float volume = 1f)
    {
        if (!musicDict.TryGetValue(name, out var clip)) return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.Play();
    }
}