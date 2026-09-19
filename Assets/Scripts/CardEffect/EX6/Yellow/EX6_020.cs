using System.Collections;
using System.Collections.Generic;
using System;

namespace DCGO.CardEffects.EX6
{
    public class EX6_020 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region On Play/When Digivolving Shared
            
            bool CanSelectDigimonCardSharedCondition(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.CardTraits.Contains("Angel") ||
                        cardSource.CardTraits.Contains("Archangel") ||
                        cardSource.CardTraits.Contains("Fallen Angel") ||
                        cardSource.CardTraits.Contains("FallenAngel"))
                    {
                        return true;
                    }
                }
                
                return false;
            }
            
            bool CanSelectTamerCardSharedCondition(CardSource cardSource)
            {
                if (cardSource.IsTamer)
                {
                    if (cardSource.CardNames.Contains("Mirei Mikagura") ||
                        cardSource.CardNames.Contains("MireiMikagura"))
                    {
                        return true;
                    }
                }
                
                return false;
            }
            
            bool CanActivateSharedCondition(Hashtable hashtable)
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
            
            #endregion
            
            #region On Play
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                string EffectDescription()
                {
                    return
                        "[On Play] Reveal the top 3 cards of your deck. Add 1 card with the [Angel]/[Archangel]/[Fallen Angel] trait and 1 [Mirei Mikagura] among them to the hand. Return the rest to the bottom of the deck.";
                }


                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                            revealCount: 3,
                            simplifiedSelectCardConditions:
                            new[]
                            {
                                new SimplifiedSelectCardConditionClass(
                                    canTargetCondition: CanSelectDigimonCardSharedCondition,
                                    message: "Select 1 Digimon card with the [Angel]/[Archangel]/[Fallen Angel] trait.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null),
                                new SimplifiedSelectCardConditionClass(
                                    canTargetCondition: CanSelectTamerCardSharedCondition,
                                    message: "Select 1 Tamer card with [Mirei Mikagura] in its name.",
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
            
            #region When Digivolving
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                string EffectDescription()
                {
                    return
                        "[When Digivolving] Reveal the top 3 cards of your deck. Add 1 card with the [Angel]/[Archangel]/[Fallen Angel] trait and 1 [Mirei Mikagura] among them to the hand. Return the rest to the bottom of the deck.";
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                            revealCount: 3,
                            simplifiedSelectCardConditions:
                            new[]
                            {
                                new SimplifiedSelectCardConditionClass(
                                    canTargetCondition: CanSelectDigimonCardSharedCondition,
                                    message: "Select 1 Digimon card with the [Angel]/[Archangel]/[Fallen Angel] trait.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null),
                                new SimplifiedSelectCardConditionClass(
                                    canTargetCondition: CanSelectTamerCardSharedCondition,
                                    message: "Select 1 Tamer card with [Mirei Mikagura] in its name.",
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
            
            #region When Attacking - ESS
            
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DP -2000", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false,
                    EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("DP-2000_EX6-020");
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return "[When Attacking][Once Per Turn] 1 of your opponent's Digimon gets -2000 DP for the turn.";
                }
                
                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
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
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1,
                            CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));
                        
                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();
                        
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
                        
                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -2000.",
                            "The opponent is selecting 1 Digimon that will get DP -2000.");
                        
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        
                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: permanent,
                                changeValue: -2000,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                        }
                    }
                }
            }
            
            #endregion
            
            return cardEffects;
        }
    }
}