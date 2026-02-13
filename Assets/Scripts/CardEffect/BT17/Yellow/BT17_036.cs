using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT17
{
    public class BT17_036 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolve Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Pulsemon") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash a security to prevent this Digimon from leaving Battle Area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("TrashSecurityToStay_BT17_036");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon would leave the battle area by an opponent's effect, by trashing the top card of your security stack, prevent it.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if(card.Owner.SecurityCards.Count > 0)
                        {
                            if (CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card))
                            {
                                if (CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOpponentEffect(cardEffect, card)))
                                {
                                    return true;
                                }
                            }
                        }                        
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;   
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        card.Owner,
                        1,
                        activateClass,
                        true).DestroySecurity());


                        Permanent thisCardPermanent = card.PermanentOfThisCard();

                        thisCardPermanent.willBeRemoveField = false;

                        thisCardPermanent.HideDeleteEffect();
                        thisCardPermanent.HideHandBounceEffect();
                        thisCardPermanent.HideDeckBounceEffect();
                        thisCardPermanent.HideWillRemoveFieldEffect();

                        yield return null;
                    }
                }
            }

            if (timing == EffectTiming.OnDiscardSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If a card is trashed from security and there's a Tamer in the digivolution cards, this Digimon digivolves for free.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("Digivolve_BT17_036");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once per turn] When a card is trashed from your security stack by an effect, this Digimon with [Leon Alexander] in its digivolution cards may digivolve into a Digimon card with [Pulsemon] in its text in the hand without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.HasText("Pulsemon") && cardSource.IsDigimon)
                    {
                        return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerOnTrashSecurity(hashtable, cardEffect => cardEffect != null, cardSource => cardSource.Owner == card.Owner))
                        {
                            if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => (cardSource.EqualsCardName("Leon Alexander") && cardSource.IsTamer)) >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: CanSelectCardCondition,
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

            #region Inherit
            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon by trashing the top card of your security.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Inherit_BT17_036");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Attack] [Once per turn] If this Digimon has [Pulsemon] in its text, by trashing the top card of your security stack, unsuspend this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnEndAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.HasText("Pulsemon"))
                        {
                            if (card.Owner.SecurityCards.Count >= 1)
                            {
                                if (CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard()))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (permanent == card.PermanentOfThisCard())
                    {
                        if (card.PermanentOfThisCard().TopCard.HasText("Pulsemon"))
                        {
                            if (CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard()))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                    player: card.Owner,
                    destroySecurityCount: 1,
                    cardEffect: activateClass,
                    fromTop: true).DestroySecurity());

                    if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}