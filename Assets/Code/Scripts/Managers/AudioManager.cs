using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    private Dictionary<string, AudioClip[]> sfxGroups = new();

    // --- Music system ---
    private List<AudioClip> musicPlaylist = new();
    private int musicIndex = 0;
    private float musicTimer = 0f;
    private const float songInterval = 180f; // 3 minutes
    private float startDelay = 30f; // 30 seconden stilte
    private bool musicStarted = false;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadAllSFXFolders();
            LoadMusicPlaylist();
        }
        else Destroy(gameObject);
    }

    void Update()
{
    if (!musicStarted)
    {
        musicTimer += Time.deltaTime;
        if (musicTimer >= startDelay)
        {
            musicTimer = 0f;
            StartMusicInternal();
            musicStarted = true;
        }
        return;
    }

    musicTimer += Time.deltaTime;
    if (musicTimer >= songInterval)
    {
        StartMusicInternal();
        musicTimer = 0f;
    }
}


    // -------------------------
    //       LOAD SFX
    // -------------------------
    void LoadAllSFXFolders()
    {
        string[] folders =
        {
            "Big_Bubbles", "Damage_Hit","Dash","Eating","Pickup",
            "Medium_Bubbles","Small_Bubbles","Swimming","Splash"
        };

        foreach (var folder in folders)
        {
            var clips = Resources.LoadAll<AudioClip>($"Audio/Sound/{folder}");
            if (clips.Length > 0)
                sfxGroups[folder] = clips;
        }
    }

    // -------------------------
    //       PLAY SFX
    // -------------------------
public void Play(string category)
{
    if (!sfxGroups.TryGetValue(category, out var clips) || clips.Length == 0)
        return;

    int idx = Random.Range(0, clips.Length);
    float pitch = Random.Range(0.7f, 1.4f);

    sfxSource.pitch = pitch;
    sfxSource.PlayOneShot(clips[idx]);
}


    // -------------------------
    //       MUSIC
    // -------------------------
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    // -------------------------
    //   MUSIC PLAYLIST SYSTEM
    // -------------------------
    void LoadMusicPlaylist()
    {
        var clips = Resources.LoadAll<AudioClip>("Audio/Music");

        musicPlaylist = new List<AudioClip>(clips);

        if (musicPlaylist.Count == 0)
        {
            Debug.LogWarning("No music in Resources/Audio/Music/");
            return;
        }

        Shuffle(musicPlaylist);
    }

    // start de volgende track uit de playlist
    void StartMusicInternal()
    {
        if (musicPlaylist.Count == 0) return;

        var clip = musicPlaylist[musicIndex];
        PlayMusic(clip, false);

        musicIndex++;

        if (musicIndex >= musicPlaylist.Count)
        {
            Shuffle(musicPlaylist);
            musicIndex = 0;
        }
    }

    void Shuffle(List<AudioClip> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
