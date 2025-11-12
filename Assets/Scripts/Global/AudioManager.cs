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

    public int initialPoolSize = 10;  // like thread pools, so multiple instances can play at once without killing the other.

    private Dictionary<string, AudioClip> soundDict;
    private List<AudioSource> pool;
    private Transform poolParent;

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
        // create a new one if we ran out
        return CreateNewSource();
    }

    public void PlaySFX(string name, float volume = 1f, float minPitch = 0.95f, float maxPitch = 1.05f)
    {
        if (!soundDict.TryGetValue(name, out var clip)) return;

        var src = GetAvailableSource();
        src.clip = clip;
        src.pitch = Random.Range(minPitch, maxPitch);  // for variety of sounds
        src.volume = volume;
        src.Play();
    }
}