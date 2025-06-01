using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Trigger effect of Link
    /// <summary>
    /// Linking 1 digimon in hand/field to another digimon
    /// </summary>
    /// <param name="CardSource">card being linked</param>
    /// <param name="int">cost for link</param>
    /// <param name="int">DP buff</param>
    /// <param name="Func(bool)">condition for you to be able to link</param>
    /// <param name="Func(Permanent,bool)">Any permaent condtion for you to link to</param>
    /// <author>Mike Bunch</author>
    public static ActivateClass LinkEffect(CardSource card, Func<bool> condition = null)
    {
        if (card == null) return null;
        if (!CardEffectCommons.IsOwnerTurn(card)) return null;
        if (!CardEffectCommons.IsExistOnHand(card) && !CardEffectCommons.IsExistOnBattleAreaDigimon(card)) return null;
        if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition)) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect($"Link (Cost: {card.linkCondition.cost})", CanUseCondition, card);
        activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, DataBase.LinkEffectDiscription());

        bool CanSelectPermanentCondition(Permanent permanent)
        {
            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
            {
                if(permanent != card.PermanentOfThisCard())
                {
                    if (permanent.IsDigimon)
                    {
                        if (card.linkCondition == null || card.linkCondition.digimonCondition(permanent))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.IsExistOnHand(card) || (CardEffectCommons.IsExistOnBattleAreaDigimon(card) && !card.IsLinked))
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
                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(-card.linkCondition.cost, activateClass));

                if(CardEffectCommons.IsExistOnHand(card))
                    yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddLinkCard(card, activateClass));
                else
                    yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToLinkCards(new List<Permanent[]>() { new Permanent[] { card.PermanentOfThisCard(), selectedPermanent } }, activateClass).PlacePermanentToLinkCards());
            }
        }

        return activateClass;
    }
    #endregion
}