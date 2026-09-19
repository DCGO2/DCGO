using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActivateClass : ICardEffect, ActivateICardEffect
{
    public Permanent PermanentWhenTriggered { get; set; } = null;
    public CardSource TopCardWhenTriggered { get; set; } = null;
    Func<Hashtable, IEnumerator> _activateCoroutine { get; set; } = null;
    public void SetUpActivateClass(Func<Hashtable, bool> canActivateCondition,
    Func<Hashtable, IEnumerator> activateCoroutine,
    int maxCountPerTurn,
    bool isOptional,
    string effectDiscription)
    {
        SetCanActivateCondition(canActivateCondition);
        SetMaxCountPerTurn(maxCountPerTurn);
        SetIsOptional(isOptional);
        SetEffectDiscription(DataBase.ReplaceToASCII(effectDiscription));
        _activateCoroutine = activateCoroutine;
    }
    public IEnumerator Activate(Hashtable hashtable)
    {
        if (_activateCoroutine != null)
        {
            yield return ContinuousController.instance.StartCoroutine(_activateCoroutine(hashtable));
        }
    }
}