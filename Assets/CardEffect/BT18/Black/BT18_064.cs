using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT18
{
    public class BT18_064 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolve Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Sephirothmon");
                }

                static bool TamerCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardColors.Contains(CardColor.Black) &&
                           targetPermanent.IsTamer;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: TamerCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: true, card: card, condition: null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Can't be returned to the hand or deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Until the end of your opponent's turn, your opponent's effects can't return this Digimon to the hand or deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if(CardEffectCommons.IsExistOnBattleArea(card))
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
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Can't be returned to the hand or deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Until the end of your opponent's turn, your opponent's effects can't return this Digimon to the hand or deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
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
                }
            }
            #endregion

            #region ESS - Opponent's Turn
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 2000, isInheritedEffect: true, card: card, condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}