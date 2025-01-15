using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX8
{
    public class EX8_015 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("WarGrowlmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 1,
                    ignoreDigivolutionRequirement: false,
                    card: card, condition: null)
                );
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Can't be returned to the hand or deck, +3000 DP, and delete 1 Digimon with 10000 DP or less.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Until the end of your opponent's turn, this Digimon can't be returned to the hand or deck and it gets +3000 DP. Then, if [WarGrowlmon]/[X Antibody] is in this Digimon's digivolution cards, delete 1 of your opponent's Digimon with 10000 DP or less.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(10000, activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateEffect(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => (cardSource.EqualsCardName("WarGrowlmon") || cardSource.EqualsCardName("X Antibody"))) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool CardEffectCondition(ICardEffect cardEffect)
                    {
                        return CardEffectCommons.IsOpponentEffect(cardEffect, card);
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotReturnToHand(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardEffectCondition: CardEffectCondition,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass,
                        effectName: "Can't return to hand by opponent's effects"));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotReturnToDeck(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardEffectCondition: CardEffectCondition,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass,
                        effectName: "Can't return to deck by opponent's effects"));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: card.PermanentOfThisCard(),
                            changeValue: 3000,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));

                    if (CanActivateEffect(hashtable))
                    {
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
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}