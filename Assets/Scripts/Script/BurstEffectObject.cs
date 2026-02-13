using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BurstEffectObject : EvolutionEffectObject
{
    [SerializeField] Image[] jogressEvoRootImages;
    [SerializeField] TextMeshProUGUI evolutionText;
    [SerializeField] AudioClip changeTextSE;
    [SerializeField] AudioClip lightningSE;
    [SerializeField] Material normalMaterial;
    [SerializeField] Material lightMaterial;
    public override IEnumerator EvolutionEffectAnimation(CardSource cardSource, CardSource[] jogressEvoRoots = null, string message = "")
    {
        if (ContinuousController.instance != null)
        {
            if (!ContinuousController.instance.showCutInAnimation)
            {
                yield break;
            }
        }

        animTime = 2.7f;

        yield return ContinuousController.instance.StartCoroutine(base.EvolutionEffectAnimation(cardSource));
    }

    public void Set_ULTIMATEEVOLUTION()
    {
        evolutionText.text = "ULTIMATE\nEVOLUTION";
    }

    public void Set_BURSTEVOLUTION()
    {
        evolutionText.text = "BURST\nEVOLUTION";
    }

    public void SetMaterialNormalGlow()
    {
        evolutionText.fontSharedMaterial = normalMaterial;
    }

    public void SetMaterialLightGlow()
    {
        evolutionText.fontSharedMaterial = lightMaterial;
    }

    public void PlayChangeTextSE()
    {
        if (ContinuousController.instance != null)
        {
            ContinuousController.instance.PlaySE(changeTextSE);
        }
    }

    public void PlayLightningSE()
    {
        if (ContinuousController.instance != null)
        {
            ContinuousController.instance.PlaySE(lightningSE);
        }
    }
}
