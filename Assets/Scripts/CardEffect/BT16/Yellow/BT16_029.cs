using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class BT16_029 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 2)
                    {
                        if (targetPermanent.TopCard.CardTraits.Contains("Light Fang"))
                        {
                            return true;
                        }

                        if (targetPermanent.TopCard.CardTraits.Contains("LightFang"))
                        {
                            return true;
                        }

                        if (targetPermanent.TopCard.CardTraits.Contains("Night Claw"))
                        {
                            return true;
                        }

                        if (targetPermanent.TopCard.CardTraits.Contains("NightClaw"))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Reveal the top 3 cards of your deck. Add 1 card with the [Light Fang] trait and 1 card with the [Night Claw] trait or 2 colors among them to the hand. Return the rest to the bottom of the deck.";
                }

                bool NightClawCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("Night Claw"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("NightClaw"))
                    {
                        return true;
                    }

                    if (cardSource.CardColors.Count == 2)
                    {
                        return true;
                    }

                    return false;
                }

                bool LightFangCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("Light Fang"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("LightFang"))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:LightFangCondition,
                            message: "Select 1 card with the [Light Fang] trait.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:NightClawCondition,
                            message: "Select 1 card with the [Night Claw] trait or 2 colors.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass
                    ));
                }
            }

            #endregion

            #region Inherit

            if (timing == EffectTiming.None)
            {
                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.Owner == card.Owner.Enemy;
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSecurityDigimonCardDPStaticEffect(
                    cardCondition: CardCondition,
                    changeValue: -3000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition,
                    effectName: "Opponent's Security Digimon gains DP -3000"));
            }

            #endregion

            return cardEffects;
        }
    }
}