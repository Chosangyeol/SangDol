using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource audioSource;

    private void Awake()
    {
        instance = this;

        audioSource = GetComponent<AudioSource>();
    }

    public void PlayBGM(AudioClip clip)
    {
        audioSource.Stop();

        audioSource.clip = clip;

        audioSource.Play();
    }
}
