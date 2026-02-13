using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Trigger effect of [Save]
    public static ActivateClass SaveEffect(CardSource card)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Save", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, DataBase.SaveEffectDiscription());

        bool CanSelectPermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
            {
                if (permanent.IsTamer)
                {
                    if (!permanent.IsToken)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanActivateSave(hashtable, CanSelectPermanentCondition);
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            return CardEffectCommons.SaveProcess(_hashtable, activateClass, card, CanSelectPermanentCondition);
        }

        return activateClass;
    }
    #endregion
}