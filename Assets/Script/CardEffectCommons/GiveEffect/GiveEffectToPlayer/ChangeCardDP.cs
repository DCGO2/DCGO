using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Player gains effect to change security digimon card's DP
    public static IEnumerator ChangeSecurityDigimonCardDPPlayerEffect(Func<CardSource, bool> cardCondition, int changeValue, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;
        if (changeValue == 0) yield break;

        bool isUpValue = changeValue > 0;
        string effectName = isUpValue ? $"Security Digimon gains DP +{changeValue}" : $"Security Digimon gains DP {changeValue}";

        CardSource card = activateClass.EffectSourceCard;

        bool Condition()
        {
            return true;
        }

        bool CardCondition(CardSource cardSource)
        {
            return cardCondition == null || cardCondition(cardSource);
        }

        ChangeCardDPClass changeDPClass = CardEffectFactory.ChangeSecurityDigimonCardDPStaticEffect(
            cardCondition: CardCondition,
            changeValue: changeValue,
            isInheritedEffect: false,
            card: card,
            condition: Condition,
            effectName: effectName);

        AddEffectToPlayer(effectDuration: effectDuration, card: card, cardEffect: changeDPClass, timing: EffectTiming.None);
    }
    #endregion
}