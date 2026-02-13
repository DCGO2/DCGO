using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT15
{
    public class BT15_067 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.CardTraits.Contains("DigiPolice") || targetPermanent.TopCard.HasXAntibodyTraits)
                    {
                        if (targetPermanent.TopCard.HasLevel)
                        {
                            if (targetPermanent.Level == 5)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Digimon card with the [Beast Dragon] or [DigiPolice] trait with 5000DP or less from your hand.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("PlayDigimon_BT15_067");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns][Once per turn] When this Digimon becomes suspeneded, you may play 1 Digimon card with the [Beast Dragon] or [DigiPolice] trait and 5000DP or less from your hand without paying the cost";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, (permanent) => permanent == card.PermanentOfThisCard()))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        if (cardSource.CardDP <= 5000)
                        {
                            if (cardSource.IsDigimon)
                            {
                                if (cardSource.HasBeastDragonTraits)
                                {
                                    return true;
                                }

                                if (cardSource.HasDigiPoliceTraits)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

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

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 of your opponent's suspended Digimon or Tamers to the bottom of the deck.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If a Tamer card with the [DigiPolice] trait is in this Digimon's digivolution cards, return 1 of your opponent's suspended Digimon or Tamers to the bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => (cardSource.CardTraits.Contains("DigiPolice") && cardSource.IsTamer)) >= 1)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsSuspended)
                        {
                            return true;
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
                    if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                    {
                        int maxCount = Math.Min(1, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

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
                            mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer that will return to the bottom of deck.", "The opponent is selecting 1 Digimon that will return to the bottom of deck.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            return cardEffects;
        }
    }
}