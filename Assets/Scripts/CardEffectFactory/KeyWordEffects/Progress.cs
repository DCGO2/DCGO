using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect of [Progress] on oneself
    public static ActivateClass ProgressSelfStaticEffect(bool isInheritedEffect, CardSource card, Func<bool> condition)
    {
        Permanent targetPermanent = card.PermanentOfThisCard();

        bool PermanentCondition(Permanent permanent) => permanent == card.PermanentOfThisCard();

        bool CanUseCondition()
        {
            if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
            {
                if (GManager.instance.attackProcess.IsAttacking)
                {
                    if (GManager.instance.attackProcess.AttackingPermanent == card.PermanentOfThisCard())
                    {
                        if (condition == null || condition())
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        return ProgressStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: isInheritedEffect, card: card, condition: CanUseCondition);
    }
    #endregion

    #region Static effect of [Progress]
    public static ActivateClass ProgressStaticEffect(
        Func<Permanent, bool> permanentCondition,
        bool isInheritedEffect,
        CardSource card,
        Func<bool> condition)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Progress", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
        activateClass.SetIsInheritedEffect(isInheritedEffect);
        activateClass.SetIsBackgroundProcess(true);

        string EffectDiscription()
        {
            return DataBase.ProgressEffectDiscription();
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
            {
                if (permanentCondition == null || permanentCondition(permanent))
                {
                    return true;
                }
            }

            return false;
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            if (condition == null || condition())
            {
                if(PermanentCondition(card.PermanentOfThisCard()))
                    return true;
            }

            return false;
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return true;
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            return CardEffectCommons.ProgressProcess(card, activateClass);
        }

        return activateClass;
    }
    #endregion
}