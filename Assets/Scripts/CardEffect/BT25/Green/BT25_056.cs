using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT25
{
    // Bootmon
    public class BT25_056 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasSuperAppTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Link Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasAppmonTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 3, card: card));
            }

            #endregion

            #region App Fusion (Logimon & Craftmon)

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.AddAppfuseMethodByName(new List<string>() { "Logimon", "Craftmon" }, card));

            }

            #endregion


            #region Barrier
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP / WD / WA

            string SharedEffectName = "Link from hand or digivolution cards to this for -2";

            string SharedEffectDescription(string tag)
                => $"[{tag}] If it's your turn, you may link 1 [Social], [Tool] or [Game] trait Digimon card from your hand or this Digimon's digivolution cards to this Digimon with the cost reduced by 2.";

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.IsOwnerTurn(card)
                    && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanLinkCardActivateCondition)
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

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    additionalActivateCondition: AdditionalActivateCondition,
                    optional: false,
                    isSkippable: true,
                    onPlay: true,
                    whenDigivolving: true,
                    whenAttacking: true);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Link Cost Reduction
                ICardEffect GetCardEffect(EffectTiming _timing)
                {
                    if (_timing == EffectTiming.None)
                    {
                        return CardEffectFactory.GrantedReduceLinkCostClass(
                            card: card, 
                            reducedCost: 2,
                            cardSourceCondition: _ => true,
                            permanentCondition: _ => true,
                            rootCondition: _ => true
                        );
                    }

                    return null;
                }

                card.Owner.UntilCalculateFixedCostEffect.Add(GetCardEffect);
                #endregion

                bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanLinkCardEffectCondition);
                bool canSelectSources = card.PermanentOfThisCard().DigivolutionCards.Any(CanLinkCardEffectCondition);

                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                if (canSelectHand)
                {
                    selectionElements.Add(new SelectionElement<int>(message: $"From hand", value : 1, spriteIndex: 0));
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
                bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                if (doLink)
                {
                    if (fromHand)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanLinkCardEffectCondition,
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

                        selectHandEffect.SetUpCustomMessage("Select 1 card to link.", "The opponent is selecting 1 card to link.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");

                        yield return StartCoroutine(selectHandEffect.Activate());
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
            
            #endregion

            #region All Turns

            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 enemy Digimon or tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When this Digimon get linked, suspend 1 of your opponent's Digimon or Tamers.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool IsOpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
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

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
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
                activateClass.SetUpICardEffect("Bottom deck 1 of your opponents suspended Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Linking] Return 1 of your opponent's suspended Digimon to the bottom of the deck.";
                }

                bool IsOpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.IsSuspended;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenLinking(hashtable, null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
