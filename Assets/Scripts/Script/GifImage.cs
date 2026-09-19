using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GifImage : MonoBehaviour
{
    [SerializeField] Sprite[] _pics;
    int _picsPerSecond = 10;
    Image _image;
    float _time = 0.0f;
    int _currentFrame = 0;
    float _frameDelay = 0.0f;

    void Start()
    {
        _image = GetComponent<Image>();

        if (_picsPerSecond > 0)
        {
            _frameDelay = 1f / _picsPerSecond;
        }
    }

    void Update()
    {
        if (_pics == null)
        {
            return;
        }

        if (_pics.Length == 0)
        {
            return;
        }

        if (_image == null)
        {
            return;
        }

        if (_frameDelay <= 0)
        {
            return;
        }

        _time += Time.deltaTime;

        if (_time >= _frameDelay)
        {
            _currentFrame = (_currentFrame + 1) % _pics.Length;
            _time = 0.0f;

            _image.sprite = _pics[_currentFrame];
        }
    }
}
