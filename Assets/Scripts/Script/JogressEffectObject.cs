using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class JogressEffectObject : EvolutionEffectObject
{
    [SerializeField] Image[] jogressEvoRootImages;
    public override IEnumerator EvolutionEffectAnimation(CardSource cardSource, CardSource[] jogressEvoRoots, string message = "")
    {
        if (ContinuousController.instance != null)
        {
            if (!ContinuousController.instance.showCutInAnimation)
            {
                yield break;
            }
        }

        animTime = 1.65f;

        for (int i = 0; i < jogressEvoRoots.Length; i++)
        {
            if (i < jogressEvoRootImages.Length)
            {
                jogressEvoRootImages[i].sprite = jogressEvoRoots[i].CardSprite;
            }
        }

        yield return ContinuousController.instance.StartCoroutine(base.EvolutionEffectAnimation(cardSource, jogressEvoRoots));
    }
}
