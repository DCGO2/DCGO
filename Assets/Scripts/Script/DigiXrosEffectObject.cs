using System.Collections;
using UnityEngine;
using DG.Tweening;

public class DigiXrosEffectObject : EvolutionEffectObject
{
    [SerializeField] GameObject digiXrosAnimParent;
    public override IEnumerator EvolutionEffectAnimation(CardSource cardSource, CardSource[] jogressEvoRoots = null, string message = "")
    {
        if (ContinuousController.instance != null)
        {
            if (!ContinuousController.instance.showCutInAnimation)
            {
                animTime = 2f;

                yield break;
            }
        }

        yield return ContinuousController.instance.StartCoroutine(base.EvolutionEffectAnimation(cardSource, jogressEvoRoots));
    }

    public void Shake()
    {
        StartCoroutine(ShakeIEnumerator());
    }

    public void PlaySlashSE()
    {
        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BattleSE);
    }

    IEnumerator ShakeIEnumerator()
    {
        bool end = false;

        float shakeTime = 0.3f;

        Sequence sequence = DOTween.Sequence();

        sequence
            .Append(digiXrosAnimParent.transform.DOShakePosition(shakeTime, strength: 16f, vibrato: 50, fadeOut: true))
            .AppendCallback(() => end = true);

        sequence.Play();

        yield return new WaitWhile(() => !end);
        end = false;

        sequence.Kill();
    }
}
