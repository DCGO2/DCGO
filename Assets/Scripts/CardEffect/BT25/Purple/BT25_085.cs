using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// BeelStarMon // Fly Bullet
namespace DCGO.CardEffects.BT25
{
    public class BT25_085 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.HasText("Three Musketeers")
                            || permanent.TopCard.EqualsTraits("TS");
                }
            
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, true, card, null, level: 5));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared WD / WA

            string UseOptionHashString = "BT25_085_WD_WA";

            string UseOptionEffectName = "Use a [Three Musketeers] or [TS] Option from hand or under this digimon for free";

            string UseOptionEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] You may use 1 [Three Musketeers] or [TS] trait Option card from your hand or this Digimon's digivolution cards without paying the cost.";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    UseOptionEffectName,
                    UseOptionActivateCoroutine,
                    UseOptionEffectDescription,
                    optional: false,
                    isSkippable: true,
                    additionalActivateCondition: CanActivateUseOption,
                    maxCountPerTurn: 1,
                    hashValue: UseOptionHashString,
                    whenDigivolving: true,
                    whenAttacking: true);

            bool IsTraitedOption(CardSource cardSource)
            {
                return cardSource.IsOption
                    && (cardSource.EqualsTraits("Three Musketeers")
                        || cardSource.EqualsTraits("TS"))
                    && !cardSource.CanNotPlayThisOption;
            }

            bool CanActivateUseOption(Hashtable hashtable)
            {
                return CardEffectCommons.HasMatchConditionOwnersHand(card, IsTraitedOption)
                    || card.PermanentOfThisCard().DigivolutionCards.Any(IsTraitedOption);
            }

            IEnumerator UseOptionActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool executed = false;
                bool validHandCards = CardEffectCommons.HasMatchConditionOwnersHand(card, IsTraitedOption);
                bool validDigivolutionCards = card.PermanentOfThisCard().DigivolutionCards.Any(IsTraitedOption);

                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                if (validHandCards)
                {
                    selectionElements.Add(new (message: $"Use from Hand", value : 1, spriteIndex: 0));
                }
                if (validDigivolutionCards)
                {
                    selectionElements.Add(new (message: $"Use from Digivolution Cards", value : 2, spriteIndex: 0));
                }
                selectionElements.Add( new (message: "Don't use an Option", value: 3, spriteIndex: 1));

                string selectPlayerMessage = "From which area will you use an Option?";
                string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                bool doUse = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                if (!doUse)
                {
                    if (fromHand)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsTraitedOption,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: cardSource => SelectCardCoroutine(cardSource, SelectCardEffect.Root.Hand),
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select a [Three Musketeers] Option to place use.", "Opponent is selecting a card to use.");

                        yield return StartCoroutine(selectHandEffect.Activate());
                    }
                    else
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: IsTraitedOption,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: cardSource => SelectCardCoroutine(cardSource, SelectCardEffect.Root.DigivolutionCards),
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to place.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select a [Three Musketeers] Option to place use.", "Opponent is selecting a card to use.");

                        yield return StartCoroutine(selectCardEffect.Activate());
                    }

                    IEnumerator SelectCardCoroutine(CardSource cardSource, SelectCardEffect.Root root)
                    {
                        executed = true;
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                cardSources: new List<CardSource> { cardSource },
                                activateClass: activateClass,
                                payCost: false,
                                root: root));
                    }
                }

                if (!executed)
                {
                    activateClass.RemoveUse();
                }
            }

            #endregion

            #region Shared WD / WA / Counter

            string UnsuspendHashString = "BT25_085_WD_WA_Counter";

            string UnsuspendEffectName = "By trashing 1 Option from any of your digivolution cards or link cards, unsuspend";

            string UnsuspendEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] By trashing 1 Option card from any of your Digimon's digivolution cards or link cards, this Digimon unsuspends.";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    UnsuspendEffectName,
                    UnsuspendActivateCoroutine,
                    UnsuspendEffectDescription,
                    optional: false,
                    isSkippable: true,
                    additionalActivateCondition: CanActivateUnsuspend,
                    maxCountPerTurn: 1,
                    hashValue: UnsuspendHashString,
                    whenDigivolving: true,
                    whenAttacking: true,
                    counter: true);

            bool CanTrashCardSource(CardSource cardSource) => cardSource.IsOption;

            bool PermanentWithTrashableCard(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionOrLinkCards.Any(CanTrashCardSource);
            }

            bool CanActivateUnsuspend(Hashtable hashtable) => CardEffectCommons.HasMatchConditionOwnersPermanent(card, PermanentWithTrashableCard);

            IEnumerator UnsuspendActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool executed = false;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: PermanentWithTrashableCard,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select a Digimon to trash a card from", "Opponent is selecting a card to trash a card from");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanTrashCardSource,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: "Select 1 card to place.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Discard,
                        root: SelectCardEffect.Root.DigivolutionCards,//This only sets behaviour, so currently is safe to use for this combined list. Will ensure pending effects show
                        customRootCardList: permanent.DigivolutionOrLinkCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select a [Three Musketeers] card to place under 1 of your Digimon.", "Opponent is selecting a card to place.");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        executed = cardSources.Count > 0;

                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                            new List<Permanent>() { card.PermanentOfThisCard() },
                            activateClass).Unsuspend());
                    }
                }

                if (!executed)
                {
                    activateClass.RemoveUse();
                }
            }

            #endregion

            #endregion 

            #region Option Effects

            #region Ignore Colour Requirement
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.HasText("Three Musketeers");
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 highest level enemy Digimon. Place 1 [Three Musketeers] card from hand or trash under any of your Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Delete 1 of your opponent's highest level Digimon. Then, you may place 1 [Three Musketeer] trait card from your hand or trash as any of your Digimon's bottom digivolution card.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool IsStrongestEnemy(Permanent permanent)
                    => CardEffectCommons.IsMaxLevel(permanent, card.Owner.Enemy);

                bool CanSelectCardCondition(CardSource cardSource) 
                    => cardSource.EqualsTraits("Three Musketeers");

                bool CanSelectPermamentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsStrongestEnemy))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsStrongestEnemy,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if(CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermamentCondition))
                    {
                        bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                        bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                        if (canSelectHand || canSelectTrash)
                        {
                            #region Setup Location Selection
                            List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                            if (canSelectHand)
                            {
                                selectionElements.Add(new(message: $"From hand", value: 1, spriteIndex: 0));
                            }
                            if (canSelectTrash)
                            {
                                selectionElements.Add(new(message: $"From trash", value: 2, spriteIndex: 0));
                            }
                            selectionElements.Add(new(message: $"Do not place a card", value: 3, spriteIndex: 1));

                            string selectPlayerMessage = "From which area will you place a card to digivolution cards?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                            #endregion

                            bool doPlace = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                            bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                            if (doPlace)
                            {
                                if (fromHand)
                                {
                                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                    selectHandEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: 1,
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        mode: SelectHandEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectHandEffect.SetUpCustomMessage("Select a [Three Musketeers] card to place under 1 of your Digimon.", "Opponent is selecting a card to place.");

                                    yield return StartCoroutine(selectHandEffect.Activate());
                                }
                                else
                                {
                                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 card to place.",
                                        maxCount: 1,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                                    selectCardEffect.SetUpCustomMessage("Select a [Three Musketeers] card to place under 1 of your Digimon.", "Opponent is selecting a card to place.");

                                    yield return StartCoroutine(selectCardEffect.Activate());
                                }

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectPermamentCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: 1,
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to place card underneath", "The opponent is selecting 1 digimon to place card underneath");
                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        if (permanent != null)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(permanent.AddDigivolutionCardsBottom(new List<CardSource>() { cardSource }, activateClass));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion

            #endregion

            return cardEffects;
        }
    }
}
