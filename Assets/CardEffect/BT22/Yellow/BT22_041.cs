using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Kentaurosmon
namespace DCGO.CardEffects.BT22
{
    public class BT22_041 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.EqualsCardName("Chirinmon")) return true;
                    if (targetPermanent.TopCard.IsLevel5 && targetPermanent.TopCard.HasCSTraits) return true;
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Reduce Cost

            if (timing == EffectTiming.BeforePayCost)
            {
                int securityCount = card.Owner.SecurityCards.Count + card.Owner.Enemy.SecurityCards.Count;

                bool Condition()
                {
                    return !CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && securityCount <= 6;
                }

                cardEffects.Add(
                    CardEffectFactory.ChangePlayCostStaticEffect(
                        changeValue: -6,
                        permanentCondition: null,
                        isInheritedEffect: false,
                        card: card,
                        condition: Condition,
                        setFixedCost: false));
            }

            #endregion

            #region Blocker

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Barrier

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(false, card, null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 yellow card from hand as top secuity card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may place 1 yellow card from your hand as your top security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, IsYellowCard);
                }

                bool IsYellowCard(CardSource cardSource) => cardSource.CardColors.Contains(CardColor.Yellow);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, IsYellowCard))
                    {
                        CardSource selectedCard = null;

                        #region Select Hand Card

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, IsYellowCard));
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsYellowCard,
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

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        selectHandEffect.SetUpCustomMessage("Select 1 card to place on top of security.", "The opponent is selecting 1 card to place on top of security.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                        #endregion

                        if (selectedCard != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(selectedCard));
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateRecoveryEffect(selectedCard.Owner));
                            yield return ContinuousController.instance.StartCoroutine(new IAddSecurity(selectedCard.Owner).AddSecurity());
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 yellow card from hand as top secuity card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may place 1 yellow card from your hand as your top security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, IsYellowCard);
                }

                bool IsYellowCard(CardSource cardSource) => cardSource.CardColors.Contains(CardColor.Yellow);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, IsYellowCard))
                    {
                        CardSource selectedCard = null;

                        #region Select Hand Card

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, IsYellowCard));
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsYellowCard,
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

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        selectHandEffect.SetUpCustomMessage("Select 1 card to place on top of security.", "The opponent is selecting 1 card to place on top of security.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                        #endregion

                        if (selectedCard != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(selectedCard));
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateRecoveryEffect(selectedCard.Owner));
                            yield return ContinuousController.instance.StartCoroutine(new IAddSecurity(selectedCard.Owner).AddSecurity());
                        }
                    }
                }
            }

            #endregion

            #region All turns - OPT

            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing top security, unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT22_041_Untap");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon suspends, by trashing your top security card, it unsuspends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenSelfPermanentSuspends(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.Owner.SecurityCards.Any();
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new IDestroySecurity(card.Owner, 1, activateClass, true).DestroySecurity());

                    yield return ContinuousController.instance.StartCoroutine(
                        new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}