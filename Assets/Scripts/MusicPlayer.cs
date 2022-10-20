using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip musicTrack;
    
    private AudioSource _audioSource;

    private static MusicPlayer _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            GameObject.Destroy(this.gameObject);
            return;
        }

        _audioSource = GetComponent<AudioSource>();

        _audioSource.clip = musicTrack;
        _audioSource.loop = true;
        _audioSource.Play();
    }
}
