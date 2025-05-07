using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT19
{
    public class BT19_037 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Blast Digivolve
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }
            #endregion

            #region On Play 
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If it's your turn, you may use 1 single-color Option", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] If it's your turn, you may use 1 single-color Option card with a cost of 5 or less from your hand without paying the cost. If it's your opponent's turn, 1 of their Digimon gains <Security Attack -1> and can't activate  effects for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanSelectOptionCard(CardSource cardSource)
                {
                    if (cardSource.IsOption)
                    {
                        if (cardSource.CardColors.Count == 1 && cardSource.GetCostItself <= 5)
                        {
                            if (!cardSource.CanNotPlayThisOption)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Count(CanSelectOptionCard) >= 1 && CardEffectCommons.IsOwnerTurn(card))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();
                
                        int maxCount = 1;
                
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                
                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOptionCard,
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
                
                        selectHandEffect.SetUpCustomMessage("Select 1 option card to use.", "The opponent is selecting 1 option card to use.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Used Card");
                
                        yield return StartCoroutine(selectHandEffect.Activate());
                
                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                
                            yield return null;
                        }
                
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayOptionCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            root: SelectCardEffect.Root.Hand));
                    }

                    if (CardEffectCommons.IsOpponentTurn(card))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(PermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: PermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get Security Attack -1 and effects", "The opponent is selecting 1 Digimon that will get Security Attack -1.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(targetPermanent: permanent, changeValue: -1, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                DisableEffectClass invalidationClass = new DisableEffectClass();
                                invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", CanUseCondition1, card);
                                invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                                selectedPermanent.UntilEachTurnEndEffects.Add((_timing) => invalidationClass);

                                bool CanUseCondition1(Hashtable hashtable)
                                {
                                    return true;
                                }

                                bool InvalidateCondition(ICardEffect cardEffect)
                                {
                                    if (selectedPermanent.TopCard != null)
                                    {
                                        if (cardEffect != null)
                                        {
                                            if (cardEffect.EffectSourceCard != null)
                                            {
                                                if (isExistOnField(cardEffect.EffectSourceCard))
                                                {
                                                    if (cardEffect.EffectSourceCard.PermanentOfThisCard() == selectedPermanent)
                                                    {
                                                        if (cardEffect.IsWhenDigivolving)
                                                        {
                                                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                                            {
                                                                return true;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region When Digivolving 
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If it's your turn, you may use 1 single-color Option", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] If it's your turn, you may use 1 single-color Option card with a cost of 5 or less from your hand without paying the cost. If it's your opponent's turn, 1 of their Digimon gains <Security Attack -1> and can't activate  effects for the turn.";
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

                bool CanSelectOptionCard(CardSource cardSource)
                {
                    if (cardSource.IsOption)
                    {
                        if (cardSource.CardColors.Count == 1 && cardSource.GetCostItself <= 5)
                        {
                            if (!cardSource.CanNotPlayThisOption)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Count(CanSelectOptionCard) >= 1 && CardEffectCommons.IsOwnerTurn(card))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOptionCard,
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

                        selectHandEffect.SetUpCustomMessage("Select 1 option card to use.", "The opponent is selecting 1 option card to use.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Used Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayOptionCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            root: SelectCardEffect.Root.Hand));                          
                    }

                    if (CardEffectCommons.IsOpponentTurn(card))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(PermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: PermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get Security Attack -1 and effects", "The opponent is selecting 1 Digimon that will get Security Attack -1.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(targetPermanent: permanent, changeValue: -1, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                DisableEffectClass invalidationClass = new DisableEffectClass();
                                invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", CanUseCondition1, card);
                                invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                                selectedPermanent.UntilEachTurnEndEffects.Add((_timing) => invalidationClass);

                                bool CanUseCondition1(Hashtable hashtable)
                                {
                                    return true;
                                }

                                bool InvalidateCondition(ICardEffect cardEffect)
                                {
                                    if (selectedPermanent.TopCard != null)
                                    {
                                        if (cardEffect != null)
                                        {
                                            if (cardEffect.EffectSourceCard != null)
                                            {
                                                if (isExistOnField(cardEffect.EffectSourceCard))
                                                {
                                                    if (cardEffect.EffectSourceCard.PermanentOfThisCard() == selectedPermanent)
                                                    {
                                                        if (cardEffect.IsWhenDigivolving)
                                                        {
                                                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                                            {
                                                                return true;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            #endregion         

            #region Inherit
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DP -4000", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] 1 of your opponent's Digimon gets -4000 DP for the turn.";
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
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
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
                         selectPermanentCoroutine: SelectPermanentCoroutine,
                         afterSelectPermanentCoroutine: null,
                         mode: SelectPermanentEffect.Mode.Custom,
                         cardEffect: activateClass);

                     selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -4000.", "The opponent is selecting 1 Digimon that will get DP -4000.");

                     yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                     IEnumerator SelectPermanentCoroutine(Permanent permanent)
                     {
                         yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -4000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                     }            
                }
            }
            #endregion

            return cardEffects;
        }
    }
}