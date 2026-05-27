using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Siriusmon / Planet Punch
namespace DCGO.CardEffects.EX12
{
    public class EX12_018 : CEntity_Effect
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
                    return permanent.TopCard.HasVBTraits
                            || permanent.TopCard.HasText("Gammamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, true, card, null, level: 5));
            }
            #endregion

            #region Progress
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ProgressSelfStaticEffect(false, card, null));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Security Attack +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared WD / WA

            string SharedEffectName = "May place up to 2 [Gammamon] in text/[VB] trait digimon from hand or trash as top/bottom digivolution card, if placed -2000 DP for each digivolution card to 1 opponent digimon";

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] You may place up to 2 Digimon cards with [Gammamon] in its text or the [VB] trait from your hand or trash as this Digimon's top or bottom digivolution cards. If this effect placed, to 1 of your opponent's Digimon, give -2000 DP until their turn ends for each of this Digimon's digivolution cards.";

            string SharedHashValue = "EX12_018_WDWA";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    hashValue: SharedHashValue,
                    maxCountPerTurn: 1,
                    optional: false,
                    isSkippable: true,
                    whenDigivolving: true,
                    whenAttacking: true);

            bool CanSelectCardCondition(CardSource cardSource)
                => cardSource.IsDigimon &&
                (cardSource.HasVBTraits || cardSource.HasText("Gammamon"));

            bool CanSelectPermamentCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isUsed = false;
                bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                if (canSelectHand || canSelectTrash)
                {
                    List<CardSource> cardSources = new List<CardSource>();

                    while (cardSources < 2)
                    {
                        #region Setup Location Selection
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                        if (canSelectHand) selectionElements.Add(new(message: $"From Hand", value: 1, spriteIndex: 0));
                        if (canSelectTrash) selectionElements.Add(new(message: $"From Trash", value: 2, spriteIndex: 0));
                        selectionElements.Add(new(message: $"Don't place any", value: 3, spriteIndex: 1));

                        string selectPlayerMessage = "Where will you select cards from to place under this Digimon as digivolution sources?";
                        string notSelectPlayerMessage = "The opponent is choosing to up to 2 digimon cards inside this cards digivolution source.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                        #endregion

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 3)
                        {
                            break;
                        }
                        else
                        {
                            int maxCount = Math.Max(2, cardSources.Count);

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                cardSources.Add(cardSource);
                                yield return null;
                            }

                            if (GManager.instance.userSelectionManager.SelectedIntValue == 1)
                            {
                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: true,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectHandEffect.SetUpCustomMessage("Select up to 2 digimon cards to add to source.", "The opponent is selecting 2 digimon cards to add to source.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Selected cards");
                                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
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
                                    message: "Select up to 2 digimon cards",
                                    maxCount: maxCount,
                                    canEndNotMax: true,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage("Select up to 2 digimon cards to add to source.", "The opponent is selecting 2 digimon cards to add to source.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Selected cards");
                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            }
                        }
                    }

                    if (cardSources.Any())
                    {
                        isUsed = true;
                        #region Setup Location Selection
                        List<SelectionElement<bool>> selectionElements1 = new List<SelectionElement<bool>>();

                        if (canSelectHand) selectionElements1.Add(new(message: $"Top of digivolution sources", value: true, spriteIndex: 0));
                        if (canSelectTrash) selectionElements1.Add(new(message: $"Bottom of digivolution sources", value: false, spriteIndex: 0));

                        string selectPlayerMessage1 = "Will you place up to 2 digimon cards inside this cards digivolution source?";
                        string notSelectPlayerMessage1 = "The opponent is choosing to up to 2 digimon cards inside this cards digivolution source.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                        #endregion

                        bool isTop = GManager.instance.userSelectionManager.SelectedBoolValue;
                        if (isTop) yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsTop(
                            addedDigivolutionCards: cardSources.Reverse(),
                            cardEffect: activateClass));
                        else yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                            addedDigivolutionCards: cardSources,
                            cardEffect: activateClass));

                        if (card.PermanentOfThisCard().DigivolutionCards.Intersect(cardSources).Any() && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermamentCondition))
                        {
                            int dpMinus = 2000 * card.PermanentOfThisCard().DigivolutionCards.Count;

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermamentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage($"Select 1 Digimon that will get DP -{dpMinus}.", $"The opponent is selecting 1 Digimon that will get DP -{dpMinus}.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -dpMinus, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                            }
                        }
                    }
                }

                if (!isUsed) activateClass.RemoveUse();
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
                    return cardSource.HasVBTraits;
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 enemy digimon with highest DP, then 1 digimon may attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Delete 1 of your opponent's highest DP Digimon. Then, 1 of your Digimon may attack.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectOpponentPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsMaxDP(permanent, card.Owner.Enemy, null);

                bool CanSelectOwnerPermamentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOpponentPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete", "The opponent is selecting 1 Digimon to delete.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnerPermamentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect =
                        GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: YourDigimonThatCanAttack,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Attack,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will attack.",
                            "The opponent is selecting 1 Digimon that will attack.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
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