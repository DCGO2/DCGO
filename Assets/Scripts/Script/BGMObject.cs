using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class BGMObject : MonoBehaviour
{
    public AudioSource _audio { get; set; }
    public bool isPlaying { get; set; } = false;
    bool isFading { get; set; } = false;
    private void Start()
    {

    }

    public void StopPlayBGM()
    {
        _audio = GetComponent<AudioSource>();

        _audio.Stop();

        _audio.clip = null;

        isPlaying = false;
    }

    public void StartPlayBGM(AudioClip clip)
    {
        _audio = GetComponent<AudioSource>();

        if (clip != null)
        {
            _audio.clip = clip;
        }

        if (ContinuousController.instance != null)
        {
            ContinuousController.instance.ChangeBGMVolume(_audio);
        }

        _audio.Play();

        isPlaying = true;
    }

    private void Update()
    {
        if (ContinuousController.instance != null)
        {
            if (_audio != null && isPlaying && !isFading)
            {
                ContinuousController.instance.ChangeBGMVolume(_audio);
            }
        }
    }

    public IEnumerator FadeOut(float duration)
    {
        _audio = GetComponent<AudioSource>();

        bool end = false;
        isFading = true;

        var sequence = DOTween.Sequence();

        sequence
            .Append(DOTween.To(() => _audio.volume, (value) => _audio.volume = value, 0, duration))
            .AppendCallback(() => end = true);

        sequence.Play();

        yield return new WaitWhile(() => !end);

        isPlaying = false;
        isFading = false;
    }
}
