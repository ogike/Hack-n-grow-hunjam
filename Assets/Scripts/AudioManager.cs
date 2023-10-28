using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource _source;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one audio manager!");
            return;
        }
        
        Instance = this;

        _source = GetComponent<AudioSource>();
        if (_source == null)
        {
            Debug.LogError("No audiosource set for audiomanager");
        }
    }

    public void PlayAudio(AudioClip clip)
    {
        _source.PlayOneShot(clip);
    }
}
