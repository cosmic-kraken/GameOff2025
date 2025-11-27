using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource sfxSource;
    public float pitchMin = 0.9f;
    public float pitchMax = 1.1f;

    private Dictionary<string, AudioClip[]> sfxGroups = new Dictionary<string, AudioClip[]>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllFolders();
        }
        else Destroy(gameObject);
    }

    void LoadAllFolders()
    {
        string[] folders = { "Damage_Hit", "Dash", "Eating", "Pickup", "Regular_Bubbles", "Small_Bubbles", "Swimming" };

        foreach (var folder in folders)
        {
            AudioClip[] clips = Resources.LoadAll<AudioClip>($"Audio/Sound/{folder}");
            if (clips.Length > 0)
                sfxGroups[folder] = clips;
        }
    }

    public void Play(string category)
    {
        if (!sfxGroups.TryGetValue(category, out var clips) || clips.Length == 0)
        {
            Debug.LogWarning($"No clips for '{category}'");
            return;
        }

        int idx = Random.Range(0, clips.Length);

        // Apply random pitch on the connected AudioSource
        sfxSource.pitch = Random.Range(pitchMin, pitchMax);
        sfxSource.PlayOneShot(clips[idx]);

        // Reset pitch to default after playing
        sfxSource.pitch = 1f;
    }
}
