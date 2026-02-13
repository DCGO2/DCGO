using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT17
{
    public class BT17_074 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Morphomon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }
            #endregion 
            
            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play white tamer or Eosmon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If it's your turn, you may play 1 white Tamer with a play cost of 4 or less or 1 level 5 or lower [Eosmon] from your hand by paying 2 cost. If this effect played, your opponent may play 1 Tamer card from their hand without paying the cost.";
                }

                bool CanSelectTamerOrEosmonCondition(CardSource cardSource)
                {
                     if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: true, cardEffect: activateClass, 
                                root: SelectCardEffect.Root.Hand, isBreedingArea: false, isPlayOption: false, fixedCost: 2))
                        {
                            if (cardSource.IsTamer && cardSource.CardColors.Contains(CardColor.White) && cardSource.GetCostItself <= 4)
                                return true;

                            if (cardSource.IsDigimon && cardSource.Level <= 5 && cardSource.EqualsCardName("Eosmon"))
                                return true;
                        }

                     return false;
                }

                bool CanSelectTamerOpponentCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass, root: SelectCardEffect.Root.Hand))
                        if (cardSource.IsTamer)
                            return true;
                    
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return (CardEffectCommons.IsExistOnBattleArea(card) && CardEffectCommons.IsOwnerTurn(card));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Count(CanSelectTamerOrEosmonCondition) >= 1)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 1;
                        bool hasSelectedCard = false;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectTamerOrEosmonCondition,
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
                            hasSelectedCard = true;

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards, 
                            activateClass: activateClass, 
                            payCost: true, 
                            isTapped: false, 
                            root: SelectCardEffect.Root.Hand, 
                            activateETB: true, 
                            fixedCost: 2));

                        if (hasSelectedCard && card.Owner.Enemy.HandCards.Count > 0)
                        {
                            List<CardSource> selectedCardsOpponent = new List<CardSource>();

                            SelectHandEffect selectHandEffectOpponent = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffectOpponent.SetUp(
                                selectPlayer: card.Owner.Enemy,
                                canTargetCondition: CanSelectTamerOpponentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutineOpponent,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);
                            
                            selectHandEffectOpponent.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                            selectHandEffectOpponent.SetUpCustomMessage_ShowCard("Played Card");

                            yield return StartCoroutine(selectHandEffectOpponent.Activate());

                            IEnumerator SelectCardCoroutineOpponent(CardSource cardSource)
                            {
                                selectedCardsOpponent.Add(cardSource);

                                yield return null;
                            }
                            
                            if(selectedCardsOpponent.Count > 0)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: selectedCardsOpponent,
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Hand,
                                    activateETB: true));
                            }
                        }
                    }
                }
            }

            #endregion
            
            #region Inherited Effect - Opponent's Turn

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Switch attack target", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true,
                    EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Eosmon_SwitchTarget_BT17_074");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return (
                        "[Opponent's Turn] [Once Per Turn] When an opponent's Digimon attacks, you may switch the attack target to 1 of your [Eosmon].");
                }

                bool IsOpponentAttacking(Permanent permanent)
                {
                    return CardEffectCommons.IsOpponentPermanent(permanent, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        if (permanent.TopCard.EqualsCardName("Eosmon"))
                            return true;

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, IsOpponentAttacking);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                        return CardEffectCommons.IsOpponentTurn(card);

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
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
                        
                        selectPermanentEffect.SetUpCustomMessage("Select 1 card to be targeted by opponent's attack.", "Opponent is selecting a card to be targeted.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        
                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(
                                activateClass,
                                false,
                                permanent));
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}