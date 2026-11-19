using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Música")]
    public AudioClip music;
    [Range(0f, 1f)] public float musicVolume = 0.25f;

    [Header("Efectos")]
    public AudioClip click;
    public AudioClip error;
    public AudioClip success;
    public AudioClip fail;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    AudioSource musicSource;
    AudioSource sfxSource;

    void Awake()
    {
        // Si ya existe uno (por ejemplo, el del menú), este se destruye
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;
        musicSource.clip = music;
        if (music != null) musicSource.Play();

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    void Play(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // Atajos que no fallan si no hay AudioManager en la escena
    public static void Click() { if (Instance != null) Instance.Play(Instance.click); }
    public static void Error() { if (Instance != null) Instance.Play(Instance.error); }
    public static void Success() { if (Instance != null) Instance.Play(Instance.success); }
    public static void Fail() { if (Instance != null) Instance.Play(Instance.fail); }
}