using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    public float _timeOutTime;

    public Timer timer;

    public Image timerImage;

    private DateTime startTime;

    public Gradient colorGradiant;

    public float maxSFXPitch;

    private void Awake()
    {
        instance = this;

        timer = new Timer(_timeOutTime * 1000);
        timer.AutoReset = false;

        timer.Elapsed += OnTimerEnded;
        GManager.OnStartTimer += StartTimer;
        GManager.OnResetTimer += ResetTimer;
    }

    private void OnDestroy()
    {
        timer.Elapsed -= OnTimerEnded;
        GManager.OnStartTimer -= StartTimer;
        GManager.OnResetTimer -= ResetTimer;

        timer.Dispose();
    }

    private void Update()
    {
        if (timer.Enabled)
        {
            TimeSpan difference = DateTime.Now - startTime;
            float timePerc = Mathf.Abs(Mathf.Clamp01(((float)difference.TotalSeconds) / _timeOutTime) - 1);

            timerImage.color = colorGradiant.Evaluate(timePerc);

            timerImage.fillAmount = timePerc;

            float audioPer = Mathf.Clamp01(timePerc / 0.3f);
            GManager.instance.BattleBGM.GetComponent<AudioSource>().pitch = 1 + (maxSFXPitch * Mathf.Abs(audioPer - 1));
        }
    }

    private void StartTimer()
    {
        startTime = DateTime.Now;
        timer.Start();
    }

    private void ResetTimer()
    {
        startTime = DateTime.Now;
        timer.Start();
    }

    private void OnTimerEnded(object sender, ElapsedEventArgs e)
    {
        Debug.Log($"The Elapsed event was raised");

        timerImage.color = colorGradiant.Evaluate(0);
        timerImage.fillAmount = 1;
        GManager.instance.BattleBGM.GetComponent<AudioSource>().pitch = 1;

        GManager.instance.OnClickSurrenderButton(false, "Surrendered Due To Timeout");
    }
}
