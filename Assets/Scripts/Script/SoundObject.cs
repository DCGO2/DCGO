using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundObject : MonoBehaviour
{
    [HideInInspector] public bool isStartPlay;
    public AudioSource _audio { get; set; }

    public void PlaySE(AudioClip clip)
    {
        _audio = GetComponent<AudioSource>();

        _audio.clip = clip;

        if (ContinuousController.instance != null)
        {
            ContinuousController.instance.ChangeSEVolume(_audio);
        }

        _audio.Play();
        isStartPlay = true;
    }

    private void Update()
    {
        // === DCGO-CUSTOM:android begin ===
        if (_audio == null)
        {
            return;
        }
        // === DCGO-CUSTOM:android end ===

        if (ContinuousController.instance != null)
        {
            ContinuousController.instance.ChangeSEVolume(_audio);
        }

        if (!_audio.isPlaying && isStartPlay)
        {
            Destroy(this.gameObject);
        }
    }
}
