using System;
using System.Collections;
using System.Collections.Generic;

// Waspmon
namespace DCGO.CardEffects.BT23
{
    public class BT23_042 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.IsLevel3
                        && (targetPermanent.TopCard.HasCSTraits || targetPermanent.TopCard.HasRoyalBaseTraits);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion Alternative Digivolution Condition

            #region All Turns - Security

            if (timing == EffectTiming.None)
            {
                string EffectDiscription()
                {
                    return "(Security) [All Turns] All of your [Royal Base] trait Digimon get +1000 DP.";
                }

                bool Condition()
                {
                    return CardEffectCommons.IsExistInSecurity(card, false);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.EqualsTraits("Royal Base"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                permanentCondition: PermanentCondition,
                changeValue: 1000,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                effectName: EffectDiscription));
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 tamer with [Royal Base] in text", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If you have 1 or fewer Tamers, you may play 1 Tamer card with [Royal Base] in its text from your hand without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.OwnerHas1OrLessTamers(card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsTamer
                        && cardSource.HasText("Royal Base");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                    {
                        CardSource selectCard = null;
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, CanSelectCardCondition));

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectCard = cardSource;
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                        if (selectCard != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            new List<CardSource>() { selectCard },
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true));
                        }
                    }
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 1000, isInheritedEffect: true, card: card, condition: Condition));
            }

            #endregion

            return cardEffects;
        }
    }
}