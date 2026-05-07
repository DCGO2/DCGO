using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

// Junomon
namespace DCGO.CardEffects.BT25
{
    public class BT25_044 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.HasTSTraits
                        || permanent.TopCard.EqualsTraits("Angel")
                        || permanent.TopCard.EqualsTraits("Archangel");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, true, card, null, level: 5));
            }
            #endregion

            #region Reduce Play Cost
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return (card.Owner.SecurityCards.Count + card.Owner.Enemy.SecurityCards.Count) <= 6;
                }

                cardEffects.Add(CardEffectFactory.MandatorySelfPlayCostReduction(5, card, Condition));
            }
            #endregion

            #region Shared OP / WD

            string SharedEffectName = "Place another digimon as top security to trash both players' top security";

            string SharedEffectDescription(string tag)
                => $"[{tag}] By placing 1 other Digimon as the top security card, trash both players' top security cards.";

            bool OtherDigimon(Permanent permanent)
            {
                return permanent != card.PermanentOfThisCard()
                    && CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);
            }

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.HasMatchConditionPermanent(OtherDigimon);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent selectedPermanent = null;
                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: OtherDigimon,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place in security", "The opponent is selecting 1 Digimon place in security.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;
                    yield return null;
                }

                if (selectedPermanent != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.PlacePermanentInSecurityAndProcessAccordingToResult(selectedPermanent, activateClass, toTop: true, SuccessProcess));

                    IEnumerator SuccessProcess(CardSource cardSource)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner.Enemy,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());

                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());
                    }
                }
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
                    whenDigivolving: true);
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play an 8 cost or less [Angel]/[Archangel]/[Iliad] card for free", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("BT25_044_AT");
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When your security stack is removed from, you may play 1 play cost 8 or lower [Angel], [Archangel] or [Iliad] trait card from your hand or trash without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, (player) => player == card.Owner);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCardCondition)
                            || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectTrashCardCondition));
                }

                bool CanSelectCardCondition(CardSource cardSource, SelectCardEffect.Root root)
                {
                    return cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 8
                        && (cardSource.EqualsTraits("Angel") || cardSource.EqualsTraits("Archangel") || cardSource.HasIliadTraits)
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, root);
                }

                bool CanSelectHandCardCondition(CardSource cardSource) => CanSelectCardCondition(cardSource, SelectCardEffect.Root.Hand);

                bool CanSelectTrashCardCondition(CardSource cardSource) => CanSelectCardCondition(cardSource, SelectCardEffect.Root.Trash);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool executed = false;
                    bool validHandCard = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCardCondition);
                    bool validTrashCard = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectTrashCardCondition);

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    if (validHandCard)
                    {
                        selectionElements.Add(new(message: "from Hand", value: 1, spriteIndex: 0));
                    }
                    if (validTrashCard)
                    {
                        selectionElements.Add(new(message: "from Trash", value: 2, spriteIndex: 0));
                    }
                    selectionElements.Add(new(message: "Do not place", value: 3, spriteIndex: 1));

                    GManager.instance.userSelectionManager.SetIntSelection(
                        selectionElements: selectionElements,
                        selectPlayer: card.Owner,
                        selectPlayerMessage: "From which area will you play a card?",
                        notSelectPlayerMessage: "The opponent is choosing from which area to play a card.");

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool doPlay = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                    bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                    if (doPlay)
                    {
                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count > 0)
                                executed = true;
                            yield return null;
                        }

                        if (fromHand)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectHandCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                mode: SelectHandEffect.Mode.PlayForFree,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectTrashCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                message: "Select 1 card to play.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.PlayForFree,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectCardEffect.Activate());
                        }
                    }
                    
                    if(!executed) activateClass.RemoveUse();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
