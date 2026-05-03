using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT25
{
    // Logamon
    public class BT25_070 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasStandardAppTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Link Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasAppmonTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 2, card: card));
            }
            #endregion

            #region App Fusion (Onmon & Gatchmon)
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.AddAppfuseMethodByName(new List<string>() { "Offmon", "Hackmon" }, card));
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Link for -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] [Once Per Turn] You may link 1 [Social], [Tool] or [Game] trait Digimon card from your trash or this Digimon's digivolution cards to this Digimon with the cost reduced by 1.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanLinkCardActivateCondition)
                            || card.PermanentOfThisCard().DigivolutionCards.Any(CanLinkCardActivateCondition));
                }

                bool CanLinkCardActivateCondition(CardSource cardSource) => CanLinkCardCondition(cardSource, false);

                bool CanLinkCardEffectCondition(CardSource cardSource) => CanLinkCardCondition(cardSource, true);

                bool CanLinkCardCondition(CardSource cardSource, bool payCost)
                {
                    return cardSource.IsDigimon
                        && (cardSource.EqualsTraits("Social")
                            || cardSource.EqualsTraits("Tool")
                            || cardSource.EqualsTraits("Game"))
                        && cardSource.CanLinkToTargetPermanent(card.PermanentOfThisCard(), payCost);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region Link Cost Reduction
                    ICardEffect GetCardEffect(EffectTiming _timing)
                    {
                        if (_timing == EffectTiming.None)
                        {
                            return CardEffectFactory.GrantedReduceLinkCostClass(
                                card: card, 
                                reducedCost: 1,
                                cardSourceCondition: _ => true,
                                permanentCondition: _ => true,
                                rootCondition: _ => true
                            );
                        }

                        return null;
                    }

                    card.Owner.UntilCalculateFixedCostEffect.Add(GetCardEffect);
                    #endregion

                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanLinkCardEffectCondition);
                    bool canSelectSources = card.PermanentOfThisCard().DigivolutionCards.Any(CanLinkCardEffectCondition);

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    if (canSelectTrash)
                    {
                        selectionElements.Add(new SelectionElement<int>(message: $"From trash", value : 1, spriteIndex: 0));
                    }
                    if (canSelectSources)
                    {
                        selectionElements.Add(new SelectionElement<int>(message: $"From digivolution cards", value : 2, spriteIndex: 0));
                    }
                    selectionElements.Add(new SelectionElement<int>(message: $"Do not Link", value : 3, spriteIndex: 1));

                    string selectPlayerMessage = "From which area will you link a card?";
                    string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool doLink = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                    bool fromTrash = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                    if (doLink)
                    {
                        if (fromTrash)
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanLinkCardEffectCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to add as source.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 card to link.", "The opponent is selecting 1 card to link.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanLinkCardEffectCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to add as source.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.DigivolutionCards,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 card to link.", "The opponent is selecting 1 card to link.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new ILinkCard(true, cardSource, card.PermanentOfThisCard(), activateClass).LinkCard());
                        }
                    }

                    #region Remove Link Cost Reduction
                    card.Owner.UntilCalculateFixedCostEffect.Remove(GetCardEffect);
                    #endregion
                }
            }
            #endregion

            #region Your Turns

            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 enemy 4 cost or less Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Your Turn] [Once Per Turn] When this Digimon gets linked, delete 1 of your opponent's Digimon with a play cost of 4 or less.";

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenLinked(hashtable, PermanentCondition, null);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasPlayCost
                        && permanent.TopCard.GetCostItself <= 4;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
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
                }
            }

            #endregion

            #region Link
            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
            }
            #endregion

            #region When Linking

            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 enemy Digimon or Tamers can't unsuspend until their turn ends", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Linking] 1 of your opponent's Digimon or Tamers can't unsuspend until their turn ends.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenLinking(hashtable, null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspend(
                                targetPermanent: permanent,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                condition: null,
                                effectName: "Can't unsuspend"
                            ));
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
