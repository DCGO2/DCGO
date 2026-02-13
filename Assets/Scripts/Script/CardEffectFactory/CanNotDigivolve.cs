using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that oneself can't digivolve
    public static CanNotDigivolveClass CanNotDigivolveStaticSelfEffect(
        Func<CardSource, bool> cardCondition,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition,
        string effectName)
    {
        bool CanUseCondition()
        {
            if (CardEffectCommons.IsExistOnField(card))
            {
                if (condition == null || condition())
                {
                    return true;
                }
            }

            return false;
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnField(permanent))
            {
                if (permanent == card.PermanentOfThisCard())
                {
                    return true;
                }
            }

            return false;
        }

        return CanNotDigivolveStaticEffect(
            permanentCondition: PermanentCondition,
            cardCondition: cardCondition,
            isInheritedEffect: isInheritedEffect,
            card: card,
            condition: CanUseCondition,
            effectName: effectName);
    }
    #endregion

    #region Static effect that can't digivolve
    public static CanNotDigivolveClass CanNotDigivolveStaticEffect(
        Func<Permanent, bool> permanentCondition,
        Func<CardSource, bool> cardCondition,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition,
        string effectName)
    {
        CanNotDigivolveClass canNotEvolveClass = new CanNotDigivolveClass();
        canNotEvolveClass.SetUpICardEffect(effectName, CanUseCondition, card);
        canNotEvolveClass.SetUpCanNotEvolveClass(permanentCondition: PermanentCondition, cardCondition: CardCondition);

        if (isInheritedEffect)
        {
            canNotEvolveClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnField(permanent))
            {
                if (!permanent.TopCard.CanNotBeAffected(canNotEvolveClass))
                {
                    if (permanentCondition == null || permanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool CardCondition(CardSource cardSource)
        {
            if (cardSource != null)
            {
                if (cardCondition == null || cardCondition(cardSource))
                {
                    return true;
                }
            }

            return false;
        }

        return canNotEvolveClass;
    }
    #endregion
}