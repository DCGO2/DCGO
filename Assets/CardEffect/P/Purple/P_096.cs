using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.P
{
    public class P_096 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Color Requirements
            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
                cardEffects.Add(ignoreColorConditionClass);

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.IsTamer)
                        {
                            if (permanent.TopCard.CardTraits.Contains("Hunter"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                    {
                        return true;
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] By placing up to 2 Digimon cards with <Save> in their text from under your Tamers or your trash under 1 of your Digimon with <Save> in its text as its bottom digivolution cards, that Digimon gains +1000 DP for each card placed by this effect.";
                }

                bool CanSelectGetDigivolutionCardsPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasSaveText)
                        {
                            if (!permanent.IsToken)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.GetBattleAreaPermanents().Some(CanSelectGetDigivolutionCardsPermanentCondition))
                    {
                        List<CardSource> digivolutionCards = new List<CardSource>();

                        bool CanSelectDigivolutionCardsCondition(CardSource cardSource)
                        {
                            if (cardSource.IsDigimon)
                            {
                                if (cardSource.HasSaveText)
                                {
                                    if (!digivolutionCards.Contains(cardSource))
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        bool CanSelectTamerCondition(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                            {
                                if (permanent.IsTamer)
                                {
                                    if (permanent.DigivolutionCards.Count(CanSelectDigivolutionCardsCondition) >= 1)
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        bool canSelectTamer() => card.Owner.GetBattleAreaPermanents().Some(CanSelectTamerCondition);
                        bool canSelectTrash() => CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectDigivolutionCardsCondition);

                        int maxSelectCardCount() => 2 - digivolutionCards.Count;

                        bool canSelect() => (canSelectTamer() || canSelectTrash()) && maxSelectCardCount() >= 1;

                        while (canSelect())
                        {
                            bool fromTrash = false;
                            bool noSelect = false;

                            yield return GManager.instance.photonWaitController.StartWait("PrismGallet_Select");

                            if (canSelectTamer() && canSelectTrash())
                            {
                                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                            {
                                new SelectionElement<int>(message: $"Under Tamer", value : 0, spriteIndex: 0),
                                new SelectionElement<int>(message: $"From trash", value : 1, spriteIndex: 0),
                                new SelectionElement<int>(message: $"No Select", value : 2, spriteIndex: 1),
                            };

                                string selectPlayerMessage = "From which area do you select cards?";
                                string notSelectPlayerMessage = "The opponent is choosing from which area to select cards.";

                                GManager.instance.userSelectionManager.SetIntSelection(
                                    selectionElements: selectionElements,
                                    selectPlayer: card.Owner,
                                    selectPlayerMessage: selectPlayerMessage,
                                    notSelectPlayerMessage: notSelectPlayerMessage);

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                                if (GManager.instance.userSelectionManager.SelectedIntValue == 2)
                                {
                                    noSelect = true;
                                }

                                else
                                {
                                    fromTrash = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                                }
                            }

                            else if (!canSelectTamer() && canSelectTrash())
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: $"Select", value : true, spriteIndex: 0),
                                new SelectionElement<bool>(message: $"Not Select", value : false, spriteIndex: 1),
                            };

                                string selectPlayerMessage = "Will you select cards from trash?";
                                string notSelectPlayerMessage = "The opponent is choosing wheter to select cards from trash.";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                                bool doSelect = GManager.instance.userSelectionManager.SelectedBoolValue;

                                if (doSelect)
                                {
                                    fromTrash = true;
                                }

                                else
                                {
                                    noSelect = true;
                                }
                            }

                            else if (canSelectTamer() && !canSelectTrash())
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: $"Select", value : true, spriteIndex: 0),
                                new SelectionElement<bool>(message: $"Not Select", value : false, spriteIndex: 1),
                            };

                                string selectPlayerMessage = "Will you select cards from under Tamer?";
                                string notSelectPlayerMessage = "The opponent is choosing wheter to select cards from under Tamer.";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                                bool doSelect = GManager.instance.userSelectionManager.SelectedBoolValue;

                                if (doSelect)
                                {
                                    fromTrash = false;
                                }

                                else
                                {
                                    noSelect = true;
                                }
                            }

                            else
                            {
                                noSelect = true;
                            }

                            if (noSelect)
                            {
                                break;
                            }

                            else
                            {
                                if (fromTrash)
                                {
                                    if (canSelectTrash())
                                    {
                                        List<CardSource> selectedCards = new List<CardSource>();

                                        int _maxCount = Math.Min(maxSelectCardCount(), CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectDigivolutionCardsCondition));

                                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                        selectCardEffect.SetUp(
                                            canTargetCondition: CanSelectDigivolutionCardsCondition,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: null,
                                            canNoSelect: () => true,
                                            selectCardCoroutine: SelectCardCoroutine,
                                            afterSelectCardCoroutine: null,
                                            message: "Select cards from trash to place in Digivolution cards.",
                                            maxCount: _maxCount,
                                            canEndNotMax: true,
                                            isShowOpponent: true,
                                            mode: SelectCardEffect.Mode.Custom,
                                            root: SelectCardEffect.Root.Trash,
                                            customRootCardList: null,
                                            canLookReverseCard: true,
                                            selectPlayer: card.Owner,
                                            cardEffect: activateClass);

                                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                        selectCardEffect.SetUpCustomMessage(
                                            "Select cards from trash to place in Digivolution cards.",
                                            "The opponent is selecting cards to place in Digivolution cards.");

                                        yield return StartCoroutine(selectCardEffect.Activate());

                                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                                        {
                                            selectedCards.Add(cardSource);
                                            digivolutionCards.Add(cardSource);
                                            yield return null;
                                        }
                                    }
                                }

                                else
                                {
                                    if (canSelectTamer())
                                    {
                                        Permanent selectedPermanent = null;

                                        int _maxCount = 1;

                                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                        selectPermanentEffect.SetUp(
                                            selectPlayer: card.Owner,
                                            canTargetCondition: CanSelectTamerCondition,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: null,
                                            maxCount: 1,
                                            canNoSelect: true,
                                            canEndNotMax: true,
                                            selectPermanentCoroutine: SelectPermanentCoroutine,
                                            afterSelectPermanentCoroutine: null,
                                            mode: SelectPermanentEffect.Mode.Custom,
                                            cardEffect: activateClass);

                                        selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer.", "The opponent is selecting 1 Tamer.");

                                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                        {
                                            selectedPermanent = permanent;

                                            yield return null;
                                        }

                                        if (selectedPermanent != null)
                                        {
                                            if (selectedPermanent.DigivolutionCards.Count(CanSelectDigivolutionCardsCondition) >= 1)
                                            {
                                                _maxCount = Math.Min(
                                                    maxSelectCardCount(),
                                                    selectedPermanent.DigivolutionCards.Count(CanSelectDigivolutionCardsCondition));

                                                List<CardSource> selectedCards = new List<CardSource>();

                                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                                selectCardEffect.SetUp(
                                                            canTargetCondition: CanSelectDigivolutionCardsCondition,
                                                            canTargetCondition_ByPreSelecetedList: null,
                                                            canEndSelectCondition: null,
                                                            canNoSelect: () => true,
                                                            selectCardCoroutine: SelectCardCoroutine,
                                                            afterSelectCardCoroutine: null,
                                                            message: "Select digivolution cards.",
                                                            maxCount: 1,
                                                            canEndNotMax: true,
                                                            isShowOpponent: true,
                                                            mode: SelectCardEffect.Mode.Custom,
                                                            root: SelectCardEffect.Root.Custom,
                                                            customRootCardList: selectedPermanent.DigivolutionCards,
                                                            canLookReverseCard: true,
                                                            selectPlayer: card.Owner,
                                                            cardEffect: activateClass);

                                                selectCardEffect.SetUpCustomMessage("Select digivolution cards.", "The opponent is selecting digivolution cards.");

                                                yield return StartCoroutine(selectCardEffect.Activate());

                                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                                {
                                                    selectedCards.Add(cardSource);
                                                    digivolutionCards.Add(cardSource);
                                                    yield return null;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (digivolutionCards.Count >= 1)
                        {
                            List<CardSource> digivolutionCards_fixed = new List<CardSource>();

                            if (digivolutionCards.Count == 1)
                            {
                                digivolutionCards_fixed = digivolutionCards.Clone();
                            }

                            else
                            {
                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: (cardSource) => true,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                                    message: "Specify the order to place the cards in the digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                                    maxCount: digivolutionCards.Count,
                                    canEndNotMax: false,
                                    isShowOpponent: false,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: digivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetNotShowCard();

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                                {
                                    foreach (CardSource cardSource in cardSources)
                                    {
                                        digivolutionCards_fixed.Add(cardSource);
                                    }

                                    yield return null;
                                }
                            }

                            if (digivolutionCards_fixed.Count >= 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(digivolutionCards_fixed, "Digivolutuion Cards", true, true));

                                if (card.Owner.GetBattleAreaPermanents().Count(CanSelectGetDigivolutionCardsPermanentCondition) >= 1)
                                {
                                    int maxCount = 1;

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectGetDigivolutionCardsPermanentCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage(
                                        "Select 1 Digimon that will get digivolution cards.",
                                        "The opponent is selecting 1 Digimon that will get digivolution cards.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(permanent.AddDigivolutionCardsBottom(
                                            digivolutionCards_fixed, activateClass));

                                        int getDigivolutionCardsCount = digivolutionCards_fixed.Count((cardSource) =>
                                        permanent.DigivolutionCards.Contains(cardSource));

                                        int plusDP = 1000 * getDigivolutionCardsCount;

                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                            targetPermanent: permanent,
                                            changeValue: plusDP,
                                            effectDuration: EffectDuration.UntilEachTurnEnd,
                                            activateClass: activateClass));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Add this card to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Add this card to its owner's hand.";
                }
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}