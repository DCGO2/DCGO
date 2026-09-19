using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;

public class OptionResolutionClass : ICardEffect, IOptionResolutionEffect
{
    public void SetUpOptionResolutionClass(Func<CardSource, IEnumerator> resolutionCoroutine, Func<CardSource, bool> resolutionCondition = null)
    {
        ResolutionCoroutine = resolutionCoroutine;
        ResolutionCondition = resolutionCondition;
    }

    Func<CardSource, bool> ResolutionCondition { get; set; }
    Func<CardSource, IEnumerator> ResolutionCoroutine { get; set; }

    public bool CanResolve(CardSource optionCard)
    {
        return ResolutionCondition == null || ResolutionCondition(optionCard);
    }

    public IEnumerator Resolve(CardSource optionCard)
    {
        if (CanResolve(optionCard))
        {
            if (ResolutionCoroutine != null)
            {
                yield return ContinuousController.instance.StartCoroutine(ResolutionCoroutine(optionCard));
            }
        }
    }
}