using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VolumePanel : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_15 = new WaitForSeconds(0.15f);
    private static readonly int CloseHash = Animator.StringToHash("Close");
    private static readonly int OpenHash = Animator.StringToHash("Open");
    [Header("SEスライダー")]
    public Slider SESlier;

    [Header("BGMスライダー")]
    public Slider BGMSlier;

    [Header("アニメーター")]
    public Animator anim;

    public void Open()
    {
        SESlier.onValueChanged.RemoveAllListeners();
        BGMSlier.onValueChanged.RemoveAllListeners();

        if (ContinuousController.instance != null)
        {
            SESlier.value = ConvertPlayerPrefsValueToSliderValue(ContinuousController.instance.SEVolume, SESlier);
            BGMSlier.value = ConvertPlayerPrefsValueToSliderValue(ContinuousController.instance.BGMVolume, BGMSlier);
        }

        SESlier.onValueChanged.AddListener(value =>
        {
            value = ConvertSliderValueToPlayerPrefsValue(value, SESlier);
            SetSEVlolume(value);
            PlaySampleSE(value);
        });

        BGMSlier.onValueChanged.AddListener(value =>
        {
            value = ConvertSliderValueToPlayerPrefsValue(value, BGMSlier);
            SetBGMVlolume(value);
        });

        gameObject.SetActive(true);
        anim.SafeSetInt(OpenHash, 1);
        anim.SafeSetInt(CloseHash, 0);
    }

    public void Off()
    {
        this.gameObject.SetActive(false);
    }

    public void Close()
    {
        Close_(true);
    }

    public void Close_(bool playSE)
    {
        if (playSE)
        {
            if (Opening.instance != null)
            {
                Opening.instance.PlayCancelSE();
            }

            else if (GManager.instance != null)
            {
                GManager.instance.PlayCancelSE();
            }
        }

        anim.SafeSetInt(OpenHash, 0);
        anim.SafeSetInt(CloseHash, 1);
    }

    float ConvertPlayerPrefsValueToSliderValue(float playerPrefsValue, Slider slider)
    {
        return playerPrefsValue * (slider.wholeNumbers ? slider.maxValue : 1);
    }

    float ConvertSliderValueToPlayerPrefsValue(float sliderValue, Slider slider)
    {
        return sliderValue / (slider.wholeNumbers && slider.maxValue > 0 ? slider.maxValue : 1);
    }

    public void Init()
    {
        Off();
    }

    public void SetSEVlolume(float value)
    {
        if (ContinuousController.instance != null)
        {
            ContinuousController.instance.SetSEVolume(value);
        }
    }

    bool _notPlaySample = false;

    public void PlaySampleSE(float _)
    {
        if (Opening.instance != null)
        {
            if (!_notPlaySample)
            {
                if (Opening.instance != null)
                {
                    Opening.instance.PlayDecisionSE();
                }

                else if (GManager.instance != null)
                {
                    GManager.instance.PlayDecisionSE();
                }

                ContinuousController.instance.StartCoroutine(TimerCoroutine());
            }
        }
    }

    IEnumerator TimerCoroutine()
    {
        _notPlaySample = true;

        yield return _waitForSeconds0_15;

        _notPlaySample = false;
    }

    public void SetBGMVlolume(float value)
    {
        if (ContinuousController.instance != null)
        {
            ContinuousController.instance.SetBGMVolume(value);
        }
    }
}