using System;
using System.Collections;
using System.Collections.Generic;

// Divine Arms Version (Omega)
namespace DCGO.CardEffects.BT25
{
    public class BT25_101 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Color Requirement
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.HasTSTraits;
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(
                    card: card,
                    cardEffects: ref cardEffects,
                    effectName: "[Security] By trashing 1 [TS] trait card from your hand, <Draw 2>, After you may link this card or 1 [TS] trait card from your trash to 1 of your Digimon on the field without paying the cost.");
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 1 [TS] trait card from hand, Draw 2, then you may link this card or 1 [TS] trait card in trash to 1 of your digimon without paying cost.", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] By trashing 1 [TS] trait card from your hand, <Draw 2> After, you may link this card or 1 [TS] trait card from your trash to 1 of your Digimon on the field without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool IsTsTraitCard(CardSource cardSource)
                {
                    return cardSource.HasTSTraits;
                }

                bool PermanentThisCardCanLinkToCondition(Permanent permanent)
                {
                    return permanent.IsDigimon
                        && CardEffectCommons.IsOwnerPermanent(permanent, card)
                        && card.CanLinkToTargetPermanent(permanent, false, true);
                }

                bool CanLinkTrashCardCondition(CardSource cardSource)
                {
                    return cardSource.HasTSTraits
                        && cardSource.CanLink(false, true);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, IsTsTraitCard))
                    {
                        CardSource selectedCardToTrash = null;

                        #region Select Hand Card to Trash

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, IsTsTraitCard));

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsTsTraitCard,
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

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCardToTrash = cardSource;
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                        #endregion

                        if (selectedCardToTrash != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashHandAndProcessAccordingToResult(
                                player: card.Owner,
                                hashtable: hashtable,
                                cardToTrash: selectedCardToTrash,
                                activateClass: activateClass,
                                successProcess: SuccessProcess,
                                failureProcess: null
                            ));

                            IEnumerator SuccessProcess(CardSource cardSource)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());

                                bool canSelectThisCard = CardEffectCommons.HasMatchConditionPermanent(PermanentThisCardCanLinkToCondition) || CardEffectCommons.HasMatchConditionOwnersBreedingPermanent(card, PermanentThisCardCanLinkToCondition);
                                bool canSelectTrashCard = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanLinkTrashCardCondition);

                                if (canSelectThisCard || canSelectTrashCard)
                                {
                                    #region Setup Location Selection
                                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                                    if (canSelectThisCard) selectionElements.Add(new(message: $"This Card", value: 1, spriteIndex: 0));
                                    if (canSelectTrashCard) selectionElements.Add(new(message: $"[TS] Trait card from trash", value: 2, spriteIndex: 0));
                                    selectionElements.Add(new(message: $"None", value: 3, spriteIndex: 1));

                                    string selectPlayerMessage = "Will you link a card to 1 digimon on your field?";
                                    string notSelectPlayerMessage = "The opponent is choosing to link a card to 1 digimon on their field.";

                                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                                    #endregion

                                    bool doLink = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                                    bool selectThisCard = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                                    if (doLink)
                                    {
                                        Permanent selectedPermament = null;
                                        CardSource selectedCardToLink = null;

                                        IEnumerator SelectPermamentToLinkToCoroutine(Permanent permanent)
                                        {
                                            selectedPermament = permanent;
                                            yield return null;
                                        }

                                        if (selectThisCard)
                                        {
                                            selectedCardToLink = card;
                                        }
                                        else
                                        {
                                            IEnumerator SelectCardToLinkCoroutine(CardSource cardSource)
                                            {
                                                selectedCardToLink = cardSource;
                                                yield return null;
                                            }

                                            #region Select Trash Card to Link
                                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                            selectCardEffect.SetUp(
                                                canTargetCondition: CanLinkTrashCardCondition,
                                                canTargetCondition_ByPreSelecetedList: null,
                                                canEndSelectCondition: null,
                                                canNoSelect: () => true,
                                                selectCardCoroutine: SelectCardToLinkCoroutine,
                                                afterSelectCardCoroutine: null,
                                                message: "Select 1 [TS] trait card to link",
                                                maxCount: 1,
                                                canEndNotMax: false,
                                                isShowOpponent: true,
                                                mode: SelectCardEffect.Mode.Custom,
                                                root: SelectCardEffect.Root.Trash,
                                                customRootCardList: null,
                                                canLookReverseCard: true,
                                                selectPlayer: card.Owner,
                                                cardEffect: activateClass);

                                            selectCardEffect.SetUpCustomMessage("Select 1 [TS] trait card from your trash to link.", "The opponent is selecting 1 [TS] trait card from their trash to link.");
                                            selectCardEffect.SetUpCustomMessage_ShowCard("Selected card");
                                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                                            #endregion
                                        }

                                        bool CanLinkChosenCardCondition(Permanent permanent)
                                        {
                                            return permanent.IsDigimon
                                            && CardEffectCommons.IsOwnerPermanent(permanent, card)
                                            && cardSource.CanLinkToTargetPermanent(permanent, false, true);
                                        }

                                        #region Select Digimon that will gain selected card as link
                                        if (selectedCardToLink != null)
                                        {
                                            if (CardEffectCommons.HasMatchConditionPermanent(CanLinkChosenCardCondition, true))
                                            {
                                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                                selectPermanentEffect.SetUp(
                                                    selectPlayer: card.Owner,
                                                    canTargetCondition: CanLinkChosenCardCondition,
                                                    canTargetCondition_ByPreSelecetedList: null,
                                                    canEndSelectCondition: null,
                                                    maxCount: 1,
                                                    canNoSelect: true,
                                                    canEndNotMax: false,
                                                    selectPermanentCoroutine: SelectPermamentToLinkToCoroutine,
                                                    afterSelectPermanentCoroutine: null,
                                                    mode: SelectPermanentEffect.Mode.Custom,
                                                    cardEffect: activateClass);

                                                selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to gain link.", "The opponent is selecting 1 Digimon to gain link.");
                                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                                            }
                                            #endregion
                                        }

                                        if (selectedPermament != null && selectedCardToLink != null) yield return ContinuousController.instance.StartCoroutine(selectedPermament.AddLinkCard(selectedCardToLink, activateClass));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Link Effects
            #region Link Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Vulcanusmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 3, card: card));
            }

            #endregion

            #region Link
            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
            }
            #endregion

            #region Sec Atk +1/Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null, isLinkedEffect: true));

                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null, isLinkedEffect: true));
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 link card to not leave battle field", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When this [Vulcanusmon] would leave the battle area, by trashing 1 of its link cards, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistLinked(card) &&
                           CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card) &&
                           card.PermanentOfThisCard().TopCard.EqualsCardName("Vulcanusmon");
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistLinked(card) &&
                           !card.PermanentOfThisCard().HasNoLinkCards;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return true;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedLinkCard = null;
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    #region Select Link Card to Trash
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: SelectCardCoroutine,
                        message: "Select 1 link card to trash.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.LinkedCards,
                        customRootCardList: thisPermanent.LinkedCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    IEnumerator SelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count > 0)
                        {
                            selectedLinkCard = cardSources[0];
                            yield return null;
                        }
                        yield return null;
                    }

                    selectCardEffect.SetUpCustomMessage("Select 1 link card to trash.", "The opponent is selecting 1 link card to trash.");
                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    #endregion

                    if (selectedLinkCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashLinkCardsAndProcessAccordingToResult(
                            targetPermanent: thisPermanent,
                            targetLinkCards: new List<CardSource>() { selectedLinkCard },
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                        IEnumerator SuccessProcess(List<CardSource> cardSources)
                        {
                            thisPermanent.willBeRemoveField = false;

                            thisPermanent.HideDeleteEffect();
                            thisPermanent.HideHandBounceEffect();
                            thisPermanent.HideDeckBounceEffect();
                            thisPermanent.HideWillRemoveFieldEffect();

                            yield return null;
                        }
                    }

                }
            }
            #endregion
            #endregion

            return cardEffects;
        }
    }
}
