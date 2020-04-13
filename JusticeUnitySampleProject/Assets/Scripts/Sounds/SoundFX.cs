using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SoundFX
{
    public E_SoundFX Name;
    public AudioClip Clip;

    [Range(0f,1f)]
    public float Volume;
    [Range(0.1f, 3f)]
    public float Pitch;

    public bool IsLooping;

    [HideInInspector]
    public AudioSource Source;
}
