using UnityEngine;

[System.Serializable]
public class SFXEntry
{
    public SFXType type;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0.5f, 1.5f)]
    public float pitchMin = 1f;

    [Range(0.5f, 1.5f)]
    public float pitchMax = 1f;
}