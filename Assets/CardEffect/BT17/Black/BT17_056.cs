using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT17
{
    public class BT17_056 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region All Turns - Attack Target Switched
            if (timing == EffectTiming.OnAttackTargetChanged)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal 3, Place 1 [Parasitemon] or 1 black level 5 or lower Digimon as bottom digivolution source", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("Reveal_BT17_056");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When an attack target is switched, reveal the top 3 cards of your deck. Place 1 [Parasitemon] or 1 black level 5 or lower Digimon card among them as this Digimon's bottom digivolution card. Trash the rest.";
                }
                
                bool IsSelectableSource(CardSource source)
                {
                    if(source.EqualsCardName("Parasitemon"))
                        return true;

                    if (source.IsDigimon)
                    {
                        if (source.CardColors.Contains(CardColor.Black))
                        {
                            if (source.HasLevel && source.Level <= 5)
                                return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:IsSelectableSource,
                            message: "Select 1 [Parasitemon] or Black Level 5 or lower Digimon.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: SelectedCardToSources),
                        },
                        remainingCardsPlace: RemainingCardsPlace.Trash,
                        activateClass: activateClass,
                        canNoSelect: false
                    ));

                    IEnumerator SelectedCardToSources (CardSource source)
                    {
                        if(source != null)
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource> { source }, activateClass));
                    }
                }
            }
            #endregion

            #region All Turns - Digivolution Source Added
            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [GroundLocomon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When one of your Digimon's effects adds to this Digimon's digivolution cards, this Digimon may digivolve into [GroundLocomon] in the hand without paying the cost.";
                }

                bool IsGroundLocomon(CardSource source)
                {
                    if (source.EqualsCardName("GroundLocomon"))
                    {
                        if (source.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, true, activateClass, SelectCardEffect.Root.Hand))
                            return true;
                    }

                    return false;
                }

                bool IsThisDigimon(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool IsMyDigimonEffect(ICardEffect effect)
                {
                    if (effect.EffectSourceCard != null)
                    {
                        if (effect.EffectSourceCard.Owner == card.Owner)
                        {
                            return effect.EffectSourceCard.IsDigimon;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnAddDigivolutionCard(
                            hashtable: hashtable,
                            permanentCondition: IsThisDigimon,
                            cardEffectCondition: IsMyDigimonEffect,
                            cardCondition: null))
                        {
                            return CardEffectCommons.HasMatchConditionOwnersHand(card, IsGroundLocomon);
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                    targetPermanent: card.PermanentOfThisCard(),
                    cardCondition: IsGroundLocomon,
                    payCost: false,
                    reduceCostTuple: null,
                    fixedCostTuple: null,
                    ignoreDigivolutionRequirementFixedCost: -1,
                    isHand: true,
                    activateClass: activateClass,
                    successProcess: null));
                }
            }
            #endregion

            #region All Turns - ESS
            if (timing == EffectTiming.OnCounterTiming)
            {
                bool condition()
                {
                    if(card.PermanentOfThisCard().TopCard != card)
                        return card.PermanentOfThisCard().TopCard.ContainsTraits("Machine");

                    return false;
                }

                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: condition));
            }
            #endregion

            return cardEffects;
        }
    }
}