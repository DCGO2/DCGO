using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT12
{
    public class BT12_091 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 card under this Tamer from hand to reduce opponent's Digimon's DP by 2000", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By placing 1 Digimon card with <Save> in its text from your hand under this Tamer, 1 of your opponent's Digimon gets -2000 DP for the turn.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.IsDigimon)
                            {
                                if (cardSource.HasSaveText)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
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

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaPermanents().Contains(card.PermanentOfThisCard()))
                        {
                            if (card.Owner.HandCards.Count >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                        {
                            bool placed = false;

                            int maxCount = 1;

                            List<CardSource> selectedCards = new List<CardSource>();

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: CanEndSelectCondition,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select 1 card to place in Digivolution cards.", "The opponent is selecting 1 card to place in Digivolution cards.");
                            selectHandEffect.SetNotShowCard();

                            yield return StartCoroutine(selectHandEffect.Activate());

                            bool CanEndSelectCondition(List<CardSource> cardSources)
                            {
                                if (maxCount >= 1)
                                {
                                    if (cardSources.Count <= 0)
                                    {
                                        return false;
                                    }
                                }

                                return true;
                            }

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                if (!card.PermanentOfThisCard().IsToken)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));

                                    placed = true;
                                }
                            }

                            if (placed)
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                                {
                                    maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

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

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -2000.", "The opponent is selecting 1 Digimon that will get DP -2000.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -2000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("DigivolutionCost-1_BT12_091");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] If one of your Digimon digivolves into a Digimon card with <Save> in its text, by suspending this Tamer and placing 1 card from under one of your Tamers under that Digimon as one of its digivolution cards, reduce the digivolution cost by 1.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return true;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.IsTamer)
                        {
                            if (permanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card);
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasSaveText)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentCondition, CardCondition))
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
                        if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                        {
                            if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                        Permanent giveDigivolutionCardsTamer = null;
                        List<CardSource> selectedDigivolutionCards = new List<CardSource>();
                        Permanent getDigivolutionCardsDigimon = null;

                        int maxCount = 1;

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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer that has digivolution cards.", "The opponent is selecting 1 Tamer that has digivolution cards.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            giveDigivolutionCardsTamer = permanent;

                            yield return null;
                        }

                        if (giveDigivolutionCardsTamer != null)
                        {
                            if (giveDigivolutionCardsTamer.DigivolutionCards.Count((cardSource) => CanSelectCardCondition(cardSource)) >= 1)
                            {
                                maxCount = 1;

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                            canTargetCondition: CanSelectCardCondition,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: null,
                                            canNoSelect: () => false,
                                            selectCardCoroutine: SelectCardCoroutine,
                                            afterSelectCardCoroutine: null,
                                            message: "Select 1 digivolution card.",
                                            maxCount: maxCount,
                                            canEndNotMax: false,
                                            isShowOpponent: true,
                                            mode: SelectCardEffect.Mode.Custom,
                                            root: SelectCardEffect.Root.Custom,
                                            customRootCardList: giveDigivolutionCardsTamer.DigivolutionCards,
                                            canLookReverseCard: true,
                                            selectPlayer: card.Owner,
                                            cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage("Select 1 digivolution card.", "The opponent is selecting 1 digivolution card.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                yield return StartCoroutine(selectCardEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedDigivolutionCards.Add(cardSource);

                                    yield return null;
                                }

                                bool CanSelectPermanentCondition1(Permanent permanent)
                                {
                                    List<Permanent> Permanents = CardEffectCommons.GetPermanentsFromHashtable(_hashtable);

                                    if (Permanents != null)
                                    {
                                        if (Permanents.Contains(permanent))
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                                            {
                                                if (!permanent.IsToken)
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }

                                    return false;
                                }

                                if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition1) >= 1)
                                {
                                    maxCount = 1;

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectPermanentCondition1,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine1,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get digivolution cards.", "The opponent is selecting 1 Digimon that will get digivolution cards.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                                    {
                                        getDigivolutionCardsDigimon = permanent;

                                        yield return null;
                                    }
                                }

                                if (getDigivolutionCardsDigimon != null)
                                {
                                    if (selectedDigivolutionCards.Count >= 1)
                                    {
                                        foreach (CardSource selectedCard in selectedDigivolutionCards)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(selectedCard, giveDigivolutionCardsTamer));
                                        }

                                        yield return ContinuousController.instance.StartCoroutine(getDigivolutionCardsDigimon.AddDigivolutionCardsBottom(selectedDigivolutionCards, activateClass));

                                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                                        ChangeCostClass changeCostClass = new ChangeCostClass();
                                        changeCostClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition1, card);
                                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                                        card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                                        bool CanUseCondition1(Hashtable hashtable)
                                        {
                                            return true;
                                        }

                                        int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                        {
                                            if (CardSourceCondition(cardSource))
                                            {
                                                if (RootCondition(root))
                                                {
                                                    if (PermanentsCondition(targetPermanents))
                                                    {
                                                        Cost -= 1;
                                                    }
                                                }
                                            }

                                            return Cost;
                                        }

                                        bool PermanentsCondition(List<Permanent> targetPermanents)
                                        {
                                            if (targetPermanents != null)
                                            {
                                                if (targetPermanents.Count(PermanentCondition) >= 1)
                                                {
                                                    return true;
                                                }
                                            }

                                            return false;
                                        }

                                        bool PermanentCondition(Permanent targetPermanent)
                                        {
                                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card);
                                        }

                                        bool CardSourceCondition(CardSource cardSource)
                                        {
                                            return cardSource.HasSaveText;
                                        }

                                        bool RootCondition(SelectCardEffect.Root root)
                                        {
                                            return true;
                                        }

                                        bool isUpDown()
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            return cardEffects;
        }
    }
}