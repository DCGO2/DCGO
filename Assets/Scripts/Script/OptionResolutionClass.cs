using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;

public class OptionResolutionClass : ICardEffect, IOptionResolutionEffect
{
    public void SetUpOptionResolutionClass(IEnumerator resolutionCoroutine, Func<bool> resolutionCondition = null)
    {
        ResolutionCoroutine = resolutionCoroutine;
        ResolutionCondition = resolutionCondition;
    }

    Func<bool> ResolutionCondition { get; set; }
    IEnumerator ResolutionCoroutine { get; set; }

    public bool CanResolve()
    {
        return ResolutionCondition == null || ResolutionCondition();
    }

    public IEnumerator Resolve()
    {
        if (CanResolve())
        {
            if (ResolutionCoroutine != null)
            {
                yield return ContinuousController.instance.StartCoroutine(ResolutionCoroutine);
            }
        }
    }
}