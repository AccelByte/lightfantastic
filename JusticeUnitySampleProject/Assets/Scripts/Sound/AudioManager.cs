using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance { get { return instance; } }

    [Header("Sound FX")]
    [SerializeField]
    private InGameAudio[] soundFXes = null;

    [Header("Background Musics")]
    [SerializeField]
    private InGameAudio[] backgroundMusics;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);

        if (soundFXes.Length > 0)
        {
            foreach (InGameAudio sfx in soundFXes)
            {
                sfx.Source = gameObject.AddComponent<AudioSource>();
                sfx.Source.clip = sfx.Clip;
                sfx.Source.volume = sfx.Volume;
                sfx.Source.pitch = sfx.Pitch;
                sfx.Source.playOnAwake = false;
                sfx.Source.loop = sfx.IsLooping;
            }
        }
        else
        {
            Debug.LogError("[AudioManager] Awake There is no sound source to play!");
        }
    }

    public void PlaySoundFX(SoundFX name)
    {
        if (soundFXes.Length > 0)
        {
            //InGameAudio sfx = Array.Find(soundFXes, sound => sound.Name == name);
            InGameAudio sfx = soundFXes[(int)name];
            if (sfx != null)
            {
                sfx.Source.Play();
            }
            else
            {
                Debug.LogError("[AudioManager] PlaySoundFX sound with name: " + name + " is not found!");
            }
        }
        else
        {
            Debug.LogError("[AudioManager] PlaySoundFX There is no sound source to play!");
        }
    }
}
