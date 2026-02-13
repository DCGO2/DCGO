using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT18
{
    public class BT18_010 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] Reveal the top 3 cards of your deck. Add 1 card with the [Hybrid]/[Ten Warriors] trait and 1 red Tamer card with an inherited effect among them to the hand. Return the rest to the bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanSelectHybridCardCondition(CardSource cardSource)
                {
                    return cardSource.HasHybridTenWarriorsTraits;
                }

                bool CanSelectTamerCardCondition(CardSource cardSource)
                {
                    return cardSource.CardColors.Contains(CardColor.Red) && cardSource.IsTamer && cardSource.HasInheritedEffect;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.Owner.LibraryCards.Count >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                            revealCount: 3,
                            simplifiedSelectCardConditions:
                            new SimplifiedSelectCardConditionClass[]
                            {
                                new(
                                    canTargetCondition: CanSelectHybridCardCondition,
                                    message: "Select 1 card with the [Hybrid]/[Ten Warriors] trait.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null),
                                new(
                                    canTargetCondition: CanSelectTamerCardCondition,
                                    message: "Select 1 red Tamer card with an inherited effect.",
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

            #region Your Turn

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("Memory+1_BT18_010");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Your Turn] (Once Per Turn) When any of your Digimon or Tamers digivolve into Digimon with the [Hybrid]/[Ten Warriors] trait, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card) && CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, DigivolveToPermanentCondition))
                        {
                            List<CardSource> evoRoots =
                                CardEffectCommons.GetDigivolutionRootsFromEnterFieldHashtable(hashtable, DigivolveToPermanentCondition);
                            
                            return evoRoots != null && evoRoots.Some(DigivolveFromCardCondition);
                        }
                    }

                    return false;
                }
                
                bool DigivolveFromCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon || cardSource.IsTamer;
                }
                
                bool DigivolveToPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.HasHybridTenWarriorsTraits;
                }
                
                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}