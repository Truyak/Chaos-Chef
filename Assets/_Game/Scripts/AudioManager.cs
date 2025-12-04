using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioMixer audioMixer;

    [Header("Musics")]
    public AudioClip mainMenuMusic;
    public AudioClip gameMusic;

    [Header("Customer SFX Clips")]
    public AudioClip complaintsSFX;
    public AudioClip allergySFX;
    public AudioClip shockSFX;
    public AudioClip badCommentsSFX;

    [Header("Waiter SFX Clips")]
    public AudioClip stunSFX;
    public AudioClip damageSFX;
    public AudioClip whoopSFX;

    public AudioClip winSFX;
    public AudioClip loseSFX;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private string currentSceneName = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void InitializeAudioSources()
    {
        GameObject musicGO = new GameObject("MusicSource");
        musicGO.transform.SetParent(transform);
        musicSource = musicGO.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Music")[0];
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        GameObject sfxGO = new GameObject("SFXSource");
        sfxGO.transform.SetParent(transform);
        sfxSource = sfxGO.AddComponent<AudioSource>();
        sfxSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        if (sceneName == currentSceneName) return;

        currentSceneName = sceneName;

        if (sceneName.Contains("MainMenu") || sceneName == "Menu")
            PlayMusic(mainMenuMusic);
        else if (sceneName.Contains("Game"))
            PlayMusic(gameMusic);
        else
            StopMusic();
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // ------------------- SFX FUNCTIONS -------------------
    public void PlayComplaints() => PlaySFX(complaintsSFX);
    public void PlayAllergy() => PlaySFX(allergySFX);
    public void PlayStun() => PlaySFX(stunSFX);
    public void PlayBadComments() => PlaySFX(badCommentsSFX);
    public void PlayShock() => PlaySFX(shockSFX);
    public void PlayDamage() => PlaySFX(damageSFX);
    public void PlayWin() => PlaySFX(winSFX);
    public void PlayLose() => PlaySFX(loseSFX);
    public void PlayWhoop() => PlaySFX(whoopSFX);

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

}