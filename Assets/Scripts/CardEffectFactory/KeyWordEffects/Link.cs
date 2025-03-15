using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Trigger effect of Link
    public static ActivateClass LinkEffect(CardSource card, Func<bool> condition, Func<Permanent, bool> digimonCondition)
    {
        if (card == null) return null;
        if (!CardEffectCommons.IsExistOnHand(card) && !CardEffectCommons.IsExistOnBattleAreaDigimon(card)) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Link", CanUseCondition, card);
        activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, DataBase.BlastDigivolveEffectDiscription());

        bool CanSelectPermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
            {
                if (digimonCondition == null || digimonCondition(permanent))
                {
                    return true;
                }
            }

            return false;
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.IsExistOnHand(card) || CardEffectCommons.IsExistOnBattleAreaDigimon(card))
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    if (condition == null || condition())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            Permanent selectedPermanent = null;

            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

            selectPermanentEffect.SetUp(
                selectPlayer: card.Owner,
                canTargetCondition: CanSelectPermanentCondition,
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                maxCount: maxCount,
                canNoSelect: false,
                canEndNotMax: false,
                selectPermanentCoroutine: SelectPermanentCoroutine,
                afterSelectPermanentCoroutine: null,
                mode: SelectPermanentEffect.Mode.Custom,
                cardEffect: activateClass);

            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to link.", "The opponent is selecting 1 Digimon to link.");

            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

            IEnumerator SelectPermanentCoroutine(Permanent permanent)
            {
                selectedPermanent = permanent;

                yield return null;
            }

            if (selectedPermanent != null)
            {
                if (CardEffectCommons.IsExistOnHand(card))
                    yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddLinkedCard(card, activateClass));
                else
                    yield return ContinuousController.instance.StartCoroutine(new ILinkPermanentToPermanent(new List<Permanent[]>() { new Permanent[] { card.PermanentOfThisCard(), selectedPermanent } }, activateClass).LinkPermanent());
            }
        }

        return activateClass;
    }
    #endregion
}