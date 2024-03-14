using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Trigger effect of [Material Save]
    public static ActivateClass MaterialSaveEffect(CardSource card, int materialSaveCount)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect($"Material Save {materialSaveCount}", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
        activateClass.SetHashString($"MaterialSave_{card.CardID}");

        string EffectDiscription()
        {
            return $"<Material Save {materialSaveCount}> (When this Digimon is deleted, you may place {materialSaveCount} card{Utils.PluralFormSuffix(materialSaveCount)} in this Digimon's DigiXros conditions from this Digimon's digivolution cards under 1 of your Tamers.)";
        }

        bool CanSelectCardCondition(CardSource cardSource)
        {
            return card.IsContainDigiXrosCondition(cardSource);
        }

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
            if (CardEffectCommons.IsExistOnBattleArea(card))
            {
                if (CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card))
                {
                    return true;
                }
            }

            return false;
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanActivateMaterialSave(card, CanSelectCardCondition, CanSelectPermanentCondition);
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            return CardEffectCommons.MaterialSaveProcess(_hashtable, activateClass, card, CanSelectCardCondition, CanSelectPermanentCondition, materialSaveCount);
        }

        return activateClass;
    }
    #endregion
}