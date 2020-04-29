// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AccelByte.Core;
using UnityEngine.Audio;

public struct AudioManagerState
{
    public bool BGM_On;
    public bool SFX_On;
}

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance { get { return instance; } }

    public const string PLAYER_PREF_AUDIO_BGM = "setting_audio_bgm";
    public const string PLAYER_PREF_AUDIO_SFX = "setting_audio_sfx";
    
    [Header("Sound FX")]
    [SerializeField]
    private SoundFX[] soundFXes = null;

    [Header("Background Music")]
    [SerializeField]
    private BackgroundMusic[] backgroundMusics;

    private AudioManagerState currentState = new AudioManagerState{BGM_On = true, SFX_On = true};

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
        
        MainThreadTaskRunner.Instance.Run(() =>
        {
            ToggleBGMVolume(PlayerPrefs.GetInt(PLAYER_PREF_AUDIO_BGM) == 1);
            ToggleSFXVolume(PlayerPrefs.GetInt(PLAYER_PREF_AUDIO_SFX) == 1);
        });
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

    public void ToggleSFXVolume(bool ON)
    {
        PlayerPrefs.SetInt(PLAYER_PREF_AUDIO_SFX, ON ? 1 : 0);
        foreach (var sfx in soundFXes)
        {
            sfx.Source.volume = ON ? 1 : 0;
        }
        currentState.SFX_On = ON;
    }

    public void ToggleBGMVolume(bool ON)
    {
        PlayerPrefs.SetInt(PLAYER_PREF_AUDIO_BGM, ON ? 1 : 0);
        foreach (var bgm in backgroundMusics)
        {
            bgm.Source.volume = ON ? 1 : 0;
        }
        currentState.BGM_On = ON;
    }

    public AudioManagerState GetAudioState()
    {
        var out_ = new AudioManagerState()
        {
            BGM_On = currentState.BGM_On,
            SFX_On = currentState.SFX_On
        };
        return out_;
    }
}
