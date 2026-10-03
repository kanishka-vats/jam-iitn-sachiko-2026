using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX")]
    [SerializeField] private List<SFXEntry> sfxEntries = new();

    [Header("SFX Master Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float sfxMasterVolume = 0.5f;

    [Header("Music")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    private Dictionary<SFXType, SFXEntry> sfxDictionary;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeSFX();
    }

    private void InitializeSFX()
    {
        sfxDictionary = new Dictionary<SFXType, SFXEntry>();

        foreach (SFXEntry entry in sfxEntries)
        {
            if (entry.clip == null)
                continue;

            if (!sfxDictionary.ContainsKey(entry.type))
            {
                sfxDictionary.Add(entry.type, entry);
            }
            else
            {
                Debug.LogWarning(
                    $"Duplicate SFX type found: {entry.type}"
                );
            }
        }
    }

    public void PlaySFX(SFXType type)
    {
        if (!sfxDictionary.TryGetValue(type, out SFXEntry entry))
        {
            Debug.LogWarning(
                $"No audio clip assigned for SFX: {type}"
            );

            return;
        }

        float randomPitch = Random.Range(
            entry.pitchMin,
            entry.pitchMax
        );

        float randomVolume = Random.Range(
            entry.volume * 0.9f,
            entry.volume
        );

        sfxSource.pitch = randomPitch;

        sfxSource.PlayOneShot(
            entry.clip,
            randomVolume * sfxMasterVolume
        );

        sfxSource.pitch = 1f;
    }

    public void PlayMusic(MusicType type)
    {
        AudioClip clip = type switch
        {
            MusicType.MainMenu => mainMenuMusic,
            MusicType.Gameplay => gameplayMusic,
            _ => null
        };

        if (clip == null)
        {
            Debug.LogWarning(
                $"No music assigned for: {type}"
            );

            return;
        }

        if (musicSource.clip == clip &&
            musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetSFXVolume(float volume)
    {
        sfxMasterVolume = Mathf.Clamp01(volume);
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (sfxEntries == null)
            sfxEntries = new List<SFXEntry>();

        int enumCount =
            System.Enum.GetValues(typeof(SFXType)).Length;

        while (sfxEntries.Count < enumCount)
        {
            sfxEntries.Add(new SFXEntry());
        }

        for (int i = 0; i < enumCount; i++)
        {
            sfxEntries[i].type = (SFXType)i;
        }

        while (sfxEntries.Count > enumCount)
        {
            sfxEntries.RemoveAt(sfxEntries.Count - 1);
        }
    }

#endif
}