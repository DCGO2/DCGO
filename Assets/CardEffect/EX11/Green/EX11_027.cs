using System;
using System.Collections;
using System.Collections.Generic;

// Maquinamon
namespace DCGO.CardEffects.EX11
{
    public class EX11_027 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel2 && targetPermanent.TopCard.HasText("Maquinamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal top 3, Add [Maquinamon] & 1 [Maquinamon] in text, then may link", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[On Play] Reveal the top 3 cards of your deck. Add 1 [Maquinamon] and 1 card with [Maquinamon] in its text among them to the hand. Return the rest to the bottom of the deck. Then, you may link this Digimon or 1 [Maquinamon] in your hand to 1 of your other Digimon without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool IsMaquinamon(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Maquinamon");
                }

                bool HasMaquinamonText(CardSource cardSource)
                {
                    return cardSource.HasText("Maquinamon");
                }

                bool CanMaybeLinkPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent != card.PermanentOfThisCard()
                        && (card.CanLinkToTargetPermanent(permanent, false)
                            || card.Owner.HandCards.Filter(CanSelectCardCondition).Some(cardSource => cardSource.CanLinkToTargetPermanent(permanent, false)));
                }

                bool CanSelectLinkTarget(Permanent permanent, CardSource cardSource)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent != card.PermanentOfThisCard()
                        && cardSource.CanLinkToTargetPermanent(permanent, false);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return IsMaquinamon(cardSource)
                        && cardSource.CanLink(false);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: IsMaquinamon,
                            message: "Select 1 [Maquinamon].",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: HasMaquinamonText,
                            message: "Select 1 card with [Maquinamon] in its text.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass
                    ));

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanMaybeLinkPermanent))
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new (message: $"Link this Maquinamon", value : 1, spriteIndex: 0)
                        };
                        bool canLinkHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                        if (canLinkHand)
                        {
                            selectionElements.Add( new (message: $"Link from Hand", value : 2, spriteIndex: 0));
                        }
                        selectionElements.Add( new (message: $"Don't link", value: 3, spriteIndex: 1));

                        string selectPlayerMessage = canLinkHand ? "From which area will you link a Maquinamon?" : "Will you link your Maquinamon to another Digimon?";
                        string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool doLink = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                        bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 2;
                        if (doLink)
                        {
                            CardSource selectedCard = null;

                            if (fromHand)
                            {
                                int maxCount = 1;

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardCondition,
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

                                selectHandEffect.SetUpCustomMessage("Select 1 card to link.", "The opponent is selecting 1 card to link.");

                                yield return StartCoroutine(selectHandEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCard = cardSource;

                                    yield return null;
                                }
                            }

                            if (selectedCard != null || !fromHand)
                            {
                                Permanent selectedPermanent = null;

                                CardSource linkTarget = selectedCard != null ? selectedCard : card;

                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(permanent => CanSelectLinkTarget(permanent, linkTarget)));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: permanent => CanSelectLinkTarget(permanent, linkTarget),
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to link.", "The opponent is selecting 1 Digimon to link.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    selectedPermanent = permanent;

                                    yield return null;
                                }

                                if (selectedPermanent != null){
                                    if(fromHand)
                                        yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddLinkCard(selectedCard, activateClass));
                                    else
                                        yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToLinkCards(new List<Permanent[]>() { new Permanent[] { card.PermanentOfThisCard(), selectedPermanent } }, activateClass).PlacePermanentToLinkCards());
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Link Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Maquinamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 2, card: card));
            }
            #endregion

            #region Link
            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
            }
            #endregion

            #region Link Effect
            if (timing == EffectTiming.WhenRemoveField)
            {
                List<Permanent> removedPermanents = new List<Permanent>();

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place a linked card into digivolution cards to prevent this digimon from leaving", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] When this Digimon would leave the battle area, by placing 1 of its link cards as its bottom digivolution card, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistLinked(card) &&
                           CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                    
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistLinked(card) &&
                           card.PermanentOfThisCard().LinkedCards.Contains(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();
                    CardSource selectedCard = null;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: _ => true,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 link card to place as bottom digivolution card.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().LinkedCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 link card to place as bottom digivolution card.", "The opponent is selecting 1 link card to place in digivolution cards.");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;

                        yield return null;
                    }

                    if (selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(thisPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass));
                        
                        thisPermanent.willBeRemoveField = false;

                        thisPermanent.HideHandBounceEffect();
                        thisPermanent.HideDeckBounceEffect();
                        thisPermanent.HideWillRemoveFieldEffect();
                        thisPermanent.HideDeleteEffect();
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
