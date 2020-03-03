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
    private SoundFX[] soundFXes = null;

    [Header("Background Music")]
    [SerializeField]
    private BackgroundMusic[] backgroundMusics;

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

        SoundFXSetup();
        BackgroundMusicSetup();
    }

    private void SoundFXSetup()
    {
        if (soundFXes.Length > 0)
        {
            foreach (SoundFX sfx in soundFXes)
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
            Debug.LogError("[AudioManager] SoundFXSetup There is no sound source to play!");
        }
    }

    private void BackgroundMusicSetup()
    {
        if (backgroundMusics.Length > 0)
        {
            foreach (BackgroundMusic music in backgroundMusics)
            {
                music.Source = gameObject.AddComponent<AudioSource>();
                music.Source.clip = music.Clip;
                music.Source.volume = music.Volume;
                music.Source.pitch = music.Pitch;
                music.Source.playOnAwake = false;
                music.Source.loop = music.IsLooping;
            }
        }
        else
        {
            Debug.LogError("[AudioManager] BackgroundMusicSetup There is no sound source to play!");
        }
        PlayBackgroundMusic(E_BackgroundMusic.BackgroundMusic01);
    }

    public void PlaySoundFX(E_SoundFX name)
    {
        if (soundFXes.Length > 0)
        {
            //InGameAudio sfx = Array.Find(soundFXes, sound => sound.Name == name);
            SoundFX sfx = soundFXes[(int)name];
            if (sfx != null)
            {
                sfx.Source.Play();
            }
            else
            {
                Debug.LogError("[AudioManager] PlaySoundFX sound with name: " + name.ToString() + " is not found!");
            }
        }
        else
        {
            Debug.LogError("[AudioManager] PlaySoundFX There is no sound source to play!");
        }
    }

    public void StopAllSoundFX()
    {
        if (soundFXes.Length > 0)
        {
            foreach (SoundFX sfx in soundFXes)
            {
                if (sfx != null)
                {
                    sfx.Source.Stop();
                }
                else
                {
                    Debug.LogError("[AudioManager] StopAllSoundFX sound with name: " + sfx.Name.ToString() + " is not found!");
                }
            }
        }
        else
        {
            Debug.LogError("[AudioManager] StopAllSoundFX There is no sound source to stop!");
        }
    }


    public void PlayBackgroundMusic(E_BackgroundMusic name)
    {
        if (backgroundMusics.Length > 0)
        {
            BackgroundMusic music = backgroundMusics[(int)name];
            if (music != null)
            {
                music.Source.Play();
            }
            else
            {
                Debug.LogError("[AudioManager] PlayBackgroundMusic sound with name: " + name.ToString() + " is not found!");
            }
        }
        else
        {
            Debug.LogError("[AudioManager] PlayBackgroundMusic There is no sound source to play!");
        }
    }

    public void StopBackgroundMusic(E_BackgroundMusic name)
    {
        if (backgroundMusics.Length > 0)
        {
            BackgroundMusic music = backgroundMusics[(int)name];
            if (music != null)
            {
                music.Source.Stop();
            }
            else
            {
                Debug.LogError("[AudioManager] StopBackgroundMusic sound with name: " + name.ToString() + " is not found!");
            }
        }
        else
        {
            Debug.LogError("[AudioManager] StopBackgroundMusic There is no sound source to stop!");
        }
    }

    public void StopAllBackgroundMusics()
    {
        if (backgroundMusics.Length > 0)
        {
            foreach (BackgroundMusic music in backgroundMusics)
            {
                if (music != null)
                {
                    music.Source.Stop();
                }
                else
                {
                    Debug.LogError("[AudioManager] StopAllBackgroundMusics sound with name: " + music.Name.ToString() + " is not found!");
                }
            }
        }
        else
        {
            Debug.LogError("[AudioManager] StopAllBackgroundMusics There is no sound source to stop!");
        }
    }
}
