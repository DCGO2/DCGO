using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.BT17
{
    public class BT17_075 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return (targetPermanent.TopCard.EqualsCardName("Eosmon") && targetPermanent.TopCard.Level == 4);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }
            #endregion 

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play Tamer then De-Digivolve 1 on play", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return
                        "[On Play] Your opponent may play 1 Tamer card from their hand without paying the cost. If they don't, you may play 1 white Tamer card with a play cost of 4 or less from your hand without paying the cost. Then, <De-Digivolve1> 1 of your opponent's Digimon for every 2 Tamers.";
                }

                bool CanSelectTamerOpponentCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false,
                            cardEffect: activateClass,
                            root: SelectCardEffect.Root.Hand, isBreedingArea: false, isPlayOption: false))
                        if (cardSource.IsTamer)
                            return true;

                    return false;
                }

                bool CanSelectWhiteTamerCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false,
                            cardEffect: activateClass,
                            root: SelectCardEffect.Root.Hand, isBreedingArea: false, isPlayOption: false))
                    {
                        if (cardSource.IsTamer)
                            if (cardSource.CardColors.Contains(CardColor.White) && cardSource.GetCostItself <= 4)
                                return true;
                    }

                    return false;
                }

                bool CheckForTamersOnFieldCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                    {
                        if (permanent.IsTamer)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentDigimonCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return (CardEffectCommons.CanTriggerOnPlay(hashtable, card));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return (CardEffectCommons.IsExistOnBattleAreaDigimon(card));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool hasSelectedTamerOpponent = false;

                    #region Opponent has tamer to play

                    if (card.Owner.Enemy.HandCards.Count(CanSelectTamerOpponentCondition) > 0)
                    {
                        List<CardSource> selectedCardsOpponent = new List<CardSource>();

                        int maxCount = Math.Min(1, card.Owner.Enemy.HandCards.Count(CanSelectTamerOpponentCondition));
                        

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

                        selectHandEffectOpponent.SetUpCustomMessage("Select 1 Tamer to play.",
                            "The opponent is selecting 1 Tamer to play.");
                        selectHandEffectOpponent.SetUpCustomMessage_ShowCard("Played Card");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffectOpponent.Activate());

                        IEnumerator SelectCardCoroutineOpponent(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                selectedCardsOpponent.Add(cardSource);
                                hasSelectedTamerOpponent = true;
                            }

                            yield return null;
                        }

                        if (hasSelectedTamerOpponent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.PlayPermanentCards(
                                    cardSources: selectedCardsOpponent,
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Hand,
                                    activateETB: true));
                        }
                    }
                    #endregion

                    #region I play tamer
                    if(!hasSelectedTamerOpponent)
                    {
                        if (card.Owner.HandCards.Count > 0)
                        {
                            List<CardSource> selectedCards = new List<CardSource>();
                            bool hasSelectedWhiteTamer = false;

                            int maxCount = Math.Min(1, card.Owner.HandCards.Count(CanSelectWhiteTamerCondition));

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectWhiteTamerCondition,
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

                            selectHandEffect.SetUpCustomMessage("Select 1 card to play.",
                                "The opponent is selecting 1 card to play.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);
                                hasSelectedWhiteTamer = true;

                                yield return null;
                            }

                            if (hasSelectedWhiteTamer)
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    CardEffectCommons.PlayPermanentCards(
                                        cardSources: selectedCards,
                                        activateClass: activateClass,
                                        payCost: false,
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Hand,
                                        activateETB: true));
                            }
                        }
                    }
                    #endregion

                    #region De Digivolve
                    //then De-Digivolve statement
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentDigimonCondition))
                    {
                        int totalTamers = CardEffectCommons.MatchConditionPermanentCount(CheckForTamersOnFieldCondition);
                        int maxCountDeDigivolve = Math.Max(0, Mathf.FloorToInt(totalTamers / 2));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentDigimonCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select Digimons to De-Digivolve.", "The opponent is selecting Digimons to De-Digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                for(int i = 0; i < maxCountDeDigivolve; i++)
                                    yield return ContinuousController.instance.StartCoroutine(new IDegeneration(selectedPermanent, 1, activateClass).Degeneration());
                            }
                        }
                    }
                    #endregion
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play Tamer then De-Digivolve 1 on play", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return
                        "[When Digivolving] Your opponent may play 1 Tamer card from their hand without paying the cost. If they don't, you may play 1 white Tamer card with a play cost of 4 or less from your hand without paying the cost. Then, <De-Digivolve1> 1 of your opponent's Digimon for every 2 Tamers.";
                }

                bool CanSelectTamerOpponentCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false,
                            cardEffect: activateClass,
                            root: SelectCardEffect.Root.Hand, isBreedingArea: false, isPlayOption: false))
                        if (cardSource.IsTamer)
                            return true;

                    return false;
                }

                bool CanSelectWhiteTamerCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false,
                            cardEffect: activateClass,
                            root: SelectCardEffect.Root.Hand, isBreedingArea: false, isPlayOption: false))
                    {
                        if (cardSource.IsTamer)
                            if (cardSource.CardColors.Contains(CardColor.White) && cardSource.GetCostItself <= 4)
                                return true;
                    }

                    return false;
                }

                bool CheckForTamersOnFieldCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                    {
                        if (permanent.IsTamer)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentDigimonCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return (CardEffectCommons.IsExistOnBattleAreaDigimon(card));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool hasSelectedTamerOpponent = false;

                    #region Opponent has tamer to play

                    if (card.Owner.Enemy.HandCards.Count(CanSelectTamerOpponentCondition) > 0)
                    {
                        List<CardSource> selectedCardsOpponent = new List<CardSource>();

                        int maxCount = Math.Min(1, card.Owner.Enemy.HandCards.Count(CanSelectTamerOpponentCondition));


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

                        selectHandEffectOpponent.SetUpCustomMessage("Select 1 Tamer to play.",
                            "The opponent is selecting 1 Tamer to play.");
                        selectHandEffectOpponent.SetUpCustomMessage_ShowCard("Played Card");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffectOpponent.Activate());

                        IEnumerator SelectCardCoroutineOpponent(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                selectedCardsOpponent.Add(cardSource);
                                hasSelectedTamerOpponent = true;
                            }

                            yield return null;
                        }

                        if (hasSelectedTamerOpponent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.PlayPermanentCards(
                                    cardSources: selectedCardsOpponent,
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Hand,
                                    activateETB: true));
                        }
                    }
                    #endregion

                    #region I play tamer
                    if (!hasSelectedTamerOpponent)
                    {
                        if (card.Owner.HandCards.Count > 0)
                        {
                            List<CardSource> selectedCards = new List<CardSource>();
                            bool hasSelectedWhiteTamer = false;

                            int maxCount = Math.Min(1, card.Owner.HandCards.Count(CanSelectWhiteTamerCondition));

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectWhiteTamerCondition,
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

                            selectHandEffect.SetUpCustomMessage("Select 1 card to play.",
                                "The opponent is selecting 1 card to play.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);
                                hasSelectedWhiteTamer = true;

                                yield return null;
                            }

                            if (hasSelectedWhiteTamer)
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    CardEffectCommons.PlayPermanentCards(
                                        cardSources: selectedCards,
                                        activateClass: activateClass,
                                        payCost: false,
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Hand,
                                        activateETB: true));
                            }
                        }
                    }
                    #endregion

                    #region De Digivolve
                    //then De-Digivolve statement
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentDigimonCondition))
                    {
                        int totalTamers = CardEffectCommons.MatchConditionPermanentCount(CheckForTamersOnFieldCondition);
                        int maxCountDeDigivolve = Math.Max(0, Mathf.FloorToInt(totalTamers / 2));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentDigimonCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select Digimons to De-Digivolve.", "The opponent is selecting Digimons to De-Digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                for (int i = 0; i < maxCountDeDigivolve; i++)
                                    yield return ContinuousController.instance.StartCoroutine(new IDegeneration(selectedPermanent, 1, activateClass).Degeneration());
                            }
                        }
                    }
                    #endregion
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
                activateClass.SetHashString("Eosmon_SwitchTarget_BT17_075");
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